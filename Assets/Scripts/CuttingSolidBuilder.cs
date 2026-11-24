using System.Collections.Generic;
using UnityEngine;

public enum CutType
{
    SingleFaceExtrusion, // 1. Dessin 2D extrudé à travers le pavé
    ObliquePlaneCut,     // 2. Plan oblique séparant le pavé en deux
    General3DPath        // 3. Forme 3D quelconque (Convex Hull 3D)
}

public static class CuttingSolidBuilder
{
    public static Mesh CreateCuttingSolid(List<Vector3> localPoints, List<Vector3> localNormals, Bounds bounds, out CutType detectedCutType)
    {
        if (localPoints == null || localPoints.Count < 3)
        {
            detectedCutType = CutType.SingleFaceExtrusion;
            return null;
        }

        List<Vector3> points = EnsureClosedLoop(localPoints);
        List<Vector3> normals = EnsureClosedLoop(localNormals);

        Vector3 center = GetCentroid(points);
        Vector3 planeNormal = ComputeLoopNormal(points, center);
        if (planeNormal.sqrMagnitude < 0.0001f) planeNormal = GetAverageNormal(normals);

        bool isMultiSurface = IsMultiSurfacePath(normals);
        bool isPlanar = ArePointsPlanar(points, center, planeNormal, bounds.size.magnitude * 0.03f);

        // -----------------------------------------------------------------
        // CAS 1 : PLAN OBLIQUE (Ligne / plan traversant plusieurs faces)
        // -----------------------------------------------------------------
        if (isMultiSurface && isPlanar)
        {
            detectedCutType = CutType.ObliquePlaneCut;
            Mesh obliqueSolid = BuildObliquePlaneSolid(center, planeNormal, bounds);
            if (obliqueSolid != null) return obliqueSolid;
        }

        // -----------------------------------------------------------------
        // CAS 2 : FORME 3D QUELCONQUE (Convex Hull 3D)
        // -----------------------------------------------------------------
        if (isMultiSurface || !isPlanar)
        {
            detectedCutType = CutType.General3DPath;
            return Build3DLoopConvexHull(points);
        }

        // -----------------------------------------------------------------
        // CAS 3 : DESSIN 2D SUR UNE SURFACE (Extrusion traversante)
        // -----------------------------------------------------------------
        detectedCutType = CutType.SingleFaceExtrusion;
        Vector3 avgNormal = GetAverageNormal(normals);
        Vector3 primaryAxis = GetPrimaryAxis(avgNormal);
        return BuildExtrudedSolidFromPoints(points, bounds, primaryAxis);
    }

    // =================================================================
    // CAS 2 : FORME 3D QUELCONQUE (CONVEX HULL 3D)
    // =================================================================

    private static Mesh Build3DLoopConvexHull(List<Vector3> points)
    {
        if (points == null || points.Count < 4) return null;

        Mesh hullMesh = BuildConvexHull3DMesh(points, "General3DPathCuttingSolid");

        if (hullMesh == null)
        {
            Debug.LogWarning("[CuttingSolidBuilder] Échec de la Convex Hull 3D (points coplanaires ou colinéaires).");
        }

        return hullMesh;
    }

    // =================================================================
    // CAS 1 & CAS 3 : PLAN OBLIQUE ET EXTRUSION 2D
    // =================================================================

    private static Mesh BuildExtrudedSolidFromPoints(List<Vector3> frontPoints, Bounds bounds, Vector3 axis)
    {
        float minDepth = GetDepth(bounds, axis, false);
        float maxDepth = GetDepth(bounds, axis, true);

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        int count = frontPoints.Count;

        for (int i = 0; i < count; i++) verts.Add(ForceDepthOnAxis(frontPoints[i], axis, maxDepth));
        for (int i = 0; i < count; i++) verts.Add(ForceDepthOnAxis(frontPoints[i], axis, minDepth));

        int backOffset = count;

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            int f1 = i, f2 = next;
            int b1 = backOffset + i, b2 = backOffset + next;

            tris.Add(f1); tris.Add(b1); tris.Add(f2);
            tris.Add(f2); tris.Add(b1); tris.Add(b2);
        }

        List<Vector2> poly2D = ProjectTo2D(frontPoints, axis);
        List<int> capTris = TriangulateEarClipping2D(poly2D);

        if (capTris != null && capTris.Count >= 3)
        {
            for (int i = 0; i < capTris.Count; i += 3)
            {
                tris.Add(capTris[i]); tris.Add(capTris[i + 1]); tris.Add(capTris[i + 2]);
            }

            for (int i = 0; i < capTris.Count; i += 3)
            {
                tris.Add(backOffset + capTris[i]); tris.Add(backOffset + capTris[i + 2]); tris.Add(backOffset + capTris[i + 1]);
            }
        }

        Mesh m = new Mesh { name = "ExtrudedCuttingSolid" };
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        EnsureOutwardNormals(m);

        return m;
    }

    private static Mesh BuildObliquePlaneSolid(Vector3 planeCenter, Vector3 planeNormal, Bounds b)
    {
        Vector3[] corners = GetBoxCorners(b);
        int[][] edges = new int[][]
        {
            new int[]{0,1}, new int[]{1,2}, new int[]{2,3}, new int[]{3,0},
            new int[]{4,5}, new int[]{5,6}, new int[]{6,7}, new int[]{7,4},
            new int[]{0,4}, new int[]{1,5}, new int[]{2,6}, new int[]{3,7}
        };

        List<Vector3> solidVerts = new List<Vector3>();

        for (int i = 0; i < edges.Length; i++)
        {
            Vector3 p1 = corners[edges[i][0]];
            Vector3 p2 = corners[edges[i][1]];

            float d1 = Vector3.Dot(planeNormal, p1 - planeCenter);
            float d2 = Vector3.Dot(planeNormal, p2 - planeCenter);

            if (d1 * d2 < 0)
            {
                float t = d1 / (d1 - d2);
                solidVerts.Add(Vector3.Lerp(p1, p2, t));
            }
        }

        List<Vector3> posSideCorners = new List<Vector3>();
        List<Vector3> negSideCorners = new List<Vector3>();

        foreach (Vector3 c in corners)
        {
            float dist = Vector3.Dot(planeNormal, c - planeCenter);
            if (dist >= 0) posSideCorners.Add(c);
            else negSideCorners.Add(c);
        }

        List<Vector3> chosenCorners = (posSideCorners.Count > 0 && posSideCorners.Count <= 4) ? posSideCorners : negSideCorners;
        if (chosenCorners.Count == 0) chosenCorners = posSideCorners;

        solidVerts.AddRange(chosenCorners);

        if (solidVerts.Count < 4) return null;
        return BuildConvexHull3DMesh(solidVerts, "ObliquePlaneCuttingSolid");
    }

    // =================================================================
    // ALGORITHME DE CONVEX HULL 3D (QUICKHULL 3D AVEC NORMALE STRICTE)
    // =================================================================

    private static Mesh BuildConvexHull3DMesh(List<Vector3> pts, string meshName)
    {
        int n = pts.Count;
        if (n < 4) return null;

        int p0 = 0, p1 = 1;
        float maxD = 0f;
        for (int i = 1; i < n; i++)
        {
            float d = Vector3.Distance(pts[0], pts[i]);
            if (d > maxD) { maxD = d; p1 = i; }
        }

        Vector3 lineDir = (pts[p1] - pts[p0]).normalized;
        int p2 = 0; maxD = 0f;
        for (int i = 0; i < n; i++)
        {
            float d = Vector3.Cross(pts[i] - pts[p0], lineDir).magnitude;
            if (d > maxD) { maxD = d; p2 = i; }
        }

        Vector3 planeNorm = Vector3.Cross(pts[p1] - pts[p0], pts[p2] - pts[p0]).normalized;
        int p3 = 0; maxD = 0f;
        for (int i = 0; i < n; i++)
        {
            float d = Mathf.Abs(Vector3.Dot(pts[i] - pts[p0], planeNorm));
            if (d > maxD) { maxD = d; p3 = i; }
        }

        if (maxD < 0.0001f) return null;

        List<int[]> faces = new List<int[]>()
        {
            new int[]{ p0, p1, p2 }, new int[]{ p0, p2, p3 },
            new int[]{ p0, p3, p1 }, new int[]{ p1, p3, p2 }
        };

        Vector3 center = (pts[p0] + pts[p1] + pts[p2] + pts[p3]) * 0.25f;
        for (int i = 0; i < faces.Count; i++)
        {
            int[] f = faces[i];
            Vector3 fn = Vector3.Cross(pts[f[1]] - pts[f[0]], pts[f[2]] - pts[f[0]]);
            if (Vector3.Dot(fn, pts[f[0]] - center) < 0)
            {
                faces[i] = new int[] { f[0], f[2], f[1] };
            }
        }

        HashSet<int> used = new HashSet<int> { p0, p1, p2, p3 };
        for (int i = 0; i < n; i++)
        {
            if (used.Contains(i)) continue;
            Vector3 pt = pts[i];

            List<int[]> visible = new List<int[]>();
            for (int j = 0; j < faces.Count; j++)
            {
                int[] f = faces[j];
                Vector3 fn = Vector3.Cross(pts[f[1]] - pts[f[0]], pts[f[2]] - pts[f[0]]);
                if (Vector3.Dot(fn, pt - pts[f[0]]) > 0.0001f)
                {
                    visible.Add(f);
                }
            }

            if (visible.Count == 0) continue;

            Dictionary<(int, int), int> edgeCounts = new Dictionary<(int, int), int>();
            foreach (var f in visible)
            {
                for (int k = 0; k < 3; k++)
                {
                    var edge = (f[k], f[(k + 1) % 3]);
                    var revEdge = (f[(k + 1) % 3], f[k]);
                    if (edgeCounts.ContainsKey(revEdge)) edgeCounts[revEdge]++;
                    else edgeCounts[edge] = 1;
                }
            }

            List<(int, int)> horizon = new List<(int, int)>();
            foreach (var kvp in edgeCounts)
            {
                if (kvp.Value == 1 && !edgeCounts.ContainsKey((kvp.Key.Item2, kvp.Key.Item1)))
                {
                    horizon.Add(kvp.Key);
                }
            }

            foreach (var f in visible) faces.Remove(f);

            foreach (var e in horizon)
            {
                faces.Add(new int[] { e.Item1, e.Item2, i });
            }
        }

        List<int> tris = new List<int>();
        foreach (var f in faces)
        {
            tris.Add(f[0]); tris.Add(f[1]); tris.Add(f[2]);
        }

        Mesh m = new Mesh { name = meshName };
        m.vertices = pts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();

        EnsureOutwardNormals(m);
        return m;
    }

    // =================================================================
    // ORIENTATION CORRECTE DE TOUTES LES NORMALES VERS L'EXTÉRIEUR
    // =================================================================

    private static void EnsureOutwardNormals(Mesh m)
    {
        Vector3[] verts = m.vertices;
        int[] tris = m.triangles;

        // Utilisation du centroïde volumétrique réel (moyenne exacte des sommets)
        Vector3 centroid = GetCentroid(new List<Vector3>(verts));

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v0 = verts[tris[i]];
            Vector3 v1 = verts[tris[i + 1]];
            Vector3 v2 = verts[tris[i + 2]];

            Vector3 norm = Vector3.Cross(v1 - v0, v2 - v0).normalized;
            Vector3 faceCenter = (v0 + v1 + v2) / 3f;

            // Le vecteur (faceCenter - centroid) pointe STRICTEMENT vers l'extérieur.
            // Si le produit scalaire est négatif, la normale pointait vers l'intérieur : on inverse le winding.
            if (Vector3.Dot(norm, faceCenter - centroid) < 0)
            {
                int temp = tris[i];
                tris[i] = tris[i + 1];
                tris[i + 1] = temp;
            }
        }

        m.triangles = tris;
        m.RecalculateNormals();
    }

    // =================================================================
    // UTILS GÉOMÉTRIQUES & TRIANGULATION 2D
    // =================================================================

    private static List<Vector3> EnsureClosedLoop(List<Vector3> list)
    {
        if (list == null || list.Count == 0) return list;
        List<Vector3> result = new List<Vector3>(list);
        if (Vector3.Distance(result[0], result[result.Count - 1]) > 0.001f)
        {
            result.Add(result[0]);
        }
        return result;
    }

    private static Vector3 GetCentroid(List<Vector3> pts)
    {
        if (pts == null || pts.Count == 0) return Vector3.zero;
        Vector3 c = Vector3.zero;
        foreach (Vector3 p in pts) c += p;
        return c / pts.Count;
    }

    private static bool IsMultiSurfacePath(List<Vector3> normals)
    {
        if (normals == null || normals.Count < 2) return false;
        Vector3 first = normals[0];
        for (int i = 1; i < normals.Count; i++)
        {
            if (Vector3.Angle(first, normals[i]) > 20f) return true;
        }
        return false;
    }

    private static bool ArePointsPlanar(List<Vector3> points, Vector3 center, Vector3 planeNormal, float maxTolerance)
    {
        foreach (Vector3 p in points)
        {
            if (Mathf.Abs(Vector3.Dot(planeNormal, p - center)) > maxTolerance) return false;
        }
        return true;
    }

    private static Vector3 ComputeLoopNormal(List<Vector3> loop, Vector3 center)
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

    private static List<int> TriangulateEarClipping2D(List<Vector2> poly)
    {
        List<int> tris = new List<int>();
        int n = poly.Count;
        if (n < 3) return tris;

        List<int> V = new List<int>();
        for (int i = 0; i < n; i++) V.Add(i);

        int count = n, safety = count * 3;

        while (count > 2 && safety > 0)
        {
            safety--;
            bool earFound = false;

            for (int i = 0; i < count; i++)
            {
                int prevIdx = V[(i - 1 + count) % count];
                int currIdx = V[i];
                int nextIdx = V[(i + 1) % count];

                Vector2 a = poly[prevIdx], b = poly[currIdx], c = poly[nextIdx];

                if (!IsConvex2D(a, b, c)) continue;

                bool hasPointInside = false;
                for (int j = 0; j < count; j++)
                {
                    int testIdx = V[j];
                    if (testIdx == prevIdx || testIdx == currIdx || testIdx == nextIdx) continue;
                    if (IsPointInTriangle2D(poly[testIdx], a, b, c))
                    {
                        hasPointInside = true; break;
                    }
                }

                if (!hasPointInside)
                {
                    tris.Add(prevIdx); tris.Add(currIdx); tris.Add(nextIdx);
                    V.RemoveAt(i);
                    count--;
                    earFound = true;
                    break;
                }
            }

            if (!earFound)
            {
                tris.Add(V[0]); tris.Add(V[1]); tris.Add(V[2]);
                V.RemoveAt(1);
                count--;
            }
        }
        return tris;
    }

    private static bool IsConvex2D(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) > 0.00001f;

    private static bool IsPointInTriangle2D(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
        float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
        float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
        return !(((d1 < -0.0001f) || (d2 < -0.0001f) || (d3 < -0.0001f)) && ((d1 > 0.0001f) || (d2 > 0.0001f) || (d3 > 0.0001f)));
    }

    private static Vector3[] GetBoxCorners(Bounds b)
    {
        return new Vector3[]
        {
            new Vector3(b.min.x, b.min.y, b.min.z), new Vector3(b.max.x, b.min.y, b.min.z),
            new Vector3(b.max.x, b.max.y, b.min.z), new Vector3(b.min.x, b.max.y, b.min.z),
            new Vector3(b.min.x, b.min.y, b.max.z), new Vector3(b.max.x, b.min.y, b.max.z),
            new Vector3(b.max.x, b.max.y, b.max.z), new Vector3(b.min.x, b.max.y, b.max.z)
        };
    }

    private static Vector3 ForceDepthOnAxis(Vector3 p, Vector3 axis, float targetDepth)
    {
        if (Mathf.Abs(axis.x) > 0.5f) return new Vector3(targetDepth, p.y, p.z);
        if (Mathf.Abs(axis.y) > 0.5f) return new Vector3(p.x, targetDepth, p.z);
        return new Vector3(p.x, p.y, targetDepth);
    }

    private static Vector3 GetAverageNormal(List<Vector3> normals)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 n in normals) sum += n;
        return sum.normalized;
    }

    private static Vector3 GetPrimaryAxis(Vector3 dir)
    {
        float absX = Mathf.Abs(dir.x), absY = Mathf.Abs(dir.y), absZ = Mathf.Abs(dir.z);
        if (absX > absY && absX > absZ) return dir.x > 0 ? Vector3.right : Vector3.left;
        if (absY > absX && absY > absZ) return dir.y > 0 ? Vector3.up : Vector3.down;
        return dir.z > 0 ? Vector3.forward : Vector3.back;
    }

    private static float GetDepth(Bounds b, Vector3 axis, bool isMax)
    {
        if (Mathf.Abs(axis.x) > 0.5f) return isMax ? b.max.x : b.min.x;
        if (Mathf.Abs(axis.y) > 0.5f) return isMax ? b.max.y : b.min.y;
        return isMax ? b.max.z : b.min.z;
    }

    private static List<Vector2> ProjectTo2D(List<Vector3> points, Vector3 axis)
    {
        List<Vector2> pts2D = new List<Vector2>();
        foreach (Vector3 p in points)
        {
            if (Mathf.Abs(axis.x) > 0.5f) pts2D.Add(new Vector2(p.z, p.y));
            else if (Mathf.Abs(axis.y) > 0.5f) pts2D.Add(new Vector2(p.x, p.z));
            else pts2D.Add(new Vector2(p.x, p.y));
        }
        return pts2D;
    }
}