using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActions : MonoBehaviour
{
    public List<Vector3Int> targetPositions { get; set; }
    public Vector3 mousePos { get; set; }


    [SerializeField] private SkillList skillList;

    [SerializeField] private CombatEntity entity;
    public void Strike() 
    {
        Utils.CombatUtil.UseSimpleDamageSkill(entity, SkillID.STRIKE, skillList, targetPositions, mousePos);
    }
    public void Hew()
    {
        Utils.CombatUtil.UseSimpleDamageSkill(entity, SkillID.HEW, skillList, targetPositions, mousePos);
    }

    public void Block() {
        Skill skill = skillList.Get(SkillID.BLOCK);
        entity.AddArmor(entity.Stats.defense);
        entity.GetServerCombatManager().UseActions(skill.actionCost);
    }

    public void Quickshot() {
        Skill skill = skillList.Get(SkillID.QUICKSHOT);
        Utils.CombatUtil.UseSimpleDamageSkill(entity, SkillID.QUICKSHOT, skillList, targetPositions, mousePos);
    }

    public void Fireball() {
        Skill skill = skillList.Get(SkillID.FIREBALL);
        Utils.CombatUtil.UseSimpleDamageSkill(entity, SkillID.FIREBALL, skillList, targetPositions, mousePos);
    }

}
