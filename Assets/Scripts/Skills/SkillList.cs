using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SkillList", menuName = "FunkyBeans/SkillList", order = 0)]
public class SkillList : ScriptableObject {
    [SerializeField] private List<Skill> skills;

    public Skill Get(SkillID id, SkillActions entityActions) {
        var skill = skills[(int)id];
        skill.behavior = new UnityEvent();
        LinkSkill(id, skill, entityActions);
        return skill;
    }

    private void LinkSkill(SkillID id, Skill skill, SkillActions actions) {
        switch (id) {
            case SkillID.STRIKE:
                skill.behavior.AddListener(actions.Strike);
                break;
        }
        
    }
}

public enum SkillID {
    STRIKE, 
}
