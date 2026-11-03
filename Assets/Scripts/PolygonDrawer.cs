using System.Collections.Generic;
using Parabox.CSG;
using UnityEngine;
using UnityEngine.InputSystem; // Utilisation du New Input System

public class PolygonDrawer : MonoBehaviour
{
    [Header("Configuration")]
    public LayerMask cakeLayer;
    public float closeDistanceThreshold = 0.3f;
    public LineRenderer lineRenderer;

    [Header("Référence du gâteau")]
    public GameObject currentCake;

    private List<Vector3> worldPoints = new List<Vector3>();
    private bool isClosed = false;

    void Update()
    {
        if (isClosed || currentCake == null || Mouse.current == null) return;

        // Détection du clic gauche avec le New Input System
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, cakeLayer))
            {
                Vector3 newPoint = hit.point + hit.normal * 0.01f;

                if (worldPoints.Count >= 3 && Vector3.Distance(newPoint, worldPoints[0]) < closeDistanceThreshold)
                {
                    ClosePolygonAndCut();
                }
                else
                {
                    worldPoints.Add(newPoint);
                    UpdateLineRenderer();
                }
            }
        }
    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = worldPoints.Count;
        lineRenderer.SetPositions(worldPoints.ToArray());
    }

    void ClosePolygonAndCut()
    {
        isClosed = true;
        lineRenderer.loop = true;

        // Récupération du matériau du gâteau d'origine
        Material cakeMaterial = null;
        if (currentCake != null && currentCake.TryGetComponent<MeshRenderer>(out var mr))
        {
            cakeMaterial = mr.sharedMaterial;
        }

        // Passage du matériau au générateur
        GameObject prism = PrismGenerator.CreatePrism(worldPoints, height: 20f, cakeMaterial);

        if (prism != null)
        {
            PerformCut(currentCake, prism);
        }
    }

    private void PerformCut(GameObject cake, GameObject prism)
    {
        try
        {
            // 1. Exécution des opérations booléennes CSG
            Model resultCake = CSG.Subtract(cake, prism);
            Model resultPiece = CSG.Intersect(cake, prism);

            if (resultCake == null || resultPiece == null)
            {
                Debug.LogError("Échec du calcul CSG.");
                return;
            }

            // 2. Création des nouveaux GameObjects
            GameObject newCake = ModelToGameObject(resultCake, "Cake_Remaining");
            GameObject newPiece = ModelToGameObject(resultPiece, "Cake_Piece");

            newCake.transform.position = Vector3.zero;
            newCake.transform.rotation = Quaternion.identity;
            newCake.layer = cake.layer;

            newPiece.transform.position = Vector3.zero;
            newPiece.transform.rotation = Quaternion.identity;

            SetupCakeBody(newCake);
            SetupPieceBody(newPiece);

            currentCake = newCake;

            Destroy(cake);
            Destroy(prism);
            lineRenderer.positionCount = 0;
            worldPoints.Clear();
            isClosed = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors de la découpe : " + e.Message);
        }
        finally
        {
            // Garantit que le prisme de coupe est TOUJOURS détruit
            if (prism != null)
            {
                Destroy(prism);
            }

            lineRenderer.positionCount = 0;
            worldPoints.Clear();
            isClosed = false;
        }
    }
    /// <summary> Le gâteau reste immobile dans l'espace </summary>
    private void SetupCakeBody(GameObject obj)
    {
        MeshCollider collider = obj.AddComponent<MeshCollider>();
        collider.convex = false; // OBLIGATOIRE : permet de conserver le trou concave

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.isKinematic = true; // Requis par Unity quand convex est à false
    }

    private void SetupPieceBody(GameObject obj)
    {
        MeshCollider collider = obj.AddComponent<MeshCollider>();
        collider.convex = true; // La part isolée reste convexe pour pouvoir tomber avec la physique

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.useGravity = true;
    }
    private GameObject ModelToGameObject(Model model, string objectName)
    {
        GameObject go = new GameObject(objectName);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = model.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterials = model.materials.ToArray();

        return go;
    }

    private void SetupCutMesh(GameObject obj)
    {
        MeshCollider collider = obj.AddComponent<MeshCollider>();
        collider.convex = true;
        obj.AddComponent<Rigidbody>();
    }
}