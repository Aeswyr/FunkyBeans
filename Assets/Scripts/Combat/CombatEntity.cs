using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatEntity : MonoBehaviour
{
    [System.Serializable] public enum EntityType
    {
        player, 
        enemy
    }

    [SerializeField] private EntityType entityType;
    public EntityType EntitiyType => entityType;

    [SerializeField] private float speed;
    public float Speed => speed;

    [SerializeField] private int numMaxActions;
    public int NumMaxActions => numMaxActions;

    [SerializeField] private Sprite uiSprite;
    public Sprite UISprite => uiSprite;
}
