using UnityEngine;

public class DragSugar : DragOnYZPlane
{
    [Header("Identification de l'objet Flour")]
    [Tooltip("Nom ou Tag attendu pour l'objet farine (insensible à la casse).")]
    public string flourIdentifier = "sugar";

    [Header("Particules de Farine")]
    [Tooltip("Angle (en degrés) à partir duquel les particules s'échappent.")]
    public float triggerAngle = 65f;
    [Tooltip("Nom exact du Transform enfant d'où s'échappent les particules.")]
    public string spawnPointChildName = "pouringpointsugar";
    [Tooltip("Crée automatiquement un ParticleSystem blanc si 'pouringpointflour' n'en possède pas encore.")]
    public bool autoCreateParticleSystemIfMissing = true;

    private ParticleSystem currentFlourParticles;
    private Transform resolvedFlourPoint;

    /// <summary>
    /// Filtre : n'autorise le drag que sur l'objet "flour" (ou un de ses enfants).
    /// </summary>
    protected override bool CanDragObject(Transform hitTransform, out Transform targetObject)
    {
        targetObject = null;
        resolvedFlourPoint = null;

        // Vérifie l'objet touché ou son parent direct (si le collider est sur un sous-mesh)
        Transform candidate = hitTransform;
        if (!IsFlourObject(candidate) && candidate.parent != null && IsFlourObject(candidate.parent))
        {
            candidate = candidate.parent;
        }

        if (!IsFlourObject(candidate)) return false;

        resolvedFlourPoint = FindChildRecursive(candidate, spawnPointChildName);
        if (resolvedFlourPoint == null)
        {
            Debug.LogWarning($"[DragFlour] L'objet '{candidate.name}' a été détecté mais ne possède pas d'enfant '{spawnPointChildName}'.");
            return false;
        }

        targetObject = candidate;
        return true;
    }

    protected override void OnDragStarted()
    {
        if (resolvedFlourPoint == null) return;

        currentFlourParticles = resolvedFlourPoint.GetComponent<ParticleSystem>();
        if (currentFlourParticles == null && autoCreateParticleSystemIfMissing)
        {
            currentFlourParticles = CreateDefaultFlourFlow(resolvedFlourPoint.gameObject);
        }

        if (currentFlourParticles != null)
        {
            currentFlourParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    protected override void OnDragUpdated()
    {
        if (currentFlourParticles == null) return;

        bool shouldEmit = Mathf.Abs(currentRotationAngle) > triggerAngle;

        if (shouldEmit && !currentFlourParticles.isPlaying)
        {
            currentFlourParticles.Play();
        }
        else if (!shouldEmit && currentFlourParticles.isPlaying)
        {
            currentFlourParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    protected override void OnDragEnded()
    {
        if (currentFlourParticles != null)
        {
            currentFlourParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            currentFlourParticles = null;
        }
        resolvedFlourPoint = null;
    }

    // =================================================================
    // HELPERS
    // =================================================================

    private bool IsFlourObject(Transform obj)
    {
        if (obj == null) return false;

        bool matchesName = obj.name.Equals(flourIdentifier, System.StringComparison.OrdinalIgnoreCase);
        bool matchesTag = false;

        try
        {
            matchesTag = obj.CompareTag(flourIdentifier);
        }
        catch (UnityException)
        {
            // Ignore si le tag "flour" n'est pas défini dans Tags & Layers
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

    private ParticleSystem CreateDefaultFlourFlow(GameObject target)
    {
        ParticleSystem ps = target.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startColor = new Color(0.98f, 0.97f, 0.94f, 0.9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.5f);
        main.gravityModifier = 0.45f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 90f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 16f;
        shape.radius = 0.015f;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.15f;
        noise.frequency = 1.5f;

        ParticleSystemRenderer renderer = target.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material flourMat = new Material(shader);
            flourMat.color = new Color(0.98f, 0.97f, 0.94f, 0.9f);
            renderer.material = flourMat;
        }

        return ps;
    }
}