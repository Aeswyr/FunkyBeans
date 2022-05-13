using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionUIController : Singleton<ActionUIController>
{
    [SerializeField] private Transform actionUIParent;
    private List<Image> images;

    [Header("Data")]
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;

    // Start is called before the first frame update
    void Start()
    {
        images = new List<Image>();

        //fill image array
        for(int i = 0; i < actionUIParent.childCount; i++)
        {
            images.Add(actionUIParent.GetChild(i).GetComponent<Image>());
        }
    }

    public void SetActionUI(int numActionsLeft, int maxActions)
    {
        for(int i = 0; i<images.Count; i++)
        {
            Image currActionSprite = images[i];

            if (i >= maxActions)
            {
                currActionSprite.sprite = null;
                continue;
            }

            if (i < numActionsLeft)
                currActionSprite.sprite = fullSprite;
            else
                currActionSprite.sprite = emptySprite;
        }
    }
}
