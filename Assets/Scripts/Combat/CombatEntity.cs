using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class CombatEntity : NetworkBehaviour
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
    [SerializeField] private StatBlock stats;
    public StatBlock Stats => stats;
    [SerializeField] private CombatReward reward;
    public CombatReward Reward => reward;
    [SerializeField] private List<SkillID> knownSkills;
    public List<SkillID> KnownSkills => knownSkills;
    [SerializeField] private SkillID defendSkill;
    public SkillID DefendSkill => defendSkill;
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

    [SyncVar] private int hp;
    public int HP => hp;
    [SyncVar] private int mp;
    public int MP => mp;
    [SyncVar] private int armor;
    public int Armor => armor;
    [SyncVar] private int evasion;
    public int Evasion => evasion;
    void Start()
    {
        stats.Init();
        hp = stats.raw.maxHp;
        mp = stats.raw.maxMp;
    }

    [Client] public void UpdateResource(ResourceType type, int amt)
    {
        TextMeshPro tm;
        switch (type)
        {
            case ResourceType.HEALTH:
                tm = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
                tm.text = amt.ToString();
                tm.color = Color.red;
                break;
            case ResourceType.MANA:
                tm = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
                tm.text = amt.ToString();
                tm.color = Color.blue;
                break;
            case ResourceType.ARMOR:
                tm = Instantiate(damageNumberPrefab, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
                tm.text = amt.ToString();
                tm.color = Color.grey;
                break;
            default:
                break;
        }

        if (entityType == EntityType.player)
            CombatUIController.Instance.UpdatePlayerResource(this);
        else
            CombatUIController.Instance.UpdateDisplayedEntity();

    }

    [Server] public void TakeDamage(int dmg)
    {


        if (armor > 0) {
            armor -= dmg;
            if (armor < 0) {
                hp += armor;
                armor = 0;
            }
            serverCombatManager.NotifyResourceChange(transform.GetComponent<CombatID>().CID, ResourceType.ARMOR, dmg);
        } else {
            hp -= dmg;
            serverCombatManager.NotifyResourceChange(transform.GetComponent<CombatID>().CID, ResourceType.HEALTH, dmg);
        }

        GameObject parent = transform.gameObject;
        Debug.Log("Entity " + parent.name + " took " + dmg + " points of damage, new hp: " + hp + "/" + stats.maxHp);
        if (hp <= 0 && entityType == EntityType.enemy) {
            CombatUIController.Instance?.DisableIfDisplayed(this);
            serverCombatManager.EntityExitTile(parent);
            serverCombatManager.RemoveEntity(this);
            Destroy(parent);
        }
    }

    [Server] public bool TrySpendMP(int val) {
        if (val == 0)
            return true;
        if (mp < val)
            return false;
        mp -= val;

        serverCombatManager.NotifyResourceChange(transform.GetComponent<CombatID>().CID, ResourceType.MANA, val);
            
        return true;
    }

    [Server] public void AddArmor(int amt) {
        armor += amt;
        serverCombatManager.NotifyResourceChange(transform.GetComponent<CombatID>().CID, ResourceType.ARMOR, -amt);
    }

    [Server] public void AddEvasion(int amt) {
        evasion += amt;
        serverCombatManager.NotifyResourceChange(transform.GetComponent<CombatID>().CID, ResourceType.EVASION, -amt);
    }

    public int GetMagnitudeOfSkill(Skill skill)
    {
        return stats.damage;
    }
    public void SetServerCombatManager(ServerCombatManager newServerCombatManager)
    {
        serverCombatManager = newServerCombatManager;
    }

    private ServerCombatManager serverCombatManager;
    public ServerCombatManager GetServerCombatManager()
    {
        return serverCombatManager;
    }
}

[Serializable]
public struct StatBlock {
    public Stats raw;
    public int maxHp {
        get {
            int val = raw.maxHp;
            foreach (var stat in modifiers.Values)
                val += stat.maxHp;
            return val;
        }
    }
    public int maxMp {
        get {
            int val = raw.maxMp;
            foreach (var stat in modifiers.Values)
                val += stat.maxMp;
            return val;
        }
    }
    public int defense {
        get {
            int val = raw.defense;
            foreach (var stat in modifiers.Values)
                val += stat.defense;
            return val;
        }
    }
    public int threat {
        get {
            int val = raw.threat;
            foreach (var stat in modifiers.Values)
                val += stat.threat;
            return val;
        }
    }
        public int damage {
        get {
            int val = raw.damage;
            foreach (var stat in modifiers.Values)
                val += stat.damage;
            return val;
        }
    }
    public int speed {
        get {
            int val = raw.speed;
            foreach (var stat in modifiers.Values)
                val += stat.speed;
            return val;
        }
    }
    public int actions {
        get {
            int val = raw.actions;
            foreach (var stat in modifiers.Values)
                val += stat.actions;
            return val;
        }
    }
    public int dodge {
        get {
            int val = raw.dodge;
            foreach (var stat in modifiers.Values)
                val += stat.dodge;
            return val;
        }
    }

    public void AddModifier(long id, Stats mod) {
        modifiers.Add(id, mod);
    }
    public void UpdateModifier(long id, Stats mod) {
        modifiers[id] = mod;
    }
    public void RemoveModifier(long id) {
        modifiers.Remove(id);
    }
    public Stats GetModifier(long id) {
        return modifiers[id];
    }
    public void Init() {
        modifiers = new Dictionary<long, Stats>();
    }
    private Dictionary<long, Stats> modifiers;
}

[Serializable]
public struct Stats {
    public int maxHp;
    public int maxMp;
    public int threat;
    public int damage;
    public int speed;
    public int actions;
    public int defense;
    public int dodge;
}

[Serializable]
public struct CombatReward
{
    public int exp;
}

public enum ResourceType {
    DEFAULT, HEALTH, MANA, ACTIONS, ARMOR, EVASION
}
 