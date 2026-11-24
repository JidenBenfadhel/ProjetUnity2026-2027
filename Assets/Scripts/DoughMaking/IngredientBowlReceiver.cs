using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngredientBowlReceiver : MonoBehaviour
{
    [Header("Référence du Bol")]
    [Tooltip("Laisser vide si ce script est déjà attaché sur l'objet 'bowl'.")]
    public Transform bowlObject;
    [Tooltip("Marge ajoutée autour du bol pour détecter les particules qui tombent dedans.")]
    public Vector3 detectionPadding = new Vector3(0.05f, 0.08f, 0.05f);

    [Header("Noms des points d'émission des ingrédients")]
    public string flourPointName = "pouringpointflour";
    public string sugarPointName = "pouringpointsugar";
    public string eggPointName = "pouringpointegg";

    [Header("Temps requis dans le bol (en secondes)")]
    [Tooltip("Durée pendant laquelle les particules de farine doivent tomber dans le bol.")]
    public float requiredFlourDuration = 2.0f;
    [Tooltip("Durée pendant laquelle les particules de sucre doivent tomber dans le bol.")]
    public float requiredSugarDuration = 2.0f;
    [Tooltip("Durée pendant laquelle les particules d'œuf doivent tomber dans le bol.")]
    public float requiredEggDuration = 2.0f;

    [Header("Navigation UI & Scène suivante")]
    [Tooltip("Le GameObject du bouton UI à afficher une fois les 3 ingrédients versés.")]
    public GameObject nextStepButton;
    [Tooltip("Nom exact de la scène suivante (doit être dans les Build Settings).")]
    public string nextSceneName = "BakingScene";

    [Header("Suivi en direct (Lecture seule)")]
    [SerializeField] private float currentFlourTime = 0f;
    [SerializeField] private float currentSugarTime = 0f;
    [SerializeField] private float currentEggTime = 0f;

    private Collider bowlCollider;
    private ParticleSystem flourParticles;
    private ParticleSystem sugarParticles;
    private ParticleSystem eggParticles;
    private ParticleSystem.Particle[] particleBuffer = new ParticleSystem.Particle[256];

    private bool isCompleted = false;

    private void Awake()
    {
        if (bowlObject == null) bowlObject = transform;
        bowlCollider = bowlObject.GetComponentInChildren<Collider>();

        // Cache le bouton au démarrage
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

    private void Update()
    {
        if (isCompleted) return;

        // Recherche paresseuse des ParticleSystems (au cas où ils sont créés dynamiquement au premier clic)
        if (flourParticles == null) flourParticles = FindParticleSystemByPointName(flourPointName);
        if (sugarParticles == null) sugarParticles = FindParticleSystemByPointName(sugarPointName);
        if (eggParticles == null) eggParticles = FindParticleSystemByPointName(eggPointName);

        Bounds bowlZone = GetDetectionBounds();

        // Accumule le temps uniquement lorsque des particules de l'ingrédient atteignent le bol
        if (currentFlourTime < requiredFlourDuration && IsReceivingParticlesFrom(flourParticles, bowlZone))
        {
            currentFlourTime = Mathf.Min(requiredFlourDuration, currentFlourTime + Time.deltaTime);
        }

        if (currentSugarTime < requiredSugarDuration && IsReceivingParticlesFrom(sugarParticles, bowlZone))
        {
            currentSugarTime = Mathf.Min(requiredSugarDuration, currentSugarTime + Time.deltaTime);
        }

        if (currentEggTime < requiredEggDuration && IsReceivingParticlesFrom(eggParticles, bowlZone))
        {
            currentEggTime = Mathf.Min(requiredEggDuration, currentEggTime + Time.deltaTime);
        }

        // Vérifie si les 3 ingrédients ont été versés assez longtemps
        if (currentFlourTime >= requiredFlourDuration &&
            currentSugarTime >= requiredSugarDuration &&
            currentEggTime >= requiredEggDuration)
        {
            CompleteMixingStep();
        }
    }

    private bool IsReceivingParticlesFrom(ParticleSystem ps, Bounds bowlZone)
    {
        if (ps == null || ps.particleCount == 0) return false;

        int count = ps.GetParticles(particleBuffer);
        bool isWorldSpace = ps.main.simulationSpace == ParticleSystemSimulationSpace.World;

        for (int i = 0; i < count; i++)
        {
            Vector3 worldPos = isWorldSpace
                ? particleBuffer[i].position
                : ps.transform.TransformPoint(particleBuffer[i].position);

            if (bowlZone.Contains(worldPos))
            {
                return true;
            }
        }

        return false;
    }

    private Bounds GetDetectionBounds()
    {
        Bounds b = (bowlCollider != null)
            ? bowlCollider.bounds
            : new Bounds(bowlObject.position, Vector3.one * 0.3f);

        b.Expand(detectionPadding);
        return b;
    }

    private void CompleteMixingStep()
    {
        isCompleted = true;

        if (nextStepButton != null)
        {
            nextStepButton.SetActive(true);
        }
    }

    /// <summary>
    /// Charge la scène suivante (branché automatiquement sur nextStepButton, ou appelable via OnClick).
    /// </summary>
    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("[IngredientBowlReceiver] Le nom de la scène suivante n'est pas renseigné.");
            return;
        }

        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }

    private ParticleSystem FindParticleSystemByPointName(string pointName)
    {
        if (string.IsNullOrEmpty(pointName)) return null;

        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];
            if (!t.gameObject.scene.IsValid()) continue;

            if (t.name.Equals(pointName, System.StringComparison.OrdinalIgnoreCase))
            {
                return t.GetComponent<ParticleSystem>();
            }
        }

        return null;
    }

    // Affiche la zone de détection du bol dans l'éditeur Unity (Gizmo vert)
    private void OnDrawGizmosSelected()
    {
        Transform target = bowlObject != null ? bowlObject : transform;
        Collider col = target.GetComponentInChildren<Collider>();
        Bounds b = (col != null) ? col.bounds : new Bounds(target.position, Vector3.one * 0.3f);
        b.Expand(detectionPadding);

        Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.35f);
        Gizmos.DrawCube(b.center, b.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}