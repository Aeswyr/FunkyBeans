using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUIController : Singleton<CombatUIController>
{
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
                    manager.SetMoveMode();
                    options.SetActive(true);
                    menuState = MenuState.SELECT;
                    break;
                case MenuState.SELECT:
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
    }

    public void FleePressed() {
        if (!manager.IsPlayerTurn())
            return;
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
}
