using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUIController : Singleton<MenuUIController>
{
    [SerializeField] private List<GameObject> menuRoots;
    [SerializeField] private List<MenuCategoryButton> categoryButtons;
    public MenuType activeMenu {get; private set;} = MenuType.CHARACTER;

    void Awake() {
        for (int i = 0; i < categoryButtons.Count; i++) {
            categoryButtons[i].menuType = (MenuType)i;
        }
        CreateInventory();
    }

    public void DisableMenus() {
        foreach (var menu in menuRoots)
            menu.SetActive(false);
    }

    public void EnableMenus() {
        foreach(var button in categoryButtons)
            button.HideNameFlyout();
        EnableMenu(MenuType.CHARACTER);
    }

    public void EnableMenu(MenuType type) {
        if (type != activeMenu) 
            categoryButtons[(int)activeMenu].HideNameFlyout();
        DisableMenus();
        menuRoots[(int)type].SetActive(true);
        activeMenu = type;
        categoryButtons[(int)activeMenu].ShowNameFlyout();
    }


    public enum MenuType {
        CHARACTER, INVENTORY, SKILL, INFO, OPTIONS
    }


    [Header("Inventory")]
    [SerializeField] private GameObject itemBoxPrefab;
    [SerializeField] private GameObject itemHolder;
    [SerializeField] private int MAX_INVENTORY = 20;
    [SerializeField] private GameObject itemOptions;
    [SerializeField] private Transform itemOptionsPosition;
    [SerializeField] private GameObject equipmentHolder;
    [SerializeField] private GameObject descriptionHolder;
    private List<MenuItemBox> itemBoxes = new List<MenuItemBox>();
    private EquipmentItem? selectedItem = null;

    public void CreateInventory() {
        foreach (var item in itemBoxes)
            Destroy(item.gameObject);
        itemBoxes.Clear();
        for (int i = 0; i < MAX_INVENTORY; i++) {
            itemBoxes.Add(Instantiate(itemBoxPrefab, itemHolder.transform).GetComponent<MenuItemBox>());
        }
    }
    public void DrawInventory(List<EquipmentItem> items) {

        for (int i = 0; i < MAX_INVENTORY; i++) {
            if (i < items.Count)
                itemBoxes[i].InsertItem(items[i]);
            else 
                itemBoxes[i].RemoveItem();
        }
    }

    public void OpenItemOptions(EquipmentItem item, Vector3 pos) {
        itemOptions.SetActive(true);
        itemOptionsPosition.SetPositionAndRotation(pos + 2 * Vector3.left, Quaternion.identity);
        selectedItem = item;
    }

    public void CloseItemOptions() {
        itemOptions.SetActive(false);
        selectedItem = null;
    }

    public void EnableEquipment() {
        DisableDescription();
        itemOptions.SetActive(false);
        equipmentHolder.SetActive(true);

        if (selectedItem != null) {
            HighlightEquip(selectedItem.Value.Type);
        }
        
    }

    public void DisableDescription() {
        descriptionHolder.SetActive(false);
    }

    public void DisableEquipment() {
        equipmentHolder.SetActive(false);
    }

    public void HighlightEquip(ItemType type) {

    }

    public void EnableDescription() {
        DisableEquipment();
        descriptionHolder.SetActive(true);
        
        CloseItemOptions();
    }


}
