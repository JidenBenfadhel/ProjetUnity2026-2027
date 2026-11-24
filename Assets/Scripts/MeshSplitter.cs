using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MeshSplitter : MonoBehaviour
{
    public virtual bool SplitMesh(List<Vector3> worldPoints)
    {
        if (worldPoints == null || worldPoints.Count < 4) return false;

        // 1. Détection de l'orientation (Horizontal XZ vs Vertical XY)
        DetectMeshOrientation(out bool isHorizontal, out float planeDepth);

        // 2. Conversion Monde -> Local 2D
        List<Vector2> rawHole = new List<Vector2>();
        for (int i = 0; i < worldPoints.Count - 1; i++)
        {
            Vector3 localP = transform.InverseTransformPoint(worldPoints[i]);
            rawHole.Add(To2D(localP, isHorizontal));
        }

        List<Vector2> holePoly = CleanPolygon(rawHole);
        if (holePoly.Count < 3) return false;

        // 3. Mesh Intérieur
        List<Vector2> innerPoly = new List<Vector2>(holePoly);
        if (GetSignedArea(innerPoly) < 0) innerPoly.Reverse();

        List<int> innerTriangles = TriangulateSimplePolygon(innerPoly);
        if (innerTriangles.Count < 3) return false;

        Mesh innerMesh = BuildCleanMesh(innerPoly, innerTriangles, "InnerMesh", isHorizontal, planeDepth);
        CreateChildGameObject("Mesh_Interieur", innerMesh);

        // 4. Mesh Extérieur (Quad évidé)
        List<Vector2> quadPoly = GetQuadCorners(isHorizontal);
        if (GetSignedArea(quadPoly) < 0) quadPoly.Reverse();

        List<Vector2> holeCW = new List<Vector2>(holePoly);
        if (GetSignedArea(holeCW) > 0) holeCW.Reverse();

        List<Vector2> mergedOuterPoly = MergeQuadAndHole(quadPoly, holeCW);
        List<int> outerTriangles = TriangulateSimplePolygon(mergedOuterPoly);
        if (outerTriangles.Count < 3) return false;

        Mesh outerMesh = BuildCleanMesh(mergedOuterPoly, outerTriangles, "OuterMesh", isHorizontal, planeDepth);

        GetComponent<MeshFilter>().sharedMesh = outerMesh;
        MeshCollider mainCollider = GetComponent<MeshCollider>();
        if (mainCollider != null)
        {
            mainCollider.sharedMesh = null;
            mainCollider.sharedMesh = outerMesh;
        }

        return true;
    }

    // ==========================================
    // MÉTHODES PROTECTED (RÉUTILISABLES PAR MESHSPLITTER3D)
    // ==========================================

    protected List<Vector2> CleanPolygon(List<Vector2> poly)
    {
        List<Vector2> cleaned = new List<Vector2>();
        if (poly == null || poly.Count < 3) return cleaned;

        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 curr = poly[i];
            Vector2 next = poly[(i + 1) % poly.Count];
            if ((curr - next).sqrMagnitude > 0.000001f)
            {
                cleaned.Add(curr);
            }
        }
        return cleaned;
    }

    protected float GetSignedArea(List<Vector2> poly)
    {
        float area = 0f;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p1 = poly[i];
            Vector2 p2 = poly[(i + 1) % poly.Count];
            area += (p1.x * p2.y - p2.x * p1.y);
        }
        return area * 0.5f;
    }

    protected List<int> TriangulateSimplePolygon(List<Vector2> vertices)
    {
        List<int> indices = new List<int>();
        int n = vertices.Count;
        if (n < 3) return indices;

        if (n == 3)
        {
            indices.AddRange(new int[] { 0, 1, 2 });
            return indices;
        }

        List<int> V = new List<int>();
        for (int i = 0; i < n; i++) V.Add(i);

        int count = V.Count;
        int maxAttempts = count * count * 2;
        int attempts = 0;

        while (count > 2 && attempts < maxAttempts)
        {
            attempts++;
            bool earFound = false;

            for (int i = 0; i < count; i++)
            {
                int prevIdx = V[(i - 1 + count) % count];
                int currIdx = V[i];
                int nextIdx = V[(i + 1) % count];

                Vector2 a = vertices[prevIdx];
                Vector2 b = vertices[currIdx];
                Vector2 c = vertices[nextIdx];

                float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
                if (cross <= 1e-7f) continue;

                bool hasInside = false;
                for (int j = 0; j < count; j++)
                {
                    int checkIdx = V[j];
                    if (checkIdx == prevIdx || checkIdx == currIdx || checkIdx == nextIdx) continue;

                    Vector2 pt = vertices[checkIdx];
                    if ((pt - a).sqrMagnitude < 1e-7f || (pt - b).sqrMagnitude < 1e-7f || (pt - c).sqrMagnitude < 1e-7f)
                        continue;

                    if (IsPointInTriangle(pt, a, b, c))
                    {
                        hasInside = true;
                        break;
                    }
                }

                if (!hasInside)
                {
                    indices.Add(prevIdx);
                    indices.Add(currIdx);
                    indices.Add(nextIdx);

                    V.RemoveAt(i);
                    count--;
                    earFound = true;
                    break;
                }
            }

            if (!earFound) break;
        }

        return indices;
    }

    protected bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float det = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        if (Mathf.Abs(det) < 1e-8f) return false;

        float u = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / det;
        float v = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / det;
        float w = 1.0f - u - v;

        return u > 1e-5f && v > 1e-5f && w > 1e-5f;
    }

    protected void CreateChildGameObject(string objName, Mesh mesh)
    {
        GameObject child = new GameObject(objName);
        child.transform.SetParent(transform.parent, false);
        child.transform.position = transform.position;
        child.transform.rotation = transform.rotation;
        child.transform.localScale = transform.localScale;

        child.AddComponent<MeshFilter>().sharedMesh = mesh;
        child.AddComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        if (mesh != null && mesh.triangles != null && mesh.triangles.Length >= 3)
        {
            child.AddComponent<MeshCollider>().sharedMesh = mesh;
        }
    }

    // --- Helpers de conversion 2D/3D ---

    protected void DetectMeshOrientation(out bool isHorizontal, out float planeDepth)
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Bounds bounds = (mesh != null) ? mesh.bounds : new Bounds(Vector3.zero, Vector3.one);
        isHorizontal = bounds.size.y < bounds.size.z;
        planeDepth = isHorizontal ? bounds.center.y : bounds.center.z;
    }

    protected Vector2 To2D(Vector3 localPos, bool isHorizontal)
    {
        return isHorizontal ? new Vector2(localPos.x, localPos.z) : new Vector2(localPos.x, localPos.y);
    }

    protected Vector3 To3D(Vector2 pos2D, bool isHorizontal, float depth)
    {
        return isHorizontal ? new Vector3(pos2D.x, depth, pos2D.y) : new Vector3(pos2D.x, pos2D.y, depth);
    }

    private List<Vector2> GetQuadCorners(bool isHorizontal)
    {
        Mesh m = GetComponent<MeshFilter>().sharedMesh;
        Bounds b = (m != null) ? m.bounds : new Bounds(Vector3.zero, Vector3.one);

        if (isHorizontal)
        {
            return new List<Vector2>
            {
                new Vector2(b.min.x, b.min.z), new Vector2(b.max.x, b.min.z),
                new Vector2(b.max.x, b.max.z), new Vector2(b.min.x, b.max.z)
            };
        }

        return new List<Vector2>
        {
            new Vector2(b.min.x, b.min.y), new Vector2(b.max.x, b.min.y),
            new Vector2(b.max.x, b.max.y), new Vector2(b.min.x, b.min.y)
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

    private Mesh BuildCleanMesh(List<Vector2> verts2D, List<int> rawTriangles, string name, bool isHorizontal, float depth)
    {
        List<Vector3> uniqueVertices = new List<Vector3>();
        List<Vector2> uniqueUVs = new List<Vector2>();
        int[] indexMap = new int[verts2D.Count];

        for (int i = 0; i < verts2D.Count; i++)
        {
            Vector2 p = verts2D[i];
            int existing = -1;

            for (int j = 0; j < uniqueVertices.Count; j++)
            {
                Vector2 existing2D = To2D(uniqueVertices[j], isHorizontal);
                if (Vector2.SqrMagnitude(existing2D - p) < 1e-8f)
                {
                    existing = j;
                    break;
                }
            }

            if (existing != -1)
            {
                indexMap[i] = existing;
            }
            else
            {
                int newIdx = uniqueVertices.Count;
                uniqueVertices.Add(To3D(p, isHorizontal, depth));
                uniqueUVs.Add(new Vector2(p.x + 0.5f, p.y + 0.5f));
                indexMap[i] = newIdx;
            }
        }

        List<int> cleanTriangles = new List<int>();
        for (int i = 0; i < rawTriangles.Count; i += 3)
        {
            int i0 = indexMap[rawTriangles[i]];
            int i1 = indexMap[rawTriangles[i + 1]];
            int i2 = indexMap[rawTriangles[i + 2]];

            if (i0 != i1 && i1 != i2 && i0 != i2)
            {
                cleanTriangles.Add(i0);
                cleanTriangles.Add(i1);
                cleanTriangles.Add(i2);
            }
        }

        Mesh mesh = new Mesh { name = name };
        mesh.vertices = uniqueVertices.ToArray();
        mesh.uv = uniqueUVs.ToArray();
        mesh.triangles = cleanTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}