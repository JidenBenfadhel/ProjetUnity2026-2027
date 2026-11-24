using UnityEngine;

public class DragEgg : DragOnYZPlane
{
    [Header("Identification de l'objet Egg")]
    [Tooltip("Nom ou Tag attendu pour l'objet œuf (insensible à la casse).")]
    public string eggIdentifier = "egg";

    [Header("Détection de Choc & Collision")]
    [Tooltip("Layers des objets contre lesquels l'œuf peut être frappé et bloqués.")]
    public LayerMask impactLayers = ~0;
    [Tooltip("Vitesse minimale (en unités/seconde) pour considérer le contact comme un coup.")]
    public float minImpactSpeed = 1.5f;
    [Tooltip("Délai minimum (en secondes) entre deux impacts consécutifs.")]
    public float impactCooldown = 0.4f;
    [Tooltip("Nombre d'itérations de résolution de collision (2 à 4 recommandé pour éviter tout blocage dans les coins).")]
    [Range(1, 5)]
    public int collisionIterations = 3;

    [Header("Particules Jaunes (pouringpointegg)")]
    [Tooltip("Nom exact du Transform enfant d'où s'échappent les particules jaunes.")]
    public string spawnPointChildName = "pouringpointegg";
    [Tooltip("Nombre de particules jaunes projetées instantanément au moment du choc.")]
    public int burstParticleCount = 35;
    [Tooltip("Débit continu de particules jaunes par seconde une fois l'œuf cassé.")]
    public float continuousFlowRate = 60f;
    [Tooltip("Crée automatiquement un ParticleSystem jaune si 'pouringpointegg' n'en possède pas encore.")]
    public bool autoCreateParticleSystemIfMissing = true;

    private ParticleSystem currentEggParticles;
    private Transform resolvedEggPoint;
    private Collider eggCollider;

    private Vector3 previousPosition;
    private float currentSpeed;
    private bool wasTouchingObstacle;
    private bool isCracked;
    private float lastImpactTime = -10f;

    /// <summary>
    /// Filtre : n'autorise le drag que sur l'objet "egg" (ou un de ses enfants).
    /// </summary>
    protected override bool CanDragObject(Transform hitTransform, out Transform targetObject)
    {
        targetObject = null;
        resolvedEggPoint = null;

        Transform candidate = hitTransform;
        if (!IsEggObject(candidate) && candidate.parent != null && IsEggObject(candidate.parent))
        {
            candidate = candidate.parent;
        }

        if (!IsEggObject(candidate)) return false;

        resolvedEggPoint = FindChildRecursive(candidate, spawnPointChildName);
        if (resolvedEggPoint == null)
        {
            Debug.LogWarning($"[DragEgg] L'objet '{candidate.name}' a été détecté mais ne possède pas d'enfant '{spawnPointChildName}'.");
            return false;
        }

        targetObject = candidate;
        return true;
    }

    protected override void OnDragStarted()
    {
        if (draggedObject == null || resolvedEggPoint == null) return;

        eggCollider = draggedObject.GetComponentInChildren<Collider>();
        previousPosition = draggedObject.position;
        currentSpeed = 0f;
        wasTouchingObstacle = false;
        isCracked = false;

        currentEggParticles = resolvedEggPoint.GetComponent<ParticleSystem>();
        if (currentEggParticles == null && autoCreateParticleSystemIfMissing)
        {
            currentEggParticles = CreateDefaultEggFlow(resolvedEggPoint.gameObject);
        }

        if (currentEggParticles != null)
        {
            currentEggParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    protected override void OnDragUpdated()
    {
        if (draggedObject == null || eggCollider == null) return;

        // 1. Calcul de la vitesse de déplacement demandée avant blocage
        if (Time.deltaTime > 0f)
        {
            currentSpeed = Vector3.Distance(draggedObject.position, previousPosition) / Time.deltaTime;
        }

        // 2. Résolution des collisions sur le plan YZ (empêche l'œuf de traverser les obstacles)
        bool isTouchingOtherObject = ResolveCollisionsOnYZPlane();

        // Mémorise la position finale corrigée pour le calcul de vitesse de la frame suivante
        previousPosition = draggedObject.position;

        // 3. Impact (front montant + vitesse suffisante)
        if (isTouchingOtherObject && !wasTouchingObstacle)
        {
            if (currentSpeed >= minImpactSpeed && Time.time - lastImpactTime >= impactCooldown)
            {
                TriggerEggImpact();
                lastImpactTime = Time.time;
            }
        }

        // 4. Maintien du flux continu tant que l'œuf est cassé
        if (isCracked && currentEggParticles != null && !currentEggParticles.isPlaying)
        {
            currentEggParticles.Play();
        }

        wasTouchingObstacle = isTouchingOtherObject;
    }

    protected override void OnDragEnded()
    {
        if (currentEggParticles != null)
        {
            currentEggParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            currentEggParticles = null;
        }

        eggCollider = null;
        resolvedEggPoint = null;
        wasTouchingObstacle = false;
        isCracked = false;
    }

    /// <summary>
    /// Repousse l'œuf hors des obstacles (sur le plan YZ uniquement) et retourne vrai s'il y a eu contact.
    /// </summary>
    private bool ResolveCollisionsOnYZPlane()
    {
        bool touchedAny = false;

        // Synchronise les Transforms physiques avant ComputePenetration
        Physics.SyncTransforms();

        for (int iter = 0; iter < collisionIterations; iter++)
        {
            bool separatedInThisPass = false;
            Bounds b = eggCollider.bounds;

            Collider[] hits = Physics.OverlapBox(
                b.center,
                b.extents * 1.02f,
                draggedObject.rotation,
                impactLayers,
                QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i];
                if (col == null) continue;

                // Ignore les colliders appartenant à l'œuf lui-même
                if (col.transform == draggedObject || col.transform.IsChildOf(draggedObject))
                {
                    continue;
                }

                if (Physics.ComputePenetration(
                    eggCollider, eggCollider.transform.position, eggCollider.transform.rotation,
                    col, col.transform.position, col.transform.rotation,
                    out Vector3 pushDirection, out float pushDistance))
                {
                    touchedAny = true;

                    // Projection du vecteur de repousse sur le plan YZ (X reste verrouillé à initialPosition.x)
                    Vector3 yzPush = new Vector3(0f, pushDirection.y, pushDirection.z);

                    if (yzPush.sqrMagnitude > 0.0001f)
                    {
                        // Compense la projection pour repousser exactement jusqu'à la surface sur le plan YZ
                        float projectionFactor = Mathf.Max(0.25f, yzPush.magnitude);
                        Vector3 correction = yzPush.normalized * (pushDistance / projectionFactor);

                        Vector3 correctedPos = draggedObject.position + correction;
                        correctedPos.x = initialPosition.x;
                        draggedObject.position = correctedPos;

                        Physics.SyncTransforms();
                        separatedInThisPass = true;
                    }
                }
            }

            if (!separatedInThisPass) break;
        }

        return touchedAny;
    }

    private void TriggerEggImpact()
    {
        if (currentEggParticles == null) return;

        isCracked = true;

        // 1. Salve instantanée d'éclaboussure au choc
        currentEggParticles.Emit(burstParticleCount);

        // 2. Démarrage du flot continu
        if (!currentEggParticles.isPlaying)
        {
            currentEggParticles.Play();
        }
    }

    // =================================================================
    // HELPERS
    // =================================================================

    private bool IsEggObject(Transform obj)
    {
        if (obj == null) return false;

        bool matchesName = obj.name.Equals(eggIdentifier, System.StringComparison.OrdinalIgnoreCase);
        bool matchesTag = false;

        try
        {
            matchesTag = obj.CompareTag(eggIdentifier);
        }
        catch (UnityException)
        {
        }

        return matchesName || matchesTag;
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null) return null;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }
        return null;
    }

    private ParticleSystem CreateDefaultEggFlow(GameObject target)
    {
        ParticleSystem ps = target.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startColor = new Color(1f, 0.82f, 0.1f, 0.95f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.05f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.55f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.3f);
        main.gravityModifier = 0.75f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = continuousFlowRate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 14f;
        shape.radius = 0.015f;

        ParticleSystemRenderer renderer = target.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material yolkMat = new Material(shader);
            yolkMat.color = new Color(1f, 0.82f, 0.1f, 0.95f);
            renderer.material = yolkMat;
        }

        return ps;
    }
}