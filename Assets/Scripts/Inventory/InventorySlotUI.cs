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
        Debug.Log("Création de la décoration : " + currentItem.itemName);

        // On vérifie d'abord que tu as bien glissé un modèle 3D dans ton Scriptable Object
        if (currentItem.prefab3D != null)
        {
            // Instantiate(Objet_A_Créer, Position, Rotation)
            // On le fait apparaître un peu en hauteur (Y = 1) pour qu'il ne soit pas coincé dans le sol
            Vector3 positionApparition = new Vector3(0, 1, 0);

            Instantiate(currentItem.prefab3D, positionApparition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Attention : Le Prefab 3D de " + currentItem.itemName + " est vide !");
        }
    }
}