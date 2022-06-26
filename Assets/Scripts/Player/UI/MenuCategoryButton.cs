using UnityEngine;

public class MenuCategoryButton : UnityEngine.UI.Button {
    [SerializeField] private GameObject infoPopup;
    public MenuUIController.MenuType menuType {get; set;}
    public void OnButtonPressed() {
        MenuUIController.Instance.EnableMenu(menuType);
    }
    public override void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData) {
        base.OnPointerExit(eventData);
        if (menuType != MenuUIController.Instance.activeMenu)
            HideNameFlyout();
    }

    public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        if (menuType != MenuUIController.Instance.activeMenu)
            ShowNameFlyout();
    }

    public void ShowNameFlyout() {
        infoPopup.SetActive(true);
    }

    public void HideNameFlyout() {
        infoPopup.SetActive(false);
    }


}