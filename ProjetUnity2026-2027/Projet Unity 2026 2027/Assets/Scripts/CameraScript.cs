
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    [Header("Rotation")]
    public float sensitivity = 0.15f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    private float horizontalAngle;
    private float verticalAngle;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;

        horizontalAngle = angles.y;
        verticalAngle = angles.x;

        if (verticalAngle > 180f)
            verticalAngle -= 360f;
    }

    void Update()
    {
        // Vérifie qu'une souris est disponible
        if (Mouse.current == null)
            return;

        // Rotation uniquement si le clic gauche est maintenu
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // Rotation horizontale et verticale
            horizontalAngle += mouseDelta.x * sensitivity;
            verticalAngle -= mouseDelta.y * sensitivity;

            // Limite l'inclinaison verticale
            verticalAngle = Mathf.Clamp(
                verticalAngle,
                minVerticalAngle,
                maxVerticalAngle
            );

            // Applique la rotation au pivot
            transform.rotation = Quaternion.Euler(
                verticalAngle,
                horizontalAngle,
                0f
            );
        }
    }
}
