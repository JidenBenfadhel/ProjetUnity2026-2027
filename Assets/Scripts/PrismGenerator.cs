using System.Collections.Generic;
using UnityEngine;

public static class PrismGenerator
{
    public static GameObject CreatePrism(List<Vector3> surfacePoints, float height, Material defaultMaterial)
    {
        int count = surfacePoints.Count;
        if (count < 3) return null;

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[count * 2];
        List<int> triangles = new List<int>();

        float halfHeight = height / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 pt = surfacePoints[i];
            vertices[i] = new Vector3(pt.x, pt.y + halfHeight, pt.z);
            vertices[i + count] = new Vector3(pt.x, pt.y - halfHeight, pt.z);
        }

        float area = 0f;
        for (int i = 0; i < count; i++)
        {
            Vector3 p1 = surfacePoints[i];
            Vector3 p2 = surfacePoints[(i + 1) % count];
            area += (p2.x - p1.x) * (p2.z + p1.z);
        }
        bool isClockwise = area > 0;

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            int topCurrent = i;
            int topNext = next;
            int bottomCurrent = i + count;
            int bottomNext = next + count;

            if (isClockwise)
            {
                triangles.Add(topCurrent);
                triangles.Add(bottomCurrent);
                triangles.Add(topNext);

                triangles.Add(topNext);
                triangles.Add(bottomCurrent);
                triangles.Add(bottomNext);
            }
            else
            {
                triangles.Add(topCurrent);
                triangles.Add(topNext);
                triangles.Add(bottomCurrent);

                triangles.Add(topNext);
                triangles.Add(bottomNext);
                triangles.Add(bottomCurrent);
            }
        }

        for (int i = 1; i < count - 1; i++)
        {
            if (isClockwise)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i);

                triangles.Add(count);
                triangles.Add(count + i);
                triangles.Add(count + i + 1);
            }
            else
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);

                triangles.Add(count);
                triangles.Add(count + i + 1);
                triangles.Add(count + i);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject prismObj = new GameObject("CutterPrism");
        MeshFilter mf = prismObj.AddComponent<MeshFilter>();
        MeshRenderer mr = prismObj.AddComponent<MeshRenderer>();
        mf.mesh = mesh;

        // Attribuer le matériau pour éviter l'erreur de clé null dans pb_CSG
        mr.sharedMaterial = defaultMaterial != null ? defaultMaterial : new Material(Shader.Find("Sprites/Default"));
        mr.enabled = false;

        return prismObj;
    }
}