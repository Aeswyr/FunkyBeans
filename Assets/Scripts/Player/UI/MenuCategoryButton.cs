using UnityEngine;

public class MenuCategoryButton : UnityEngine.UI.Button {
    [SerializeField] private GameObject infoPopup;

    public override void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData) {
        base.OnPointerExit(eventData);
        infoPopup.SetActive(false);
    }

    public override void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        infoPopup.SetActive(true);
    }


}