using UnityEngine;

public class BoutiqueCameraIntro : MonoBehaviour
{
    [Header("Points de trajet de la caméra")]
    [Tooltip("Position de départ (plan large ou extérieur de la boutique)")]
    public Transform startPoint;
    [Tooltip("Position finale (face au comptoir / client)")]
    public Transform targetPoint;

    [Header("Paramètres du zoom")]
    public float transitionDuration = 2.0f;
    public float startFOV = 70f;
    public float targetFOV = 55f;

    private Camera cam;
    private float elapsedTime = 0f;
    private bool isTransitioning = true;

    private void Start()
    {
        cam = GetComponent<Camera>();

        // Garde le curseur visible et libre pour cliquer sur les boutons/ingrédients
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Place la caméra au point de départ dès le chargement de la scène
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
        }

        if (cam != null)
        {
            cam.fieldOfView = startFOV;
        }
    }

    private void Update()
    {
        if (!isTransitioning || startPoint == null || targetPoint == null)
            return;

        elapsedTime += Time.deltaTime;
        float rawProgress = Mathf.Clamp01(elapsedTime / transitionDuration);

        // SmoothStep adoucit le démarrage et freine naturellement à l'arrivée
        float smoothProgress = Mathf.SmoothStep(0f, 1f, rawProgress);

        // Déplacement et orientation progressifs
        transform.position = Vector3.Lerp(startPoint.position, targetPoint.position, smoothProgress);
        transform.rotation = Quaternion.Slerp(startPoint.rotation, targetPoint.rotation, smoothProgress);

        // Zoom optique progressif (réduction du champ de vision)
        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, smoothProgress);
        }

        if (rawProgress >= 1f)
        {
            isTransitioning = false;
        }
    }
}