using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class BoxMeshGenerator : MonoBehaviour
{
    public Vector3 boxSize = new Vector3(1f, 0.5f, 1f);

    private void Awake()
    {
        GenerateBox();
    }

    [ContextMenu("Re-generate Box")]
    public void GenerateBox()
    {
        Mesh mesh = new Mesh { name = "InitialBox" };

        float x = boxSize.x * 0.5f;
        float y = boxSize.y * 0.5f;
        float z = boxSize.z * 0.5f;

        Vector3[] vertices = new Vector3[]
        {
            // Face Avant (+Z)
            new Vector3(-x, -y,  z), new Vector3( x, -y,  z), new Vector3( x,  y,  z), new Vector3(-x,  y,  z),
            // Face Arrière (-Z)
            new Vector3( x, -y, -z), new Vector3(-x, -y, -z), new Vector3(-x,  y, -z), new Vector3( x,  y, -z),
            // Face Supérieure (+Y)
            new Vector3(-x,  y,  z), new Vector3( x,  y,  z), new Vector3( x,  y, -z), new Vector3(-x,  y, -z),
            // Face Inférieure (-Y)
            new Vector3(-x, -y, -z), new Vector3( x, -y, -z), new Vector3( x, -y,  z), new Vector3(-x, -y,  z),
            // Face Droite (+X)
            new Vector3( x, -y,  z), new Vector3( x, -y, -z), new Vector3( x,  y, -z), new Vector3( x,  y,  z),
            // Face Gauche (-X)
            new Vector3(-x, -y, -z), new Vector3(-x, -y,  z), new Vector3(-x,  y,  z), new Vector3(-x,  y, -z)
        };

        int[] triangles = new int[36];
        for (int i = 0; i < 6; i++)
        {
            int vIdx = i * 4;
            int tIdx = i * 6;

            // Enroulement horaire pour pointer les normales vers l'extérieur du volume
            triangles[tIdx] = vIdx;
            triangles[tIdx + 1] = vIdx + 1;
            triangles[tIdx + 2] = vIdx + 2;
            triangles[tIdx + 3] = vIdx;
            triangles[tIdx + 4] = vIdx + 2;
            triangles[tIdx + 5] = vIdx + 3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = null; // Re-bake PhysX
            collider.sharedMesh = mesh;
        }
    }
}