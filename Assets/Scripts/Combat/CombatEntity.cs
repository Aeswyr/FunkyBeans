using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatEntity : MonoBehaviour
{
    [SerializeField] private float speed;
    public float Speed => speed;

    [SerializeField] private Sprite uiSprite;
    public Sprite UISprite => uiSprite;
}
