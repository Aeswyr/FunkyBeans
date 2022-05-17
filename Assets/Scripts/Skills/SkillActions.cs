using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActions : MonoBehaviour
{

    [SerializeField] private CombatEntity entity;
    public void Strike() {
        Debug.Log($"OUCH: {entity.Stats.speed}");
    }
}
