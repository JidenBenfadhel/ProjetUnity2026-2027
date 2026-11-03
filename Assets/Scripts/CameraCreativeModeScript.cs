using UnityEngine;
using UnityEngine.InputSystem;

public class CameraCreativeModeScript : MonoBehaviour
{
    [Header("Deplacement")]
    public float speed = 10f;
    public float sprintMultiplier = 3f;

    [Header("Souris")]
    public float mouseSensitivity = 2f;

    private float rotationX = 0f;
    private Transform playerCamera;

    void Start()
    {
        // Recuperer la camera enfant
        Camera cam = GetComponentInChildren<Camera>();

        if (cam != null)
        {
            playerCamera = cam.transform;
        }

        // Cacher et verrouiller la souris
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        LookAround();
        Move();

        // Echap : liberer la souris
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Clic gauche : reprendre la souris
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }



    void LookAround()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * 0.05f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.05f;

        transform.Rotate(Vector3.up * mouseX);

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation =
                Quaternion.Euler(rotationX, 0f, 0f);
        }
    }

    void Move()
    {
        if (Keyboard.current == null)
            return;

        float x = 0f;
        float z = 0f;
        float y = 0f;

        if (Keyboard.current.wKey.isPressed) z += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;

        if (Keyboard.current.spaceKey.isPressed) y += 1f;
        if (Keyboard.current.leftCtrlKey.isPressed) y -= 1f;

        Vector3 direction =
            transform.right * x +
            transform.forward * z +
            Vector3.up * y;

        direction = Vector3.ClampMagnitude(direction, 1f);

        float currentSpeed = speed;

        if (Keyboard.current.leftShiftKey.isPressed)
            currentSpeed *= sprintMultiplier;

        transform.position +=
            direction * currentSpeed * Time.deltaTime;
    }

}
