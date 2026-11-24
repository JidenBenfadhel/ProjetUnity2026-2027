using System.Collections.Generic;
using UnityEngine;

public class MultiFaceMeshSplitter : MeshSplitter
{
    public bool SplitCorner(List<Vector3> worldPoints)
    {
        if (worldPoints == null || worldPoints.Count < 4) return false;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return false;

        Mesh srcMesh = mf.sharedMesh;
        Vector3[] srcVerts = srcMesh.vertices;
        int[] srcTris = srcMesh.triangles;

        // 1. Conversion de la boucle tracée en coordonnées locales
        List<Vector3> localLoop = new List<Vector3>();
        for (int i = 0; i < worldPoints.Count - 1; i++)
        {
            localLoop.Add(transform.InverseTransformPoint(worldPoints[i]));
        }

        // 2. Calcul du plan de coupe moyen à partir du tracé
        Vector3 loopCenter = Vector3.zero;
        foreach (Vector3 p in localLoop) loopCenter += p;
        loopCenter /= localLoop.Count;

        Vector3 planeNormal = ComputeAverageNormal(localLoop, loopCenter);
        Plane cutPlane = new Plane(planeNormal, loopCenter);

        // Listes pour le mesh du coin et le mesh restant
        List<Vector3> cornerVerts = new List<Vector3>();
        List<int> cornerTris = new List<int>();

        List<Vector3> mainVerts = new List<Vector3>();
        List<int> mainTris = new List<int>();

        List<Vector3> cutEdgePoints = new List<Vector3>();

        // 3. Traitement triangle par triangle (Découpe & Insertion de sommets)
        for (int i = 0; i < srcTris.Length; i += 3)
        {
            Vector3 v0 = srcVerts[srcTris[i]];
            Vector3 v1 = srcVerts[srcTris[i + 1]];
            Vector3 v2 = srcVerts[srcTris[i + 2]];

            bool side0 = cutPlane.GetSide(v0);
            bool side1 = cutPlane.GetSide(v1);
            bool side2 = cutPlane.GetSide(v2);

            // Cas A : Le triangle est entièrement dans le coin (+)
            if (side0 && side1 && side2)
            {
                AddTriangle(cornerVerts, cornerTris, v0, v1, v2);
            }
            // Cas B : Le triangle est entièrement dans le bloc principal (-)
            else if (!side0 && !side1 && !side2)
            {
                AddTriangle(mainVerts, mainTris, v0, v1, v2);
            }
            // Cas C : Le triangle croise le plan de coupe -> Subdivision
            else
            {
                SplitAndAddIntersectedTriangle(
                    cutPlane, v0, v1, v2, side0, side1, side2,
                    cornerVerts, cornerTris, mainVerts, mainTris, cutEdgePoints
                );
            }
        }

        if (cornerVerts.Count < 3 || mainVerts.Count < 3)
        {
            Debug.LogWarning("[MultiFaceSplitter] Échec : Aucun triangle n'a été coupé.");
            return false;
        }

        // 4. Triangulation de la surface interne de coupe (Bouchon)
        List<Vector2> cap2D = ProjectToPlane2D(localLoop, planeNormal);
        List<int> capTris = TriangulateSimplePolygon(cap2D);

        if (capTris.Count >= 3)
        {
            int cornerCapStart = cornerVerts.Count;
            cornerVerts.AddRange(localLoop);
            for (int i = 0; i < capTris.Count; i += 3)
            {
                cornerTris.Add(cornerCapStart + capTris[i]);
                cornerTris.Add(cornerCapStart + capTris[i + 1]);
                cornerTris.Add(cornerCapStart + capTris[i + 2]);
            }

            int mainCapStart = mainVerts.Count;
            mainVerts.AddRange(localLoop);
            for (int i = 0; i < capTris.Count; i += 3)
            {
                mainTris.Add(mainCapStart + capTris[i]);
                mainTris.Add(mainCapStart + capTris[i + 2]); // Inversé pour le bloc restant
                mainTris.Add(mainCapStart + capTris[i + 1]);
            }
        }

        // 5. Instanciation des deux nouveaux meshes
        Mesh cornerMesh = BuildFinalMesh(cornerVerts, cornerTris, "CornerMesh");
        CreateChildGameObject("Coin_Decoupe", cornerMesh);

        Mesh mainMesh = BuildFinalMesh(mainVerts, mainTris, "MainMesh");
        mf.sharedMesh = mainMesh;

        MeshCollider col = GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = mainMesh;
        }

        return true;
    }

    // --- ALGORITHME DE DECOUPE DU TRIANGLE EN ARÊTES ---

    private void SplitAndAddIntersectedTriangle(
        Plane plane, Vector3 v0, Vector3 v1, Vector3 v2,
        bool s0, bool s1, bool s2,
        List<Vector3> cVerts, List<int> cTris,
        List<Vector3> mVerts, List<int> mTris,
        List<Vector3> cutPoints)
    {
        // Re-trier les sommets pour que 'top' soit le sommet isolé d'un côté du plan
        Vector3 top, b1, b2;
        bool topSide;

        if (s0 != s1 && s0 != s2)
        {
            top = v0; b1 = v1; b2 = v2; topSide = s0;
        }
        else if (s1 != s0 && s1 != s2)
        {
            top = v1; b1 = v2; b2 = v0; topSide = s1;
        }
        else
        {
            top = v2; b1 = v0; b2 = v1; topSide = s2;
        }

        // Calcul des 2 nouveaux sommets d'intersection sur les arêtes
        Vector3 i1 = IntersectEdgeWithPlane(top, b1, plane);
        Vector3 i2 = IntersectEdgeWithPlane(top, b2, plane);

        cutPoints.Add(i1);
        cutPoints.Add(i2);

        // Si le sommet isolé est du côté du coin (+)
        if (topSide)
        {
            // 1 triangle pour le coin
            AddTriangle(cVerts, cTris, top, i1, i2);
            // 2 triangles (un quad) pour le bloc principal
            AddTriangle(mVerts, mTris, b1, b2, i1);
            AddTriangle(mVerts, mTris, b2, i2, i1);
        }
        else
        {
            // 1 triangle pour le bloc principal
            AddTriangle(mVerts, mTris, top, i1, i2);
            // 2 triangles (un quad) pour le coin
            AddTriangle(cVerts, cTris, b1, b2, i1);
            AddTriangle(cVerts, cTris, b2, i2, i1);
        }
    }

    private Vector3 IntersectEdgeWithPlane(Vector3 p1, Vector3 p2, Plane plane)
    {
        Ray ray = new Ray(p1, (p2 - p1).normalized);
        if (plane.Raycast(ray, out float enter))
        {
            return p1 + ray.direction * enter;
        }
        return (p1 + p2) * 0.5f;
    }

    private void AddTriangle(List<Vector3> verts, List<int> tris, Vector3 a, Vector3 b, Vector3 c)
    {
        int idx = verts.Count;
        verts.Add(a);
        verts.Add(b);
        verts.Add(c);

        tris.Add(idx);
        tris.Add(idx + 1);
        tris.Add(idx + 2);
    }

    private Vector3 ComputeAverageNormal(List<Vector3> loop, Vector3 center)
    {
        Vector3 normal = Vector3.zero;
        for (int i = 0; i < loop.Count; i++)
        {
            Vector3 current = loop[i] - center;
            Vector3 next = loop[(i + 1) % loop.Count] - center;
            normal += Vector3.Cross(current, next);
        }
        return normal.normalized;
    }

    private List<Vector2> ProjectToPlane2D(List<Vector3> loop, Vector3 planeNorm)
    {
        Quaternion rot = Quaternion.FromToRotation(planeNorm, Vector3.forward);
        List<Vector2> pts2D = new List<Vector2>();
        foreach (Vector3 p in loop)
        {
            Vector3 proj = rot * p;
            pts2D.Add(new Vector2(proj.x, proj.y));
        }
        return pts2D;
    }

    private Mesh BuildFinalMesh(List<Vector3> verts, List<int> tris, string name)
    {
        Mesh m = new Mesh { name = name };
        m.vertices = verts.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}