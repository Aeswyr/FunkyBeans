using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuItemBox : MonoBehaviour
{
    [SerializeField] private Image image;
    private EquipmentItem? item = null;

    private static Color EMPTY = new Color(0, 0, 0, 0);

    public void InsertItem(EquipmentItem? item) {
        if (item == null) {
            RemoveItem();
            return;
        }

        image.color = Color.white;
        image.sprite = GameHandler.Instance.ItemHelper.GetSprite(item.Value.SpriteID);
        this.item = item;
    }

    public void RemoveItem() {
        image.color = EMPTY;
        item = null;
    }

    public void TrySelectItem() {
        if (item == null)
            return;
        MenuUIController.Instance.OpenItemOptions(item.Value, transform.position);
    }
}
