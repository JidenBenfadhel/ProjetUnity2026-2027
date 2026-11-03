using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    [Header("Rotation")]
    public float sensitivity = 0.15f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    [Header("Animation Vue UI")]
    public float transitionSpeed = 8f; // Vitesse de transition vers la vue préenregistrée

    private float horizontalAngle;
    private float verticalAngle;

    private float targetHorizontalAngle;
    private float targetVerticalAngle;
    private bool isTransitioning = false;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;

        horizontalAngle = angles.y;
        verticalAngle = angles.x;

        if (verticalAngle > 180f)
            verticalAngle -= 360f;

        targetHorizontalAngle = horizontalAngle;
        targetVerticalAngle = verticalAngle;
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        // Si le joueur clique/glisse, on interrompt la transition auto
        if (Mouse.current.middleButton.isPressed)
        {
            isTransitioning = false;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            horizontalAngle += mouseDelta.x * sensitivity;
            verticalAngle -= mouseDelta.y * sensitivity;

            verticalAngle = Mathf.Clamp(
                verticalAngle,
                minVerticalAngle,
                maxVerticalAngle
            );

            targetHorizontalAngle = horizontalAngle;
            targetVerticalAngle = verticalAngle;
        }

        // Animation progressive vers la cible quand un bouton est cliqué
        if (isTransitioning)
        {
            horizontalAngle = Mathf.LerpAngle(horizontalAngle, targetHorizontalAngle, Time.deltaTime * transitionSpeed);
            verticalAngle = Mathf.Lerp(verticalAngle, targetVerticalAngle, Time.deltaTime * transitionSpeed);

            if (Mathf.Abs(Mathf.DeltaAngle(horizontalAngle, targetHorizontalAngle)) < 0.05f &&
                Mathf.Abs(verticalAngle - targetVerticalAngle) < 0.05f)
            {
                horizontalAngle = targetHorizontalAngle;
                verticalAngle = targetVerticalAngle;
                isTransitioning = false;
            }
        }

        // Applique la rotation
        transform.rotation = Quaternion.Euler(
            verticalAngle,
            horizontalAngle,
            0f
        );
    }

    // --- Fonctions appelées par les Boutons UI ---

    /// <summary> Vue du dessus (plafonné par maxVerticalAngle) </summary>
    public void SetTopView()
    {
        SetTargetAngles(maxVerticalAngle, horizontalAngle);
    }

    /// <summary> Vue de face / côté sur l'axe X </summary>
    public void SetViewX()
    {
        SetTargetAngles(0f, 90f);
    }

    /// <summary> Vue de face / côté sur l'axe Z </summary>
    public void SetViewZ()
    {
        SetTargetAngles(0f, 0f);
    }

    private void SetTargetAngles(float vAngle, float hAngle)
    {
        targetVerticalAngle = Mathf.Clamp(vAngle, minVerticalAngle, maxVerticalAngle);
        targetHorizontalAngle = hAngle;
        isTransitioning = true;
    }
}