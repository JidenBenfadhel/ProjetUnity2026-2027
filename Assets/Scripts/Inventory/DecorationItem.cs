using UnityEngine;

// La liste de tes catégories (tu peux en ajouter autant que tu veux ici)
public enum ItemCategory
{
    Fruits,
    Fleurs,
    Bougies,
    Papillons
}

[CreateAssetMenu(fileName = "New Decoration", menuName = "Bakery/Decoration Item")]
public class DecorationItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab3D;

    // La nouvelle case qui apparaîtra dans Unity
    public ItemCategory category;
}