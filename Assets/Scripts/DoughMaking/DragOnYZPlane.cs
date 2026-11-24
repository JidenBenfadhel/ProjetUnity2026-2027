using UnityEngine;
using UnityEngine.InputSystem;

public class DragOnYZPlane : MonoBehaviour
{
    [Header("Filtrage (Optionnel)")]
    [Tooltip("Laisser sur Everything pour autoriser tous les objets, ou sélectionner un Layer précis.")]
    public LayerMask draggableLayers = ~0;

    [Header("Options de Déplacement")]
    [Tooltip("Conserve le décalage entre le point cliqué et le centre de l'objet pour éviter un saut au clic.")]
    public bool keepClickOffset = true;

    [Header("Options de Rotation (Plan YZ)")]
    [Tooltip("Vitesse de rotation en degrés par seconde avec les flèches Gauche/Droite.")]
    public float rotationSpeed = 120f;
    [Tooltip("Remet aussi la rotation d'origine quand on relâche le clic.")]
    public bool resetRotationOnRelease = true;

    protected Camera mainCam;
    protected Transform draggedObject;
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    protected float currentRotationAngle = 0f;

    private Vector3 grabOffset;
    private Plane yzPlane;

    protected virtual void Awake()
    {
        mainCam = Camera.main;
    }

    protected virtual void Update()
    {
        if (Mouse.current == null || mainCam == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryStartDrag();
        }
        else if (Mouse.current.leftButton.isPressed && draggedObject != null)
        {
            UpdateDrag();
            HandleRotation();
            OnDragUpdated();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && draggedObject != null)
        {
            EndDrag();
        }
    }

    /// <summary>
    /// Permet aux classes filles de filtrer quel objet peut être pris (et de remonter au parent si besoin).
    /// </summary>
    protected virtual bool CanDragObject(Transform hitTransform, out Transform targetObject)
    {
        targetObject = hitTransform;
        return targetObject != null;
    }

    protected virtual void OnDragStarted() { }
    protected virtual void OnDragUpdated() { }
    protected virtual void OnDragEnded() { }

    private void TryStartDrag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, draggableLayers))
        {
            if (!CanDragObject(hit.transform, out Transform target)) return;

            draggedObject = target;
            initialPosition = draggedObject.position;
            initialRotation = draggedObject.rotation;
            currentRotationAngle = 0f;

            yzPlane = new Plane(Vector3.right, initialPosition);

            if (keepClickOffset && yzPlane.Raycast(ray, out float enter))
            {
                Vector3 hitOnPlane = ray.GetPoint(enter);
                grabOffset = initialPosition - hitOnPlane;
            }
            else
            {
                grabOffset = Vector3.zero;
            }

            OnDragStarted();
        }
    }

    private void UpdateDrag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);

        if (yzPlane.Raycast(ray, out float enter))
        {
            Vector3 targetPos = ray.GetPoint(enter) + grabOffset;
            targetPos.x = initialPosition.x;
            draggedObject.position = targetPos;
        }
    }

    private void HandleRotation()
    {
        if (Keyboard.current == null) return;

        float direction = 0f;
        if (Keyboard.current.leftArrowKey.isPressed) direction -= 1f;
        if (Keyboard.current.rightArrowKey.isPressed) direction += 1f;

        if (Mathf.Abs(direction) > 0.01f)
        {
            float deltaAngle = direction * rotationSpeed * Time.deltaTime;
            draggedObject.Rotate(Vector3.right, deltaAngle, Space.World);
            currentRotationAngle += deltaAngle;
        }
    }

    private void EndDrag()
    {
        OnDragEnded();

        draggedObject.position = initialPosition;

        if (resetRotationOnRelease)
        {
            draggedObject.rotation = initialRotation;
        }

        currentRotationAngle = 0f;
        draggedObject = null;
    }
}