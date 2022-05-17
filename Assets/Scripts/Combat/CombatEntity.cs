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

    private CombatManager combatManager;
    public void SetCombatManager(CombatManager manager)
    {
        combatManager = manager;
    }

    public void OnTurnStart()
    {
        switch(entityType)
        {
            case EntityType.player:
                {
                    //combatManager.SetNumActions(numMaxActions);
                    break;
                }
            case EntityType.enemy:
                {
                    break;
                }
        }
    }

    public void OnTurnEnd()
    {
        switch (entityType)
        {
            case EntityType.player:
                {

                    break;
                }
            case EntityType.enemy:
                {
                    break;
                }
        }
    }
}
