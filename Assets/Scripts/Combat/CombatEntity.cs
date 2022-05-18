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
    [SerializeField] private GameObject damageNumberPrefab;
    [Header("Per-entity data")]
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

    public void UseDefense() {
        skillsMaster.Get(defendSkill, skillActions).behavior.Invoke();
    }

    private int hp;
    private int shield;
    private void Start()
    {
        hp = stats.maxHp;
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
        GameObject parent = transform.parent.gameObject;
        Debug.Log("Entity " + parent.name + " took " + dmg + " points of damage, new hp: " + hp + "/" + stats.maxHp);
        if (hp <= 0 && entityType == EntityType.enemy) {
            combatManager.EntityExitTile(parent);
            combatManager.RemoveEntity(this);
            Destroy(parent);
        }
    }

    public void AddShield(int amt) {
        shield += amt;
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
