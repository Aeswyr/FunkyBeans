using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CombatUIController : Singleton<CombatUIController>
{
    [Header("Action Menu")]
    [SerializeField] private SkillList skillMasterList;
    [SerializeField] private GameObject holder;
    [SerializeField] private GameObject options;
    [SerializeField] private GameObject buttonPrefab;
    private List<GameObject> buttons = new List<GameObject>();
    private CombatManager manager;
    private List<SkillID> skills = new List<SkillID>();

    public MenuState menuState = MenuState.START;

    void FixedUpdate() {
        if (InputHandler.Instance.back.pressed) {
            switch (menuState) {
                case MenuState.TARGET:
                    manager.SetIdleMode();
                    options.SetActive(true);
                    menuState = MenuState.SELECT;
                    break;
                case MenuState.SELECT:
                    manager.SetMoveMode();
                    options.SetActive(false);
                    menuState = MenuState.START;
                    break;
            }
        }
    }

    public void AttackPressed() {
        if (!manager.IsPlayerTurn())
            return;
        menuState = MenuState.SELECT;
        foreach (var button in buttons) {
            Destroy(button);
        }
        buttons.Clear();
        options.SetActive(true);
        manager.SetIdleMode();
        for (int i = 0; i < skills.Count; i++) {
            if (skillMasterList.Get(skills[i]).category == Skill.Category.ATTACK) {
                GameObject button = Instantiate(buttonPrefab, holder.transform);
                button.GetComponent<SkillButton>().Init(skills[i], skillMasterList, manager);
                buttons.Add(button);
            }
        }
    }

    public void SkillPressed() {
        if (!manager.IsPlayerTurn())
            return;
        menuState = MenuState.SELECT;
        foreach (var button in buttons) {
            Destroy(button);
        }
        buttons.Clear();
        options.SetActive(true);
        manager.SetIdleMode();
        for (int i = 0; i < skills.Count; i++) {
            if (skillMasterList.Get(skills[i]).category == Skill.Category.ABILITY) {
                GameObject button = Instantiate(buttonPrefab, holder.transform);
                button.GetComponent<SkillButton>().Init(skills[i], skillMasterList, manager);
                buttons.Add(button);
            }
        }
    }

    public void DefendPressed() {
        if (!manager.IsPlayerTurn())
            return;
        manager.SetDefendMode();
    }

    public void FleePressed() {
        if (!manager.IsPlayerTurn())
            return;
        manager.EndCombat();
    }

    public void SetCombatManager(CombatManager manager) {
        this.manager = manager;
    }

    public void SetKnownSkills(List<SkillID> skills) {
        this.skills = skills;
    }

    public void Reset() {
        options.SetActive(false);
        menuState = MenuState.START;
    }

    public enum MenuState {
        START, SELECT, TARGET
    }



    // Turn order management
    [Header("Turn Order")]
    [SerializeField] private GameObject turnOrderCanvas;
    public GameObject TurnOrderCanvas => turnOrderCanvas;
    [SerializeField] private Image currEntityImage;
    [SerializeField] private Transform leftBar;
    [SerializeField] private Transform rightBar;

    public void PlaceTurnEntity(Transform obj, float percentOnBar)
    {
        Vector3 offSet = ((rightBar.position - leftBar.position)+new Vector3(0, 0,-50)) * percentOnBar + new Vector3(0, -0.07f, 0);

        obj.transform.position = leftBar.position + offSet;
    }

    public void SetCurrEntitySprite(Sprite spr)
    {
        currEntityImage.sprite = spr;
    }


    // Action Counter
    [Header("Action Display")]
    [SerializeField] private TextMeshProUGUI textRemaining;
    [SerializeField] private TextMeshProUGUI textMax;


    public void SetActionUI(int numActionsLeft, int maxActions)
    {
        textRemaining.text = numActionsLeft.ToString();
        textMax.text = maxActions.ToString();
    }


    // Enemy Info Display
    [Header("Enemy Display")]
    [SerializeField] private GameObject enemyInfo;
    [SerializeField] private ResourceController resource;
    [SerializeField] private TextMeshProUGUI description;
    private CombatEntity displayedEntity;

    public void SetDisplayedEntity(CombatEntity entity) {
        enemyInfo.SetActive(true);
        displayedEntity = entity;
        resource.SetHP(entity.HP, entity.Stats.maxHp);
        resource.SetMP(entity.MP, entity.Stats.maxMp);
        resource.SetNametag(entity.EntityName);
        description.text = entity.Description;
    }

    public void UpdateDisplayedEntity() {
        if (displayedEntity == null)
            return;

        resource.SetHP(displayedEntity.HP, displayedEntity.Stats.maxHp);
        resource.SetMP(displayedEntity.MP, displayedEntity.Stats.maxMp);
        resource.SetNametag(displayedEntity.EntityName);
        description.text = displayedEntity.Description;
    }

    public void DisableIfDisplayed(CombatEntity entity) {
        enemyInfo.SetActive(false);
        if (entity == displayedEntity) {
            displayedEntity = null;
        }
    }

    public void DisableDisplay() {
        enemyInfo.SetActive(false);
    }

    [Header("Player Display")]
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private GameObject resourceHolder;
    private Dictionary<CombatEntity, ResourceController> activeBars = new Dictionary<CombatEntity, ResourceController>();

    public void RegisterNewResource(CombatEntity entity) {
        activeBars[entity] = Instantiate(resourcePrefab, resourceHolder.transform).GetComponent<ResourceController>();
        UpdatePlayerResource(entity);
    }

    public void UpdatePlayerResource(CombatEntity entity) {
        ResourceController resource = activeBars[entity];

        resource.SetNametag(entity.EntityName);
        resource.SetHP(entity.HP, entity.Stats.maxHp);
        resource.SetMP(entity.MP, entity.Stats.maxMp);
    }

    public void ClearPlayerResources() {
        foreach (var res in activeBars) {
            Destroy(res.Value.gameObject);
        }
        activeBars.Clear();
    }

}
