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
    [SerializeField] private List<SkillID> knownSkills;
    public List<SkillID> KnownSkills => knownSkills;
    [SerializeField] private Sprite uiSprite;
    public Sprite UISprite => uiSprite;
    [SerializeField] private EntityType entityType;
    public EntityType EntitiyType => entityType;

    public void UseSkill(SkillID id) {
        skillsMaster.Get(id, skillActions).behavior.Invoke();
    }

    private CombatManager combatManager;
    public void SetCombatManager(CombatManager manager)
    {
        combatManager = manager;
    }
    public CombatManager CombatManager => combatManager;
}

[Serializable]
public struct Stats {
    public int speed;
    public int actions;
}
