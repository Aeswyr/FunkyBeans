using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityTurnIndicator : MonoBehaviour
{
    [SerializeField] private Image image;

    public void SetSprite(Sprite newSprite)
    {
        image.sprite = newSprite;
    }
}
