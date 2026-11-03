using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<DecorationItem> allDecorations;
    public GameObject inventorySlotPrefab;
    public Transform inventoryPanel;

    void Start()
    {
        // Affiche "Tous" au lancement
        GenerateInventory(0);
    }

    // Cette fonction sera maintenant appelée par tes boutons
    public void GenerateInventory(int categoryIndex)
    {
        foreach (Transform child in inventoryPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (DecorationItem item in allDecorations)
        {
            if (categoryIndex == 0 || (int)item.category == (categoryIndex - 1))
            {
                GameObject newSlot = Instantiate(inventorySlotPrefab, inventoryPanel);
                InventorySlotUI slotUI = newSlot.GetComponent<InventorySlotUI>();
                slotUI.Setup(item);
            }
        }
    }
}