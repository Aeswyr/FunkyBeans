using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActions : MonoBehaviour
{
    [SerializeField] private SkillList skillList;

    [SerializeField] private CombatEntity entity;
    public void Strike() 
    {
        Skill skill = skillList.Get(SkillID.STRIKE);
        Utils.CombatUtil.UseSimpleDamageSkill(entity, skill);
    }
    public void Hew()
    {
        Skill skill = skillList.Get(SkillID.HEW);
        Utils.CombatUtil.UseSimpleDamageSkill(entity, skill);
    }

    public void Block() {
        Skill skill = skillList.Get(SkillID.BLOCK);
        entity.AddShield(entity.Stats.defense);
        entity.CombatManager.UseActions(skill.actionCost);
    }

    public void Quickshot() {
        Skill skill = skillList.Get(SkillID.QUICKSHOT);
        Utils.CombatUtil.UseSimpleDamageSkill(entity, skill);
    }

    public void Fireball() {
        Skill skill = skillList.Get(SkillID.FIREBALL);
        Utils.CombatUtil.UseSimpleDamageSkill(entity, skill);
    }
}
