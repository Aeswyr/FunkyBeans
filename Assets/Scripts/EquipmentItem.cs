using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    weapon,
    head,
    body,
    ring,
    summon,
    max
}

public struct EquipmentItem
{
    [SerializeField] private ItemType type;
    [SerializeField] private string name;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Stats stats;
    [SerializeField] private List<SkillID> skills;
    [SerializeField] private long id;

    public ItemType Type => type;
    public string Name => name;
    public Sprite Sprite => sprite;
    public Stats Stats => stats;
    public List<SkillID> Skills => skills;
    public long ID => id;
}
