using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CombatEntity : MonoBehaviour
{
    [System.Serializable] public enum EntityType
    {
        player, 
        enemy
    }

    [Header("Universal data")]
    [SerializeField] private SkillList skillsMaster;
    [SerializeField] private SkillActions skillActions;
    [Header("Per-entity data")]
    [SerializeField] private Stats stats;
    public Stats Stats => stats;
    [SerializeField] private Sprite uiSprite;
    public Sprite UISprite => uiSprite;
    [SerializeField] private EntityType entityType;
    public EntityType EntitiyType => entityType;
    public void UseSkill(SkillID id) {
        skillsMaster.Get(id, skillActions).behavior.Invoke();
    }
}

[Serializable]
public struct Stats {
    public int speed;
    public int actions;
}
