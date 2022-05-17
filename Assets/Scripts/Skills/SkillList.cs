using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillList", menuName = "FunkyBeans/SkillList", order = 0)]
public class SkillList : ScriptableObject {
    [SerializeField] private List<Skill> skills;
}
