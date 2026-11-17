using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class NormalizedQuadGenerator : MonoBehaviour
{
    private void Start()
    {
        GenerateQuad();
    }

    [ContextMenu("Générer le Quad")]
    public void GenerateQuad()
    {
        Mesh mesh = new Mesh
        {
            name = "Normalized Quad"
        };

        // 1. Sommets centrés sur l'origine (taille 1x1)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0f, -0.5f), // 0: Bas-gauche
            new Vector3( 0.5f, 0f, -0.5f), // 1: Bas-droite
            new Vector3(-0.5f,  0f, 0.5f), // 2: Haut-gauche
            new Vector3( 0.5f,  0f, 0.5f)  // 3: Haut-droite
        };

        // 2. Coordonnées UV (0,0 à 1,1)
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        // 3. Triangles (sens horaire pour orienter la face vers +Z)
        int[] triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        // Calcul automatique des normales et tangentes
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;


        // Ajoute cette ligne pour synchroniser le collider immédiatement :
        if (TryGetComponent(out MeshCollider meshCollider))
        {
            meshCollider.sharedMesh = mesh;
        }
    }
}