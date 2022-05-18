using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActions : MonoBehaviour
{
    [SerializeField] private SkillList skillList;

    [SerializeField] private CombatEntity entity;
    public void Strike() 
    {
        //Debug.Log($"OUCH: {entity.Stats.speed}");
        Skill skill = skillList.Get(SkillID.STRIKE);

        if(entity.EntitiyType == CombatEntity.EntityType.player)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(InputHandler.Instance.mousePos);

            Vector3Int entityPos = GameHandler.Instance.currentLevel.WorldToCell(entity.transform.parent.position);

            List<CombatEntity> entities = Utils.CombatUtil.GetEntitiesInAttack(entityPos, mousePos, entity.CombatManager, skill.target, skill.range, skill.size);
            Debug.Log("num entities: " + entities.Count);

            if ((entities.Count == 0) && (skill.requiresValidTarget))
                return;

            foreach(CombatEntity entity in entities)
            {
                entity.TakeDamage(entity.Stats.damage);
            }
        }
        else
        {
            //Uhhhhhhhhhhhhhhhhhhhhh yeah
        }

        //use actions
        entity.CombatManager.UseActions(skill.actionCost);
    }
    public void Box()
    {
        Debug.Log($"OUCH: {entity.Stats.speed}");
    }
}
