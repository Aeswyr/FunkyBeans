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

    void FixedUpdate() {
        
    }

    public void AttackPressed() {
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

    public void SetCombatManager(CombatManager manager) {
        this.manager = manager;
    }

    public void SetKnownSkills(List<SkillID> skills) {
        this.skills = skills;
    }

}
