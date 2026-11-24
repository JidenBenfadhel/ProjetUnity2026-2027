using UnityEngine;
using UnityEngine.InputSystem;

public class FrostingManager : MonoBehaviour
{
    [Header("Paramètres de glaçage")]
    public Color currentColor = Color.red;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // On vérifie qu'on est bien à l'étape du gâteau
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Gateau)
            return;

        // On détecte le clic gauche sur le gateau
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ApplyFrosting();
        }
    }

    private void ApplyFrosting()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Cake"))
            {
                MeshRenderer cakeRenderer = hit.collider.GetComponent<MeshRenderer>();
                if (cakeRenderer != null)
                {
                    cakeRenderer.material.color = currentColor;
                    Debug.Log("Glaçage appliqué ! Nouvelle couleur : " + currentColor);
                }
            }
        }
    }
}