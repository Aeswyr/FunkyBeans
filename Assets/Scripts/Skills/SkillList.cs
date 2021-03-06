using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SkillList", menuName = "FunkyBeans/SkillList", order = 0)]
public class SkillList : ScriptableObject {
    [SerializeField] private List<Skill> skills;

    /**
    *   returns a usable copy of the skill
    */
    public Skill Get(SkillID id, SkillActions entityActions) {
        var skill = skills[(int)id];
        skill.behavior = new UnityEvent();
        LinkSkill(id, skill, entityActions);
        return skill;
    }

    /**
    *   returns an unusable copy of the skill, but with accurate skill data
    */
    public Skill Get(SkillID id) {
        return skills[(int)id];
    }

    private void LinkSkill(SkillID id, Skill skill, SkillActions actions) {
        switch (id) {
            case SkillID.STRIKE:
                skill.behavior.AddListener(actions.Strike);
                break;
            case SkillID.HEW:
                skill.behavior.AddListener(actions.Hew);
                break;
            case SkillID.BLOCK:
                skill.behavior.AddListener(actions.Block);
                break;
            case SkillID.QUICKSHOT:
                skill.behavior.AddListener(actions.Quickshot);
                break;
            case SkillID.FIREBALL:
                skill.behavior.AddListener(actions.Fireball);
                break;
        }
    }
}

public enum SkillID {
    NULL,
    STRIKE,
    HEW,
    BLOCK,
    QUICKSHOT,
    FIREBALL,

}
