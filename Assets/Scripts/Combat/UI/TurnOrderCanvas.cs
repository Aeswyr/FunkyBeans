using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderCanvas : Singleton<TurnOrderCanvas>
{
    [SerializeField] private Image currEntityImage;
    [SerializeField] private Transform leftBar;
    [SerializeField] private Transform rightBar;

    public void PlaceTurnEntity(Transform obj, float percentOnBar)
    {
        Vector3 offSet = ((rightBar.position - leftBar.position)+new Vector3(0, 0,-50)) * percentOnBar + new Vector3(0, -0.07f, 0);

        obj.transform.position = leftBar.position + offSet;
    }

    public void SetCurrEntitySprite(Sprite spr)
    {
        currEntityImage.sprite = spr;
    }
}
