using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DragDough : DragOnYZPlane
{
    [Header("Identification de l'objet Mold")]
    [Tooltip("Nom ou Tag attendu pour l'objet moule à déplacer (insensible à la casse).")]
    public string moldIdentifier = "mold";

    [Header("Références Scène (Optionnel - recherche auto si vide)")]
    [Tooltip("L'objet Stove sur lequel le moule doit être lâché.")]
    public Transform stoveObject;
    public string stoveIdentifier = "stove";

    [Tooltip("Le repère cible où placer le moule (doughstoveplacement).")]
    public Transform doughStovePlacement;
    public string placementIdentifier = "doughstoveplacement";

    [Header("Options de Détection & Placement")]
    [Tooltip("Distance maximale sur le plan YZ entre mold et stove si leurs colliders ne se touchent pas.")]
    public float dropDistanceThreshold = 0.6f;
    [Tooltip("Applique aussi la rotation de doughstoveplacement à l'objet mold.")]
    public bool matchPlacementRotation = true;

    [Header("Navigation UI & Scène suivante")]
    [Tooltip("Le GameObject du bouton UI à afficher une fois le moule placé dans le stove.")]
    public GameObject nextStepButton;
    [Tooltip("Nom exact de la scène suivante (doit être dans les Build Settings).")]
    public string nextSceneName = "FinalScene";

    private Collider moldCollider;
    private Collider stoveCollider;

    protected override void Awake()
    {
        base.Awake();
        ResolveSceneReferences();

        // Masque le bouton au démarrage et branche le chargement de la scène suivante
        if (nextStepButton != null)
        {
            nextStepButton.SetActive(false);

            Button btn = nextStepButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(LoadNextScene);
                btn.onClick.AddListener(LoadNextScene);
            }
        }
    }

    /// <summary>
    /// Filtre : n'autorise le drag que sur l'objet "mold" (ou un de ses enfants).
    /// </summary>
    protected override bool CanDragObject(Transform hitTransform, out Transform targetObject)
    {
        targetObject = null;

        Transform candidate = hitTransform;
        if (!MatchesIdentifier(candidate, moldIdentifier) && candidate.parent != null && MatchesIdentifier(candidate.parent, moldIdentifier))
        {
            candidate = candidate.parent;
        }

        if (!MatchesIdentifier(candidate, moldIdentifier)) return false;

        targetObject = candidate;
        return true;
    }

    protected override void OnDragStarted()
    {
        if (draggedObject == null) return;

        ResolveSceneReferences();
        moldCollider = draggedObject.GetComponentInChildren<Collider>();
        if (stoveObject != null)
        {
            stoveCollider = stoveObject.GetComponentInChildren<Collider>();
        }
    }

    protected override void OnDragEnded()
    {
        if (draggedObject != null && IsMoldOverStove())
        {
            PlaceMoldOnStove();
        }

        moldCollider = null;
    }

    private bool IsMoldOverStove()
    {
        if (stoveObject == null) return false;

        // 1. Vérification par chevauchement des Colliders (Bounds)
        if (moldCollider != null && stoveCollider != null)
        {
            if (moldCollider.bounds.Intersects(stoveCollider.bounds))
            {
                return true;
            }
        }

        // 2. Vérification par distance sur le plan YZ (sécurité si les X sont décalés ou les colliders petits)
        Vector2 moldYZ = new Vector2(draggedObject.position.y, draggedObject.position.z);
        Vector2 stoveYZ = new Vector2(stoveObject.position.y, stoveObject.position.z);

        return Vector2.Distance(moldYZ, stoveYZ) <= dropDistanceThreshold;
    }

    private void PlaceMoldOnStove()
    {
        if (doughStovePlacement == null)
        {
            Debug.LogWarning("[DragDough] Impossible de placer 'mold' : vérifiez que 'doughstoveplacement' existe dans la scène.");
            return;
        }

        // Met à jour initialPosition / initialRotation pour que EndDrag() conserve cette position
        initialPosition = doughStovePlacement.position;
        draggedObject.position = initialPosition;

        if (matchPlacementRotation)
        {
            initialRotation = doughStovePlacement.rotation;
            draggedObject.rotation = initialRotation;
        }

        // Affiche le bouton permettant de passer à la scène suivante
        if (nextStepButton != null)
        {
            nextStepButton.SetActive(true);
        }
    }

    /// <summary>
    /// Charge la scène suivante (relié automatiquement à nextStepButton).
    /// </summary>
    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("[DragDough] Le nom de la scène suivante n'est pas renseigné.");
            return;
        }

        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }

    // =================================================================
    // HELPERS DE RECHERCHE DANS LA SCÈNE
    // =================================================================

    private void ResolveSceneReferences()
    {
        if (stoveObject == null) stoveObject = FindObjectByIdentifier(stoveIdentifier);
        if (doughStovePlacement == null) doughStovePlacement = FindObjectByIdentifier(placementIdentifier);
    }

    private Transform FindObjectByIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) return null;

        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];
            if (!t.gameObject.scene.IsValid()) continue;

            if (MatchesIdentifier(t, identifier))
            {
                return t;
            }
        }

        return null;
    }

    private bool MatchesIdentifier(Transform obj, string identifier)
    {
        if (obj == null || string.IsNullOrEmpty(identifier)) return false;

        bool matchesName = obj.name.Equals(identifier, System.StringComparison.OrdinalIgnoreCase);
        bool matchesTag = false;

        try
        {
            matchesTag = obj.CompareTag(identifier);
        }
        catch (UnityException)
        {
        }

        return matchesName || matchesTag;
    }
}