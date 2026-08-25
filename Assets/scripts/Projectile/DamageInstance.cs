using UnityEngine;

public enum DamageType { Physical, Spell, DoT, True }

public struct DamageInstance
{
    public DamageType type;
    public float amount;
    public bool isCrit;
    public Color indicatorColor;
    public GameObject owner;

    public DamageInstance(DamageType type, float amount, bool isCrit, Color indicatorColor, GameObject owner)
    {
        this.type = type;
        this.amount = amount;
        this.isCrit = isCrit;
        this.indicatorColor = indicatorColor;
        this.owner = owner;
    }
}