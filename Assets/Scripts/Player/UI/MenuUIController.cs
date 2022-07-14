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
    private List<MenuItemBox> itemBoxes = new List<MenuItemBox>();

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
}
