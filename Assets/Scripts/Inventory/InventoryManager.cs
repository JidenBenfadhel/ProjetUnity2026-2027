using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Données")]
    public List<DecorationItem> allDecorations; // La liste de tous tes ScriptableObjects (Fraise, Cerise...)

    [Header("Interface")]
    public GameObject slotPrefab; // Ton prefab "InventorySlot"
    public Transform inventoryPanel; // Ton panel UI avec le Grid Layout Group

    void Start()
    {
        GenerateInventory();
    }

    void GenerateInventory()
    {
        // Pour chaque objet dans notre liste de données
        foreach (DecorationItem item in allDecorations)
        {
            // On crée un nouveau bouton dans le panel
            GameObject newSlot = Instantiate(slotPrefab, inventoryPanel);

            // On récupère le script du bouton pour lui donner ses informations
            InventorySlotUI slotScript = newSlot.GetComponent<InventorySlotUI>();
            slotScript.Setup(item);
        }
    }
}