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
}
