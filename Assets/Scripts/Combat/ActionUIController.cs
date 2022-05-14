using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionUIController : Singleton<ActionUIController>
{


    [Header("Text fields")]
    [SerializeField] private TextMeshProUGUI textRemaining;
    [SerializeField] private TextMeshProUGUI textMax;


    public void SetActionUI(int numActionsLeft, int maxActions)
    {
        textRemaining.text = numActionsLeft.ToString();
        textMax.text = maxActions.ToString();
    }
}
