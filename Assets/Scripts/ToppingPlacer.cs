using UnityEngine;
using UnityEngine.InputSystem;

public class ToppingPlacer : MonoBehaviour
{
    [Header("Ajustements de placement")]
    [Tooltip("Petit décalage pour éviter que l'objet s'enfonce dans le gâteau")]
    public float spawnOffset = 0.02f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Gateau) return;

        // NOUVEAU : On s'arrête si l'outil actuel n'est pas la décoration
        if (ToolManager.Instance != null && ToolManager.Instance.currentTool != ToolType.Decoration) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceTopping();
        }
    }

    private void PlaceTopping()
    {
        GameObject prefabToPlace = ToolManager.Instance.currentDecorationPrefab;

        if (prefabToPlace == null)
        {
            Debug.LogWarning("ToppingPlacer : Aucun prefab sélectionné !");
            return;
        }

        Debug.Log("1. Clic pour placer un fruit détecté !");

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("2. Le Raycast a touché : " + hit.collider.name + " (Tag: " + hit.collider.tag + ")");

            if (hit.collider.CompareTag("Cake"))
            {
                Vector3 spawnPosition = hit.point + (hit.normal * spawnOffset);
                Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                GameObject newTopping = Instantiate(prefabToPlace, spawnPosition, spawnRotation);
                newTopping.transform.SetParent(hit.collider.transform.parent);

                Debug.Log("3. SUCCÈS ! Fruit instancié : " + newTopping.name);
            }
            else
            {
                Debug.LogWarning("ÉCHEC : L'objet touché n'a pas le tag 'Cake'.");
            }
        }
        else
        {
            Debug.LogWarning("ÉCHEC : Le Raycast a tiré dans le vide (ou est bloqué par l'interface UI).");
        }
    }
}