using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class UnifiedMeshSplitter : MonoBehaviour
{
    [Header("Matériaux")]
    public Material cutPieceMaterial;

    [Header("Paramètres de Découpe 2D")]
    [Tooltip("Tolérance (en % de la taille du pavé) sous laquelle un trait est collé au bord.")]
    public float edgeSnapMarginRatio = 0.04f;
    [Tooltip("Distance de dépassement à l'extérieur du pavé pour garantir une coupe franche.")]
    public float edgeExtensionRatio = 0.05f;

    [Header("Débogage Visuel")]
    public bool debugShowCuttingSolid = false;
    public float debugSolidOpacity = 0.5f;

    public bool PerformCut(List<Vector3> worldPoints, List<Vector3> worldNormals)
    {
        if (worldPoints == null || worldPoints.Count < 3) return false;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return false;

        List<Vector3> localPoints = new List<Vector3>();
        List<Vector3> localNormals = new List<Vector3>();

        for (int i = 0; i < worldPoints.Count; i++)
        {
            localPoints.Add(transform.InverseTransformPoint(worldPoints[i]));
            localNormals.Add(transform.InverseTransformDirection(worldNormals[i]).normalized);
        }

        Bounds bounds = mf.sharedMesh.bounds;
        Mesh cuttingSolid = CuttingSolidBuilder.CreateCuttingSolid(localPoints, localNormals, bounds, out CutType cutType);

        if (cuttingSolid == null) return false;

        if (debugShowCuttingSolid)
        {
            ShowDebugSolid(cuttingSolid, cutType);
            return true;
        }

        return ExecuteUnifiedSplit(mf.sharedMesh, localPoints, localNormals, cutType);
    }

    private bool ExecuteUnifiedSplit(Mesh sourceMesh, List<Vector3> localPoints, List<Vector3> localNormals, CutType cutType)
    {
        List<Triangle3D> paveTriangles = ExtractTriangles(sourceMesh);
        List<Triangle3D> outsideTris = new List<Triangle3D>();
        List<Triangle3D> insideTris = new List<Triangle3D>();
        List<Triangle3D> seamWalls = new List<Triangle3D>();

        Bounds bounds = sourceMesh.bounds;

        if (cutType == CutType.SingleFaceExtrusion)
        {
            Vector3 avgNormal = GetAverageNormal(localNormals);
            Vector3 axis = GetPrimaryAxis(avgNormal);

            float snapMargin = bounds.size.magnitude * edgeSnapMarginRatio;
            float extension = bounds.size.magnitude * edgeExtensionRatio;
            List<Vector3> processedPoints = SnapPointsToFaceEdges(localPoints, bounds, axis, snapMargin, extension);

            List<Vector2> poly2D = ProjectTo2D(processedPoints, axis);
            List<Plane> edgePlanes = BuildSegmentPlanes2D(processedPoints, axis);

            List<Triangle3D> slicedTris = SubdivideTrianglesByPlanes(paveTriangles, edgePlanes);

            foreach (var tri in slicedTris)
            {
                Vector3 center3D = (tri.a + tri.b + tri.c) / 3f;
                Vector2 center2D = ProjectSingleTo2D(center3D, axis);

                if (IsPointInPolygon2D(center2D, poly2D)) insideTris.Add(tri);
                else outsideTris.Add(tri);
            }

            // Génération des parois de tunnel strictement restreintes au volume du pavé
            seamWalls = BuildVerticalSideWalls(processedPoints, axis, bounds);
        }
        else if (cutType == CutType.ObliquePlaneCut)
        {
            Vector3 center = Vector3.zero;
            foreach (Vector3 p in localPoints) center += p;
            center /= localPoints.Count;

            Vector3 planeNormal = ComputeLoopNormal(localPoints, center);
            Plane cutPlane = new Plane(planeNormal, center);

            foreach (var tri in paveTriangles)
            {
                SplitTriangleByPlane(tri, cutPlane, out List<Triangle3D> front, out List<Triangle3D> back);
                outsideTris.AddRange(front);
                insideTris.AddRange(back);
            }

            seamWalls = BuildPlanarCap(cutPlane, bounds);
        }
        else // General3DPath
        {
            Vector3 center = Vector3.zero;
            foreach (Vector3 p in localPoints) center += p;
            center /= localPoints.Count;

            Vector3 loopNorm = ComputeLoopNormal(localPoints, center);

            List<Plane> segmentPlanes = BuildPathSegmentPlanes(localPoints, localNormals);
            List<Triangle3D> slicedTris = SubdivideTrianglesByPlanes(paveTriangles, segmentPlanes);

            foreach (var tri in slicedTris)
            {
                Vector3 triCenter = (tri.a + tri.b + tri.c) / 3f;
                if (IsPointIn3DPolygonProjection(triCenter, localPoints, loopNorm))
                {
                    insideTris.Add(tri);
                }
                else
                {
                    outsideTris.Add(tri);
                }
            }

            List<int> seamIndices = Triangulate3DLoopDirect(localPoints);
            for (int i = 0; i < seamIndices.Count; i += 3)
            {
                seamWalls.Add(new Triangle3D(
                    localPoints[seamIndices[i]],
                    localPoints[seamIndices[i + 1]],
                    localPoints[seamIndices[i + 2]]
                ));
            }
        }

        if (insideTris.Count == 0) return false;

        Vector3 pathCenter = Vector3.zero;
        foreach (var p in localPoints) pathCenter += p;
        pathCenter /= localPoints.Count;

        Mesh mainMesh = AssembleMeshWithOrientation(outsideTris, seamWalls, pathCenter, isMainPavé: true, "MainPavéMesh");
        Mesh pieceMesh = AssembleMeshWithOrientation(insideTris, seamWalls, pathCenter, isMainPavé: false, "CutPieceMesh");

        ApplySplitToScene(mainMesh, pieceMesh, cutType);
        return true;
    }

    // =================================================================
    // PAROIS INTÉRIEURES ROGNÉES AUX BORDURES DU PAVÉ
    // =================================================================

    private List<Triangle3D> BuildVerticalSideWalls(List<Vector3> points, Vector3 axis, Bounds bounds)
    {
        List<Triangle3D> walls = new List<Triangle3D>();
        float minDepth = GetDepth(bounds, axis, false);
        float maxDepth = GetDepth(bounds, axis, true);

        float minU = 0f, maxU = 0f, minV = 0f, maxV = 0f;
        if (Mathf.Abs(axis.x) > 0.5f)
        {
            minU = bounds.min.z; maxU = bounds.max.z;
            minV = bounds.min.y; maxV = bounds.max.y;
        }
        else if (Mathf.Abs(axis.y) > 0.5f)
        {
            minU = bounds.min.x; maxU = bounds.max.x;
            minV = bounds.min.z; maxV = bounds.max.z;
        }
        else
        {
            minU = bounds.min.x; maxU = bounds.max.x;
            minV = bounds.min.y; maxV = bounds.max.y;
        }

        int count = points.Count;
        for (int i = 0; i < count; i++)
        {
            Vector3 p1 = points[i];
            Vector3 p2 = points[(i + 1) % count];

            float u1, v1, u2, v2;
            if (Mathf.Abs(axis.x) > 0.5f)
            {
                u1 = p1.z; v1 = p1.y;
                u2 = p2.z; v2 = p2.y;
            }
            else if (Mathf.Abs(axis.y) > 0.5f)
            {
                u1 = p1.x; v1 = p1.z;
                u2 = p2.x; v2 = p2.z;
            }
            else
            {
                u1 = p1.x; v1 = p1.y;
                u2 = p2.x; v2 = p2.y;
            }

            // Rognage 2D du segment aux bordures [minU, maxU] x [minV, maxV]
            if (ClipSegment2D(u1, v1, u2, v2, minU, maxU, minV, maxV, out float cu1, out float cv1, out float cu2, out float cv2))
            {
                Vector3 f1 = ForceDepthOnAxis2D(cu1, cv1, axis, maxDepth);
                Vector3 f2 = ForceDepthOnAxis2D(cu2, cv2, axis, maxDepth);
                Vector3 b1 = ForceDepthOnAxis2D(cu1, cv1, axis, minDepth);
                Vector3 b2 = ForceDepthOnAxis2D(cu2, cv2, axis, minDepth);

                walls.Add(new Triangle3D(f1, b1, f2));
                walls.Add(new Triangle3D(f2, b1, b2));
            }
        }
        return walls;
    }

    private bool ClipSegment2D(float u1, float v1, float u2, float v2, float minU, float maxU, float minV, float maxV, out float cu1, out float cv1, out float cu2, out float cv2)
    {
        cu1 = u1; cv1 = v1;
        cu2 = u2; cv2 = v2;

        float du = u2 - u1;
        float dv = v2 - v1;

        float[] p = new float[] { -du, du, -dv, dv };
        float[] q = new float[] { u1 - minU, maxU - u1, v1 - minV, maxV - v1 };

        float t0 = 0.0f;
        float t1 = 1.0f;

        for (int i = 0; i < 4; i++)
        {
            if (Mathf.Abs(p[i]) < 0.00001f)
            {
                if (q[i] < 0) return false;
            }
            else
            {
                float t = q[i] / p[i];
                if (p[i] < 0)
                {
                    if (t > t1) return false;
                    if (t > t0) t0 = t;
                }
                else
                {
                    if (t < t0) return false;
                    if (t < t1) t1 = t;
                }
            }
        }

        if (t0 > t1) return false;

        cu1 = u1 + t0 * du;
        cv1 = v1 + t0 * dv;
        cu2 = u1 + t1 * du;
        cv2 = v1 + t1 * dv;

        return true;
    }

    private Vector3 ForceDepthOnAxis2D(float u, float v, Vector3 axis, float targetDepth)
    {
        if (Mathf.Abs(axis.x) > 0.5f) return new Vector3(targetDepth, v, u);
        if (Mathf.Abs(axis.y) > 0.5f) return new Vector3(u, targetDepth, v);
        return new Vector3(u, v, targetDepth);
    }

    // =================================================================
    // LOGIQUE DE SNAPPING DE BORD
    // =================================================================

    private List<Vector3> SnapPointsToFaceEdges(List<Vector3> points, Bounds bounds, Vector3 axis, float snapMargin, float extension)
    {
        List<Vector3> result = new List<Vector3>();

        float minU = 0f, maxU = 0f, minV = 0f, maxV = 0f;

        if (Mathf.Abs(axis.x) > 0.5f)
        {
            minU = bounds.min.z; maxU = bounds.max.z;
            minV = bounds.min.y; maxV = bounds.max.y;
        }
        else if (Mathf.Abs(axis.y) > 0.5f)
        {
            minU = bounds.min.x; maxU = bounds.max.x;
            minV = bounds.min.z; maxV = bounds.max.z;
        }
        else
        {
            minU = bounds.min.x; maxU = bounds.max.x;
            minV = bounds.min.y; maxV = bounds.max.y;
        }

        foreach (Vector3 p in points)
        {
            float u, v;
            if (Mathf.Abs(axis.x) > 0.5f) { u = p.z; v = p.y; }
            else if (Mathf.Abs(axis.y) > 0.5f) { u = p.x; v = p.z; }
            else { u = p.x; v = p.y; }

            if (Mathf.Abs(u - minU) <= snapMargin) u = minU - extension;
            else if (Mathf.Abs(u - maxU) <= snapMargin) u = maxU + extension;

            if (Mathf.Abs(v - minV) <= snapMargin) v = minV - extension;
            else if (Mathf.Abs(v - maxV) <= snapMargin) v = maxV + extension;

            if (Mathf.Abs(axis.x) > 0.5f) result.Add(new Vector3(p.x, v, u));
            else if (Mathf.Abs(axis.y) > 0.5f) result.Add(new Vector3(u, p.y, v));
            else result.Add(new Vector3(u, v, p.z));
        }

        return result;
    }

    // =================================================================
    // ASSEMBLAGE ET ORIENTATION DES MAILLAGES
    // =================================================================

    private Mesh AssembleMeshWithOrientation(List<Triangle3D> surfaceTris, List<Triangle3D> seamWalls, Vector3 targetCenter, bool isMainPavé, string name)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        foreach (var t in surfaceTris)
        {
            int idx = verts.Count;
            verts.Add(t.a); verts.Add(t.b); verts.Add(t.c);
            tris.Add(idx); tris.Add(idx + 1); tris.Add(idx + 2);
        }

        foreach (var w in seamWalls)
        {
            Triangle3D oriented = OrientSeamWall(w, targetCenter, isMainPavé);

            int idx = verts.Count;
            verts.Add(oriented.a); verts.Add(oriented.b); verts.Add(oriented.c);
            tris.Add(idx); tris.Add(idx + 1); tris.Add(idx + 2);
        }

        Mesh m = new Mesh { name = name };
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private Triangle3D OrientSeamWall(Triangle3D tri, Vector3 targetCenter, bool isMainPavé)
    {
        Vector3 triCenter = (tri.a + tri.b + tri.c) / 3f;
        Vector3 norm = Vector3.Cross(tri.b - tri.a, tri.c - tri.a);
        Vector3 dirToCenter = (targetCenter - triCenter).normalized;

        float dot = Vector3.Dot(norm, dirToCenter);

        if (isMainPavé)
        {
            if (dot < 0) return new Triangle3D(tri.a, tri.c, tri.b);
        }
        else
        {
            if (dot > 0) return new Triangle3D(tri.a, tri.c, tri.b);
        }

        return tri;
    }

    // =================================================================
    // SUBDIVISION ET DÉCOUPE PLANAIRE
    // =================================================================

    private struct Triangle3D
    {
        public Vector3 a, b, c;
        public Triangle3D(Vector3 a, Vector3 b, Vector3 c) { this.a = a; this.b = b; this.c = c; }
    }

    private List<Plane> BuildPathSegmentPlanes(List<Vector3> pts, List<Vector3> normals)
    {
        List<Plane> planes = new List<Plane>();
        int count = pts.Count;

        for (int i = 0; i < count; i++)
        {
            Vector3 p1 = pts[i];
            Vector3 p2 = pts[(i + 1) % count];
            Vector3 segDir = (p2 - p1).normalized;

            Vector3 norm = Vector3.Cross(segDir, normals[i]).normalized;
            if (norm.sqrMagnitude > 0.001f)
            {
                planes.Add(new Plane(norm, p1));
            }
        }
        return planes;
    }

    private List<Triangle3D> SubdivideTrianglesByPlanes(List<Triangle3D> inputTris, List<Plane> planes)
    {
        List<Triangle3D> currentList = new List<Triangle3D>(inputTris);

        foreach (Plane plane in planes)
        {
            List<Triangle3D> nextList = new List<Triangle3D>();
            foreach (var tri in currentList)
            {
                SplitTriangleByPlane(tri, plane, out List<Triangle3D> front, out List<Triangle3D> back);
                nextList.AddRange(front);
                nextList.AddRange(back);
            }
            currentList = nextList;
        }
        return currentList;
    }

    private void SplitTriangleByPlane(Triangle3D tri, Plane plane, out List<Triangle3D> front, out List<Triangle3D> back)
    {
        front = new List<Triangle3D>();
        back = new List<Triangle3D>();

        float dA = plane.GetDistanceToPoint(tri.a);
        float dB = plane.GetDistanceToPoint(tri.b);
        float dC = plane.GetDistanceToPoint(tri.c);

        bool aFront = dA >= -0.0001f;
        bool bFront = dB >= -0.0001f;
        bool cFront = dC >= -0.0001f;

        if (aFront && bFront && cFront) { front.Add(tri); return; }
        if (!aFront && !bFront && !cFront) { back.Add(tri); return; }

        Vector3 pA = tri.a, pB = tri.b, pC = tri.c;
        float distA = dA, distB = dB, distC = dC;

        if (aFront == bFront)
        {
            pA = tri.c; pB = tri.a; pC = tri.b;
            distA = dC; distB = dA; distC = dB;
        }
        else if (aFront == cFront)
        {
            pA = tri.b; pB = tri.c; pC = tri.a;
            distA = dB; distB = dC; distC = dA;
        }

        Vector3 AB = IntersectSegmentPlane(pA, pB, distA, distB);
        Vector3 AC = IntersectSegmentPlane(pA, pC, distA, distC);

        if (distA >= 0)
        {
            front.Add(new Triangle3D(pA, AB, AC));
            back.Add(new Triangle3D(AB, pB, pC));
            back.Add(new Triangle3D(AB, pC, AC));
        }
        else
        {
            back.Add(new Triangle3D(pA, AB, AC));
            front.Add(new Triangle3D(AB, pB, pC));
            front.Add(new Triangle3D(AB, pC, AC));
        }
    }

    private Vector3 IntersectSegmentPlane(Vector3 a, Vector3 b, float distA, float distB)
    {
        float denom = distA - distB;
        if (Mathf.Abs(denom) < 0.00001f) return a;
        float t = Mathf.Clamp01(distA / denom);
        return Vector3.Lerp(a, b, t);
    }

    private bool IsPointInPolygon2D(Vector2 pt, List<Vector2> poly)
    {
        bool inside = false;
        int n = poly.Count;
        int j = n - 1;

        for (int i = 0; i < n; i++)
        {
            if (((poly[i].y > pt.y) != (poly[j].y > pt.y)) &&
                (pt.x < (poly[j].x - poly[i].x) * (pt.y - poly[i].y) / (poly[j].y - poly[i].y + 0.00001f) + poly[i].x))
            {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }

    private bool IsPointIn3DPolygonProjection(Vector3 pt, List<Vector3> poly3D, Vector3 loopNorm)
    {
        float absX = Mathf.Abs(loopNorm.x), absY = Mathf.Abs(loopNorm.y), absZ = Mathf.Abs(loopNorm.z);
        Vector2 pt2D;
        List<Vector2> poly2D = new List<Vector2>();

        if (absX >= absY && absX >= absZ)
        {
            pt2D = new Vector2(pt.y, pt.z);
            foreach (var p in poly3D) poly2D.Add(new Vector2(p.y, p.z));
        }
        else if (absY >= absX && absY >= absZ)
        {
            pt2D = new Vector2(pt.x, pt.z);
            foreach (var p in poly3D) poly2D.Add(new Vector2(p.x, p.z));
        }
        else
        {
            pt2D = new Vector2(pt.x, pt.y);
            foreach (var p in poly3D) poly2D.Add(new Vector2(p.x, p.y));
        }

        return IsPointInPolygon2D(pt2D, poly2D);
    }

    private List<Triangle3D> BuildPlanarCap(Plane plane, Bounds bounds)
    {
        List<Triangle3D> capTris = new List<Triangle3D>();
        Vector3[] corners = GetBoxCorners(bounds);

        List<Vector3> intersectPoints = new List<Vector3>();
        int[][] edges = new int[][]
        {
            new int[]{0,1}, new int[]{1,2}, new int[]{2,3}, new int[]{3,0},
            new int[]{4,5}, new int[]{5,6}, new int[]{6,7}, new int[]{7,4},
            new int[]{0,4}, new int[]{1,5}, new int[]{2,6}, new int[]{3,7}
        };

        for (int i = 0; i < edges.Length; i++)
        {
            Vector3 p1 = corners[edges[i][0]];
            Vector3 p2 = corners[edges[i][1]];

            float d1 = plane.GetDistanceToPoint(p1);
            float d2 = plane.GetDistanceToPoint(p2);

            if (d1 * d2 < 0)
            {
                float t = d1 / (d1 - d2);
                intersectPoints.Add(Vector3.Lerp(p1, p2, t));
            }
        }

        if (intersectPoints.Count < 3) return capTris;

        Vector3 center = Vector3.zero;
        foreach (var p in intersectPoints) center += p;
        center /= intersectPoints.Count;

        Vector3 norm = plane.normal;
        Vector3 tangent = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(norm, tangent)) > 0.99f) tangent = Vector3.right;

        Vector3.OrthoNormalize(ref norm, ref tangent);
        Vector3 u = tangent;
        Vector3 v = Vector3.Cross(norm, u);

        intersectPoints.Sort((a, b) =>
        {
            Vector3 dirA = a - center;
            Vector3 dirB = b - center;
            float angleA = Mathf.Atan2(Vector3.Dot(dirA, v), Vector3.Dot(dirA, u));
            float angleB = Mathf.Atan2(Vector3.Dot(dirB, v), Vector3.Dot(dirB, u));
            return angleA.CompareTo(angleB);
        });

        for (int i = 1; i < intersectPoints.Count - 1; i++)
        {
            capTris.Add(new Triangle3D(intersectPoints[0], intersectPoints[i], intersectPoints[i + 1]));
        }

        return capTris;
    }

    private List<int> Triangulate3DLoopDirect(List<Vector3> points)
    {
        List<int> tris = new List<int>();
        int n = points.Count;
        if (n < 3) return tris;

        List<int> V = new List<int>();
        for (int i = 0; i < n; i++) V.Add(i);

        int safety = n * n * 2;

        while (V.Count > 3 && safety > 0)
        {
            safety--;
            int bestEarIdx = -1;
            float minArea = float.MaxValue;

            int count = V.Count;
            for (int i = 0; i < count; i++)
            {
                int prevIdx = V[(i - 1 + count) % count];
                int currIdx = V[i];
                int nextIdx = V[(i + 1) % count];

                Vector3 a = points[prevIdx];
                Vector3 b = points[currIdx];
                Vector3 c = points[nextIdx];

                float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
                if (area < 0.000001f) continue;

                if (area < minArea)
                {
                    minArea = area;
                    bestEarIdx = i;
                }
            }

            if (bestEarIdx != -1)
            {
                int prevIdx = V[(bestEarIdx - 1 + V.Count) % V.Count];
                int currIdx = V[bestEarIdx];
                int nextIdx = V[(bestEarIdx + 1) % V.Count];

                tris.Add(prevIdx); tris.Add(currIdx); tris.Add(nextIdx);
                V.RemoveAt(bestEarIdx);
            }
            else
            {
                tris.Add(V[0]); tris.Add(V[1]); tris.Add(V[2]);
                V.RemoveAt(1);
            }
        }

        if (V.Count == 3)
        {
            tris.Add(V[0]); tris.Add(V[1]); tris.Add(V[2]);
        }

        return tris;
    }

    private void ApplySplitToScene(Mesh mainMesh, Mesh cutMesh, CutType cutType)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshCollider col = GetComponent<MeshCollider>();

        mf.sharedMesh = mainMesh;
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = mainMesh;
        }

        GameObject cutPieceObj = new GameObject($"PavéPiece_{cutType}");
        cutPieceObj.transform.SetParent(transform.parent, false);
        cutPieceObj.transform.position = transform.position;
        cutPieceObj.transform.rotation = transform.rotation;
        cutPieceObj.transform.localScale = transform.localScale;

        MeshFilter pieceMf = cutPieceObj.AddComponent<MeshFilter>();
        MeshRenderer pieceMr = cutPieceObj.AddComponent<MeshRenderer>();
        MeshCollider pieceCol = cutPieceObj.AddComponent<MeshCollider>();

        pieceMf.sharedMesh = cutMesh;
        pieceMr.sharedMaterial = cutPieceMaterial != null ? cutPieceMaterial : GetComponent<MeshRenderer>().sharedMaterial;

        pieceCol.sharedMesh = cutMesh;
        pieceCol.convex = false;

        cutPieceObj.isStatic = true;
    }

    private void ShowDebugSolid(Mesh cuttingSolid, CutType cutType)
    {
        GameObject debugObj = new GameObject($"Debug_CuttingSolid_{cutType}");
        debugObj.transform.SetParent(transform.parent, false);
        debugObj.transform.position = transform.position;
        debugObj.transform.rotation = transform.rotation;
        debugObj.transform.localScale = transform.localScale;

        MeshFilter mf = debugObj.AddComponent<MeshFilter>();
        MeshRenderer mr = debugObj.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        mf.sharedMesh = cuttingSolid;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        Material debugMat = new Material(shader);
        Color debugColor = new Color(1f, 0f, 0f, debugSolidOpacity);

        if (debugMat.HasProperty("_BaseColor")) debugMat.SetColor("_BaseColor", debugColor);
        else debugMat.color = debugColor;

        debugMat.SetFloat("_Surface", 1);
        debugMat.SetFloat("_Blend", 0);
        debugMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        debugMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        debugMat.SetInt("_ZWrite", 0);
        debugMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        debugMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        debugMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        mr.sharedMaterial = debugMat;
    }

    private List<Plane> BuildSegmentPlanes2D(List<Vector3> pts, Vector3 axis)
    {
        List<Plane> planes = new List<Plane>();
        int count = pts.Count;
        for (int i = 0; i < count; i++)
        {
            Vector3 p1 = pts[i];
            Vector3 p2 = pts[(i + 1) % count];
            Vector3 segDir = (p2 - p1).normalized;
            Vector3 norm = Vector3.Cross(segDir, axis).normalized;

            if (norm.sqrMagnitude > 0.001f) planes.Add(new Plane(norm, p1));
        }
        return planes;
    }

    private List<Triangle3D> ExtractTriangles(Mesh m)
    {
        List<Triangle3D> list = new List<Triangle3D>();
        Vector3[] verts = m.vertices;
        int[] tris = m.triangles;

        for (int i = 0; i < tris.Length; i += 3)
        {
            list.Add(new Triangle3D(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]]));
        }
        return list;
    }

    private Vector3[] GetBoxCorners(Bounds b)
    {
        return new Vector3[]
        {
            new Vector3(b.min.x, b.min.y, b.min.z), new Vector3(b.max.x, b.min.y, b.min.z),
            new Vector3(b.max.x, b.max.y, b.min.z), new Vector3(b.min.x, b.max.y, b.min.z),
            new Vector3(b.min.x, b.min.y, b.max.z), new Vector3(b.max.x, b.min.y, b.max.z),
            new Vector3(b.max.x, b.max.y, b.max.z), new Vector3(b.min.x, b.max.y, b.max.z)
        };
    }

    private Vector3 GetAverageNormal(List<Vector3> normals)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 n in normals) sum += n;
        return sum.normalized;
    }

    private Vector3 GetPrimaryAxis(Vector3 dir)
    {
        float absX = Mathf.Abs(dir.x), absY = Mathf.Abs(dir.y), absZ = Mathf.Abs(dir.z);
        if (absX > absY && absX > absZ) return dir.x > 0 ? Vector3.right : Vector3.left;
        if (absY > absX && absY > absZ) return dir.y > 0 ? Vector3.up : Vector3.down;
        return dir.z > 0 ? Vector3.forward : Vector3.back;
    }

    private float GetDepth(Bounds b, Vector3 axis, bool isMax)
    {
        if (Mathf.Abs(axis.x) > 0.5f) return isMax ? b.max.x : b.min.x;
        if (Mathf.Abs(axis.y) > 0.5f) return isMax ? b.max.y : b.min.y;
        return isMax ? b.max.z : b.min.z;
    }

    private List<Vector2> ProjectTo2D(List<Vector3> points, Vector3 axis)
    {
        List<Vector2> pts2D = new List<Vector2>();
        foreach (Vector3 p in points) pts2D.Add(ProjectSingleTo2D(p, axis));
        return pts2D;
    }

    private Vector2 ProjectSingleTo2D(Vector3 p, Vector3 axis)
    {
        if (Mathf.Abs(axis.x) > 0.5f) return new Vector2(p.z, p.y);
        if (Mathf.Abs(axis.y) > 0.5f) return new Vector2(p.x, p.z);
        return new Vector2(p.x, p.y);
    }

    private Vector3 ComputeLoopNormal(List<Vector3> loop, Vector3 center)
    {
        Vector3 norm = Vector3.zero;
        for (int i = 0; i < loop.Count; i++)
        {
            Vector3 c = loop[i] - center;
            Vector3 n = loop[(i + 1) % loop.Count] - center;
            norm += Vector3.Cross(c, n);
        }
        return norm.normalized;
    }
}