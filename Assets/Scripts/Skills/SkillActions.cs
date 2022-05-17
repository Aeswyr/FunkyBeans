using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActions : MonoBehaviour
{
    [SerializeField] private SkillList skillList;

    [SerializeField] private CombatEntity entity;
    public void Strike() {
        Debug.Log($"OUCH: {entity.Stats.speed}");
    }
    public void Box()
    {
        Debug.Log($"OUCH: {entity.Stats.speed}");
    }
}
