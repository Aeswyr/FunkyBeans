using TMPro;
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
    public SkillActions SkillActions => skillActions;
    [SerializeField] private GameObject damageNumberPrefab;
    [Header("Per-entity data")]
    [SerializeField] private string entityName;
    public string EntityName => entityName;
    [SerializeField] private string description;
    public string Description => description;
    [SerializeField] private Stats stats;
    public Stats Stats => stats;
    [SerializeField] private CombatReward reward;
    public CombatReward Reward => reward;
    [SerializeField] private List<SkillID> knownSkills;
    public List<SkillID> KnownSkills => knownSkills;
    [SerializeField] private SkillID defendSkill;
    [SerializeField] private Sprite uiSprite;
    public Sprite UISprite => uiSprite;
    [SerializeField] private EntityType entityType;
    public EntityType team => entityType;

    public void UseSkill(SkillID id) {
        skillsMaster.Get(id, skillActions).behavior.Invoke();
    }

    internal void UseSkillAI(SkillID id, List<Vector3Int> skillTargPositions)
    {
        Debug.Log("Skill "+id+" going to hit "+skillTargPositions.Count+" locations");
        skillActions.targetPositions = skillTargPositions;
        skillsMaster.Get(id, skillActions).behavior.Invoke();
    }

    public void UseDefense() {
        skillsMaster.Get(defendSkill, skillActions).behavior.Invoke();
    }

    private int hp;
    public int HP => hp;
    private int mp;
    public int MP => mp;
    private int shield;
    public int Shield => shield;
    private void Start()
    {
        hp = stats.maxHp;
        mp = stats.maxMp;
    }

    public void TakeDamage(int dmg)
    {
        var tm = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
        tm.text = dmg.ToString();

        if (shield > 0) {
            shield -= dmg;
            if (shield < 0) {
                hp += shield;
                shield = 0;
            }
            tm.color = Color.gray;
        } else {
            hp -= dmg;
            tm.color = Color.red;
        }

        CombatUIController.Instance.UpdateDisplayedEntity();
        if (team == EntityType.player)
            CombatUIController.Instance.UpdatePlayerResource(this);

        GameObject parent = transform.parent.gameObject;
        Debug.Log("Entity " + parent.name + " took " + dmg + " points of damage, new hp: " + hp + "/" + stats.maxHp);
        if (hp <= 0 && entityType == EntityType.enemy) {
            CombatUIController.Instance.DisableIfDisplayed(this);
            combatManager.EntityExitTile(parent);
            combatManager.RemoveEntity(this);
            Destroy(parent);
        }
    }

    public bool TrySpendMP(int val) {
        if (mp < val)
            return false;
        mp -= val;

        CombatUIController.Instance.UpdateDisplayedEntity();
        if (team == EntityType.player)
            CombatUIController.Instance.UpdatePlayerResource(this);
            
        return true;
    }

    public void AddShield(int amt) {
        shield += amt;
    }

    public int GetMagnitudeOfSkill(Skill skill)
    {
        return stats.damage;
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
    public int maxHp;
    public int maxMp;
    public int damage;
    public int speed;
    public int actions;
    public int defense;
}

[Serializable]
public struct CombatReward
{
    public int exp;
}
