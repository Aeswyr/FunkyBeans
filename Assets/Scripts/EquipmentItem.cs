using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ItemType
{
    weapon,
    head,
    body,
    ring,
    summon,
    other
}
public struct TempItem {
    public string Name;
    public ItemType type;
    public int SpriteID;
    public long id;
    public List<SkillID> skills;
    public Stats stats;
}
[Serializable] public struct EquipmentItem
{
    public ItemType Type;
    public string Name;
    public int SpriteID;
    public Stats Stats;
    public List<SkillID> Skills;
    public long ID;

    public EquipmentItem(ItemType type, string name, int spriteID, Stats stats, List<SkillID> skills, long id) {
        this.Type = type;
        this.Name = name;
        this.SpriteID = spriteID;
        this.Stats = stats;
        this.Skills = skills;
        this.ID = id;
    }
}
