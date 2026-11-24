using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconDisplay; // Glisse ton ItemIcon ici dans l'inspecteur
    private DecorationItem currentItem; // L'objet stocké dans cette case

    // Fonction appelée par le Manager pour configurer la case
    public void Setup(DecorationItem itemData)
    {
        // On demande à Unity de vérifier d'où vient le problème
        if (itemData == null)
        {
            Debug.LogError("Erreur : Un des éléments dans la liste 'All Decorations' du InventoryManager est vide (None) !");
            return;
        }

        if (iconDisplay == null)
        {
            Debug.LogError("Erreur : La case 'Icon Display' du bouton " + gameObject.name + " est vide dans le Prefab !");
            return;
        }

        // Si tout va bien, le code normal s'exécute
        currentItem = itemData;
        iconDisplay.sprite = itemData.icon;
        iconDisplay.color = Color.white;
    }

    // Fonction appelée quand on clique sur le bouton
    public void OnSlotClicked()
    {
        // On vérifie d'abord que le modèle 3D existe bien dans le Scriptable Object[cite: 2]
        if (currentItem.prefab3D != null)
        {
            // Au lieu de l'instancier, on le donne au ToolManager !
            if (ToolManager.Instance != null)
            {
                ToolManager.Instance.SelectDecoration(currentItem.prefab3D);
            }
        }
        else
        {
            Debug.LogWarning("Attention : Le Prefab 3D de " + currentItem.itemName + " est vide !");
        }
    }
}