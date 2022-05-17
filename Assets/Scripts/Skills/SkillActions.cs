using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActions : MonoBehaviour
{

    [SerializeField] private CombatEntity stats;
    public void Strike() {
        Debug.Log($"OUCH: {stats.Speed}");
    }
}
