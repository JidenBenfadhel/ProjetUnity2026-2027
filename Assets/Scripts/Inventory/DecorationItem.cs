using UnityEngine;

// Cette ligne permet de créer facilement de nouveaux objets depuis le menu d'Unity
[CreateAssetMenu(fileName = "New Decoration", menuName = "Bakery/Decoration Item")]
public class DecorationItem : ScriptableObject
{
    public string itemName; // Le nom (ex: "Fraise", "Cerise")
    public Sprite icon;     // L'image 2D pour l'inventaire
    public GameObject prefab3D; // Le modèle 3D qui apparaîtra sur le gâteau
}