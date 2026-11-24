using UnityEngine;

// La liste de nos outils disponibles
public enum ToolType { Glacage, Decoration }

public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance;

    public ToolType currentTool = ToolType.Glacage; // Glaçage par défaut au lancement
    public GameObject currentDecorationPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Appelée par les boutons de fruits de ton coéquipier
    public void SelectDecoration(GameObject prefab)
    {
        currentDecorationPrefab = prefab;
        currentTool = ToolType.Decoration; // On passe automatiquement en mode Déco
        Debug.Log("Outil actuel : Décoration (" + prefab.name + ")");
    }

    // Tu pourras appeler cette fonction plus tard via un bouton "Spatule" dans l'UI
    public void SelectGlacage()
    {
        currentTool = ToolType.Glacage;
        currentDecorationPrefab = null;
        Debug.Log("Outil actuel : Glaçage");
    }
}