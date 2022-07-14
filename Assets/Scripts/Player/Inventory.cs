using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Inventory : NetworkBehaviour
{
    
    private List<EquipmentItem> items = new List<EquipmentItem>();

    public void Insert(EquipmentItem item) {
        items.Add(item);
        RedrawInventory(); 
    }

    public void Remove(EquipmentItem item) {
        for (int i = 0; i < items.Count; i++) {
            if (items[i].ID == item.ID) {
                items.RemoveAt(i);
                i--;
            }
        }
        RedrawInventory();
    }


    [Client] public void RedrawInventory() {
        MenuUIController.Instance?.DrawInventory(items);
    }
    
}
