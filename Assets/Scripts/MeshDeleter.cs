using UnityEngine;
using UnityEngine.InputSystem;

public class MeshDeleter : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Script de dessin à désactiver lorsque le mode suppression est actif")]
    public MeshSegmentDrawer segmentDrawer;

    [Header("Surbrillance")]
    [Tooltip("Matériau jaune appliqué au survol (Optionnel). Si vide, un matériau jaune est généré automatiquement.")]
    public Material highlightMaterial;

    [Header("État")]
    public bool isDeleteMode = false;

    private Renderer currentHoveredRenderer;
    private Material[] originalMaterials;
    private Material defaultYellowMaterial;

    private void Start()
    {
        // Force PhysX à détecter les Raycasts sur les faces arrière (Backfaces)
        Physics.queriesHitBackfaces = true;

        if (highlightMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            defaultYellowMaterial = new Material(shader)
            {
                color = Color.yellow
            };
        }
    }

    public void ToggleDeleteMode()
    {
        SetDeleteMode(!isDeleteMode);
    }

    public void SetDeleteMode(bool active)
    {
        isDeleteMode = active;

        if (segmentDrawer != null)
        {
            segmentDrawer.enabled = !isDeleteMode;
        }

        if (!isDeleteMode)
        {
            ResetHighlight();
        }
    }

    private void Update()
    {
        if (!isDeleteMode || Mouse.current == null)
        {
            ResetHighlight();
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.transform.gameObject;
            Renderer hitRenderer = hitObject.GetComponent<Renderer>();

            // Vérifie que l'objet possède un mesh et un renderer
            if (hitRenderer != null && hitObject.GetComponent<MeshFilter>() != null)
            {
                // Si le curseur change d'objet survolé
                if (hitRenderer != currentHoveredRenderer)
                {
                    ResetHighlight();
                    ApplyHighlight(hitRenderer);
                }

                // Clic gauche : suppression
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ResetHighlight();
                    Destroy(hitObject);
                }
                return;
            }
        }

        // Si la souris ne pointe sur aucun mesh
        ResetHighlight();
    }

    private void ApplyHighlight(Renderer targetRenderer)
    {
        currentHoveredRenderer = targetRenderer;
        originalMaterials = targetRenderer.sharedMaterials;

        Material activeMat = highlightMaterial != null ? highlightMaterial : defaultYellowMaterial;

        // Remplace tous les matériaux de l'objet par le matériau jaune
        Material[] highlightArray = new Material[originalMaterials.Length];
        for (int i = 0; i < highlightArray.Length; i++)
        {
            highlightArray[i] = activeMat;
        }

        targetRenderer.materials = highlightArray;
    }

    private void ResetHighlight()
    {
        if (currentHoveredRenderer != null)
        {
            currentHoveredRenderer.materials = originalMaterials;
            currentHoveredRenderer = null;
            originalMaterials = null;
        }
    }

    private void OnDisable()
    {
        ResetHighlight();
    }
}