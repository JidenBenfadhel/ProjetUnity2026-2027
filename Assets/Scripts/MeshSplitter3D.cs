using System.Collections.Generic;
using UnityEngine;

public class MeshSplitter3D : MultiFaceMeshSplitter
{
    public bool SplitMesh3D(List<Vector3> worldPoints, Vector3 hitWorldNormal)
    {
        if (worldPoints == null || worldPoints.Count < 4) return false;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return false;

        Mesh srcMesh = mf.sharedMesh;
        Vector3 localHitNormal = transform.InverseTransformDirection(hitWorldNormal).normalized;
        Vector3 primaryAxis = GetPrimaryAxis(localHitNormal);

        // 1. Conversion des points du tracé en coordonnées locales et 2D
        List<Vector3> localFront = new List<Vector3>();
        List<Vector2> poly2D = new List<Vector2>();

        for (int i = 0; i < worldPoints.Count - 1; i++)
        {
            Vector3 localP = transform.InverseTransformPoint(worldPoints[i]);
            localFront.Add(localP);
            poly2D.Add(ProjectTo2D(localP, primaryAxis));
        }

        poly2D = CleanPolygon(poly2D);
        if (poly2D.Count < 3) return false;

        Bounds bounds = srcMesh.bounds;
        List<Vector2> quad2D = GetQuadCorners2D(bounds, primaryAxis);

        // 2. Détection si au moins 2 points du tracé touchent le bord du pavé
        List<int> boundaryIndices = GetPointsOnBoundary(poly2D, quad2D, 0.015f);

        // --- ROUTAGE : DECOUPE SPECIFIQUE DE BORDURE (SPLIT EDGE) ---
        if (boundaryIndices.Count >= 2)
        {
            return SplitEdge(srcMesh, poly2D, boundaryIndices, quad2D, primaryAxis, bounds);
        }

        // --- PERÇAGE INTERNE (TUNNELING CLASSIQUE) ---
        return SplitTunnel(srcMesh, poly2D, localFront, primaryAxis, bounds);
    }

    // ==========================================
    // FONCTION SPÉCIALISÉE : SPLIT EDGE
    // ==========================================

    private bool SplitEdge(
        Mesh srcMesh,
        List<Vector2> path2D,
        List<int> bIndices,
        List<Vector2> quad2D,
        Vector3 axis,
        Bounds bounds)
    {
        // 1. Extraction du chemin du tracé reliant l'entrée et la sortie du bord
        int entryIdx = bIndices[0];
        int exitIdx = bIndices[bIndices.Count - 1];

        List<Vector2> cutPath = new List<Vector2>();
        for (int i = entryIdx; i != exitIdx; i = (i + 1) % path2D.Count)
        {
            cutPath.Add(path2D[i]);
        }
        cutPath.Add(path2D[exitIdx]);

        Vector2 pEntry = cutPath[0];
        Vector2 pExit = cutPath[cutPath.Count - 1];

        // 2. Calcul du périmètre paramétrique pour séparer le quad en 2 polygones
        float t1 = GetPerimeterT(pEntry, quad2D);
        float t2 = GetPerimeterT(pExit, quad2D);

        var corners = new (float t, Vector2 pt)[]
        {
            (0.0f, quad2D[0]),
            (1.0f, quad2D[1]),
            (2.0f, quad2D[2]),
            (3.0f, quad2D[3])
        };

        List<Vector2> perim1 = GetPerimeterSegment(corners, t1, t2);
        List<Vector2> perim2 = GetPerimeterSegment(corners, t2, t1);

        // Construction du polygone de tranche et du polygone restant
        List<Vector2> polyA = new List<Vector2>(cutPath);
        polyA.AddRange(perim2);

        List<Vector2> cutPathRev = new List<Vector2>(cutPath);
        cutPathRev.Reverse();
        List<Vector2> polyB = new List<Vector2>(cutPathRev);
        polyB.AddRange(perim1);

        polyA = CleanPolygon(polyA);
        polyB = CleanPolygon(polyB);

        if (polyA.Count < 3 || polyB.Count < 3) return false;

        // Identification : la plus petite surface est le morceau découpé
        List<Vector2> slicePoly = (Mathf.Abs(GetSignedArea(polyA)) < Mathf.Abs(GetSignedArea(polyB))) ? polyA : polyB;
        List<Vector2> remainingPoly = (slicePoly == polyA) ? polyB : polyA;

        // 3. Extrusion 3D indépendante des deux morceaux
        Mesh sliceMesh = BuildExtrudedVolume(slicePoly, bounds, axis, "EdgeSliceMesh");
        Mesh remainingMesh = BuildExtrudedVolume(remainingPoly, bounds, axis, "RemainingBlockMesh");

        if (sliceMesh == null || remainingMesh == null) return false;

        // 4. Application dans la scène
        CreateChildGameObject("Morceau_Bordure_3D", sliceMesh);

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = remainingMesh;

        MeshCollider col = GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = remainingMesh;
        }

        return true;
    }

    // ==========================================
    // PERÇAGE INTERNE (TUNNELING)
    // ==========================================

    private bool SplitTunnel(Mesh srcMesh, List<Vector2> hole2D, List<Vector3> front3D, Vector3 axis, Bounds bounds)
    {
        float thickness = GetThickness(bounds, axis);
        Vector3 extrusionVector = -axis * thickness;

        List<Vector3> back3D = new List<Vector3>();
        for (int i = 0; i < front3D.Count; i++)
        {
            back3D.Add(front3D[i] + extrusionVector);
        }

        List<Vector2> ccwPoly = new List<Vector2>(hole2D);
        if (GetSignedArea(ccwPoly) < 0) ccwPoly.Reverse();

        List<int> capTris2D = TriangulateSimplePolygon(ccwPoly);
        if (capTris2D.Count < 3) return false;

        Mesh innerMesh = BuildPrismPiece(ccwPoly, capTris2D, front3D, back3D);
        Mesh outerMesh = BuildNotchedBlock(srcMesh, bounds, hole2D, capTris2D, front3D, back3D, axis);

        if (innerMesh == null || outerMesh == null) return false;

        CreateChildGameObject("Piece_Decoupee_3D", innerMesh);

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = outerMesh;

        MeshCollider col = GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = outerMesh;
        }

        return true;
    }

    // ==========================================
    // UTILITAIRES SPLIT EDGE & GEOMETRIE
    // ==========================================

    private List<int> GetPointsOnBoundary(List<Vector2> poly, List<Vector2> quad, float tolerance)
    {
        List<int> onBoundary = new List<int>();
        float minX = quad[0].x, maxX = quad[1].x;
        float minY = quad[0].y, maxY = quad[2].y;

        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p = poly[i];
            if (Mathf.Abs(p.x - minX) < tolerance || Mathf.Abs(p.x - maxX) < tolerance ||
                Mathf.Abs(p.y - minY) < tolerance || Mathf.Abs(p.y - maxY) < tolerance)
            {
                onBoundary.Add(i);
            }
        }
        return onBoundary;
    }

    private List<Vector2> GetPerimeterSegment((float t, Vector2 pt)[] corners, float tStart, float tEnd)
    {
        List<Vector2> pts = new List<Vector2>();
        if (tStart < tEnd)
        {
            foreach (var c in corners)
                if (c.t > tStart && c.t < tEnd) pts.Add(c.pt);
        }
        else
        {
            List<(float t, Vector2 pt)> temp = new List<(float t, Vector2 pt)>();
            foreach (var c in corners)
            {
                if (c.t > tStart || c.t < tEnd)
                {
                    float tAdj = c.t > tStart ? c.t : c.t + 4.0f;
                    temp.Add((tAdj, c.pt));
                }
            }
            temp.Sort((a, b) => a.t.CompareTo(b.t));
            foreach (var item in temp) pts.Add(item.pt);
        }
        return pts;
    }

    private float GetPerimeterT(Vector2 p, List<Vector2> quad)
    {
        float minX = quad[0].x, maxX = quad[1].x;
        float minY = quad[0].y, maxY = quad[2].y;
        float eps = 0.02f;

        if (Mathf.Abs(p.y - minY) < eps) return Mathf.Clamp01((p.x - minX) / (maxX - minX));
        if (Mathf.Abs(p.x - maxX) < eps) return 1f + Mathf.Clamp01((p.y - minY) / (maxY - minY));
        if (Mathf.Abs(p.y - maxY) < eps) return 2f + Mathf.Clamp01((maxX - p.x) / (maxX - minX));
        if (Mathf.Abs(p.x - minX) < eps) return 3f + Mathf.Clamp01((maxY - p.y) / (maxY - minY));

        return 0f;
    }

    private Mesh BuildExtrudedVolume(List<Vector2> poly2D, Bounds b, Vector3 axis, string name)
    {
        if (GetSignedArea(poly2D) < 0) poly2D.Reverse();
        List<int> tris2D = TriangulateSimplePolygon(poly2D);
        if (tris2D.Count < 3) return null;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        float frontDepth = GetDepth(b, axis, true);
        float backDepth = GetDepth(b, axis, false);

        int frontOffset = verts.Count;
        foreach (Vector2 p in poly2D) verts.Add(UnprojectTo3D(p, axis, frontDepth));

        int backOffset = verts.Count;
        foreach (Vector2 p in poly2D) verts.Add(UnprojectTo3D(p, axis, backDepth));

        for (int i = 0; i < tris2D.Count; i += 3)
        {
            tris.Add(frontOffset + tris2D[i]);
            tris.Add(frontOffset + tris2D[i + 1]);
            tris.Add(frontOffset + tris2D[i + 2]);
        }

        for (int i = 0; i < tris2D.Count; i += 3)
        {
            tris.Add(backOffset + tris2D[i]);
            tris.Add(backOffset + tris2D[i + 2]);
            tris.Add(backOffset + tris2D[i + 1]);
        }

        int count = poly2D.Count;
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            int f1 = frontOffset + i;
            int f2 = frontOffset + next;
            int b1 = backOffset + i;
            int b2 = backOffset + next;

            tris.Add(f1); tris.Add(b1); tris.Add(f2);
            tris.Add(f2); tris.Add(b1); tris.Add(b2);
        }

        Mesh m = new Mesh { name = name };
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private Mesh BuildPrismPiece(List<Vector2> poly2D, List<int> capTris2D, List<Vector3> front3D, List<Vector3> back3D)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        int frontOffset = verts.Count;
        verts.AddRange(front3D);
        int backOffset = verts.Count;
        verts.AddRange(back3D);

        for (int i = 0; i < capTris2D.Count; i += 3)
        {
            tris.Add(frontOffset + capTris2D[i]);
            tris.Add(frontOffset + capTris2D[i + 1]);
            tris.Add(frontOffset + capTris2D[i + 2]);
        }

        for (int i = 0; i < capTris2D.Count; i += 3)
        {
            tris.Add(backOffset + capTris2D[i]);
            tris.Add(backOffset + capTris2D[i + 2]);
            tris.Add(backOffset + capTris2D[i + 1]);
        }

        int count = front3D.Count;
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            int f1 = i;
            int f2 = next;
            int b1 = backOffset + i;
            int b2 = backOffset + next;

            tris.Add(f1); tris.Add(b1); tris.Add(f2);
            tris.Add(f2); tris.Add(b1); tris.Add(b2);
        }

        Mesh m = new Mesh { name = "InnerPrismPiece" };
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private Mesh BuildNotchedBlock(Mesh srcMesh, Bounds b, List<Vector2> hole2D, List<int> capTris2D, List<Vector3> front3D, List<Vector3> back3D, Vector3 axis)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        List<Vector2> quad2D = GetQuadCorners2D(b, axis);
        if (GetSignedArea(quad2D) < 0) quad2D.Reverse();

        List<Vector2> holeCW = new List<Vector2>(hole2D);
        if (GetSignedArea(holeCW) > 0) holeCW.Reverse();

        List<Vector2> mergedPoly = MergeQuadAndHole(quad2D, holeCW);
        List<int> outerCapTris = TriangulateSimplePolygon(mergedPoly);
        if (outerCapTris.Count < 3) return null;

        float frontDepth = GetDepth(b, axis, true);
        float backDepth = GetDepth(b, axis, false);

        int frontCapOffset = verts.Count;
        for (int i = 0; i < mergedPoly.Count; i++) verts.Add(UnprojectTo3D(mergedPoly[i], axis, frontDepth));

        for (int i = 0; i < outerCapTris.Count; i += 3)
        {
            tris.Add(frontCapOffset + outerCapTris[i]);
            tris.Add(frontCapOffset + outerCapTris[i + 1]);
            tris.Add(frontCapOffset + outerCapTris[i + 2]);
        }

        int backCapOffset = verts.Count;
        for (int i = 0; i < mergedPoly.Count; i++) verts.Add(UnprojectTo3D(mergedPoly[i], axis, backDepth));

        for (int i = 0; i < outerCapTris.Count; i += 3)
        {
            tris.Add(backCapOffset + outerCapTris[i]);
            tris.Add(backCapOffset + outerCapTris[i + 2]);
            tris.Add(backCapOffset + outerCapTris[i + 1]);
        }

        int tunnelFrontOffset = verts.Count;
        verts.AddRange(front3D);
        int tunnelBackOffset = verts.Count;
        verts.AddRange(back3D);

        int count = front3D.Count;
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            int f1 = tunnelFrontOffset + i;
            int f2 = tunnelFrontOffset + next;
            int b1 = tunnelBackOffset + i;
            int b2 = tunnelBackOffset + next;

            tris.Add(f1); tris.Add(f2); tris.Add(b1);
            tris.Add(f2); tris.Add(b2); tris.Add(b1);
        }

        AddBoxSides(b, axis, verts, tris);

        Mesh m = new Mesh { name = "OuterNotchedBlock" };
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private void AddBoxSides(Bounds b, Vector3 axis, List<Vector3> verts, List<int> tris)
    {
        Vector3 min = b.min;
        Vector3 max = b.max;

        Vector3[] c = new Vector3[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, max.z),
            new Vector3(min.x, max.y, max.z)
        };

        if (Mathf.Abs(axis.z) > 0.5f)
        {
            AddQuad(verts, tris, c[3], c[7], c[6], c[2]);
            AddQuad(verts, tris, c[0], c[1], c[5], c[4]);
            AddQuad(verts, tris, c[0], c[4], c[7], c[3]);
            AddQuad(verts, tris, c[1], c[2], c[6], c[5]);
        }
        else if (Mathf.Abs(axis.y) > 0.5f)
        {
            AddQuad(verts, tris, c[4], c[5], c[6], c[7]);
            AddQuad(verts, tris, c[0], c[3], c[2], c[1]);
            AddQuad(verts, tris, c[0], c[4], c[7], c[3]);
            AddQuad(verts, tris, c[1], c[2], c[6], c[5]);
        }
        else
        {
            AddQuad(verts, tris, c[3], c[7], c[6], c[2]);
            AddQuad(verts, tris, c[0], c[1], c[5], c[4]);
            AddQuad(verts, tris, c[4], c[5], c[6], c[7]);
            AddQuad(verts, tris, c[0], c[3], c[2], c[1]);
        }
    }

    private void AddQuad(List<Vector3> verts, List<int> tris, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int idx = verts.Count;
        verts.Add(v0); verts.Add(v1); verts.Add(v2); verts.Add(v3);

        tris.Add(idx); tris.Add(idx + 1); tris.Add(idx + 2);
        tris.Add(idx); tris.Add(idx + 2); tris.Add(idx + 3);
    }

    private Vector3 GetPrimaryAxis(Vector3 dir)
    {
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);
        float absZ = Mathf.Abs(dir.z);

        if (absX > absY && absX > absZ) return dir.x > 0 ? Vector3.right : Vector3.left;
        if (absY > absX && absY > absZ) return dir.y > 0 ? Vector3.up : Vector3.down;
        return dir.z > 0 ? Vector3.forward : Vector3.back;
    }

    private float GetThickness(Bounds b, Vector3 axis)
    {
        if (Mathf.Abs(axis.x) > 0.5f) return b.size.x;
        if (Mathf.Abs(axis.y) > 0.5f) return b.size.y;
        return b.size.z;
    }

    private float GetDepth(Bounds b, Vector3 axis, bool isFront)
    {
        if (axis == Vector3.right) return isFront ? b.max.x : b.min.x;
        if (axis == Vector3.left) return isFront ? b.min.x : b.max.x;
        if (axis == Vector3.up) return isFront ? b.max.y : b.min.y;
        if (axis == Vector3.down) return isFront ? b.min.y : b.max.y;
        if (axis == Vector3.forward) return isFront ? b.max.z : b.min.z;
        return isFront ? b.min.z : b.max.z;
    }

    private Vector2 ProjectTo2D(Vector3 p, Vector3 axis)
    {
        if (Mathf.Abs(axis.x) > 0.5f) return new Vector2(p.z, p.y);
        if (Mathf.Abs(axis.y) > 0.5f) return new Vector2(p.x, p.z);
        return new Vector2(p.x, p.y);
    }

    private Vector3 UnprojectTo3D(Vector2 p2D, Vector3 axis, float depth)
    {
        if (Mathf.Abs(axis.x) > 0.5f) return new Vector3(depth, p2D.y, p2D.x);
        if (Mathf.Abs(axis.y) > 0.5f) return new Vector3(p2D.x, depth, p2D.y);
        return new Vector3(p2D.x, p2D.y, depth);
    }

    private List<Vector2> GetQuadCorners2D(Bounds b, Vector3 axis)
    {
        if (Mathf.Abs(axis.x) > 0.5f)
        {
            return new List<Vector2>
        {
            new Vector2(b.min.z, b.min.y), new Vector2(b.max.z, b.min.y),
            new Vector2(b.max.z, b.max.y), new Vector2(b.min.z, b.max.y)
        };
        }
        if (Mathf.Abs(axis.y) > 0.5f)
        {
            return new List<Vector2>
        {
            new Vector2(b.min.x, b.min.z), new Vector2(b.max.x, b.min.z),
            new Vector2(b.max.x, b.max.z), new Vector2(b.min.x, b.max.z) // Coordonnée corrigée
        };
        }
        return new List<Vector2>
    {
        new Vector2(b.min.x, b.min.y), new Vector2(b.max.x, b.min.y),
        new Vector2(b.max.x, b.max.y), new Vector2(b.min.x, b.max.y)
    };
    }

    private List<Vector2> MergeQuadAndHole(List<Vector2> outer, List<Vector2> inner)
    {
        int bestOuter = 0, bestInner = 0;
        float minDst = float.MaxValue;

        for (int i = 0; i < outer.Count; i++)
        {
            for (int j = 0; j < inner.Count; j++)
            {
                float dst = Vector2.SqrMagnitude(outer[i] - inner[j]);
                if (dst < minDst)
                {
                    minDst = dst;
                    bestOuter = i;
                    bestInner = j;
                }
            }
        }

        List<Vector2> merged = new List<Vector2>();
        for (int i = 0; i <= bestOuter; i++) merged.Add(outer[i]);
        for (int j = 0; j < inner.Count; j++) merged.Add(inner[(bestInner + j) % inner.Count]);
        merged.Add(inner[bestInner]);
        merged.Add(outer[bestOuter]);
        for (int i = bestOuter + 1; i < outer.Count; i++) merged.Add(outer[i]);

        return merged;
    }
}