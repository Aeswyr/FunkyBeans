using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CombatEntity : MonoBehaviour
{
    private bool localIsMine;
    public bool LocalIsMine => localIsMine;

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

    public void UseSkillAI(SkillID id, List<Vector3Int> skillTargPositions)
    {
        //Debug.Log("Skill "+id+" going to hit "+skillTargPositions.Count+" locations");
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
    private int armor;
    public int Armor => armor;
    void Start()
    {
        hp = stats.maxHp;
        mp = stats.maxMp;
    }

    public void TakeDamage(int dmg)
    {
        var tm = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
        tm.text = dmg.ToString();

        if (armor > 0) {
            armor -= dmg;
            if (armor < 0) {
                hp += armor;
                armor = 0;
            }
            tm.color = Color.gray;
        } else {
            hp -= dmg;
            tm.color = Color.red;
        }
        CombatUIController.Instance?.UpdateDisplayedEntity();
        if (team == EntityType.player)
            CombatUIController.Instance?.UpdatePlayerResource(this);

        GameObject parent = transform.parent.gameObject;
        Debug.Log("Entity " + parent.name + " took " + dmg + " points of damage, new hp: " + hp + "/" + stats.maxHp);
        if (hp <= 0 && entityType == EntityType.enemy) {
            CombatUIController.Instance?.DisableIfDisplayed(this);
            combatManager.EntityExitTile(parent);
            combatManager.RemoveEntity(this);
            Destroy(parent);
        }
    }

    public bool TrySpendMP(int val) {
        if (mp < val)
            return false;
        mp -= val;

        CombatUIController.Instance?.UpdateDisplayedEntity();
        if (team == EntityType.player)
            CombatUIController.Instance?.UpdatePlayerResource(this);
            
        return true;
    }

    public void AddArmor(int amt) {
        armor += amt;
        CombatUIController.Instance?.UpdateDisplayedEntity();
        if (team == EntityType.player)
            CombatUIController.Instance?.UpdatePlayerResource(this);
    }

    public int GetMagnitudeOfSkill(Skill skill)
    {
        return stats.damage;
    }

    private ServerCombatManager serverCombatManager;
    public void SetServerCombatManager(ServerCombatManager newServerCombatManager)
    {
        serverCombatManager = newServerCombatManager;
    }

    public ServerCombatManager ServerCombatManager => serverCombatManager;
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

public enum ResourceType {
    DEFAULT, HEALTH, MANA, ACTIONS, ARMOR,
}
 