using System;
using UnityEngine.Events;


[Serializable]
public struct Skill
{
    public enum Type {
        COLD, FLAME, FLURRY, LIGHTNING, PSYCHIC, MIGHT, NATURE, TACTICAL,
        FUNDEMENTALS, STARNOMAD, SPELLSWORD
    }
    public enum Category {
        ATTACK, ABILITY
    }

    public enum Target {
        SQUARE, RADIUS, LINE, ARC
    }

    public string name;
    public Type[] types;
    public Category category;
    public Target target;
    public int range;
    public int size;
    public int speedCost;
    public int manaCost;
    public int actionCost;

    public UnityEvent behavior;
}
