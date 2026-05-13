using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    [Header("Basic")]
    public float duration; // in seconds
    public float tickInterval; // how often effect triggers
    protected GameObject target;
    
    [Header("Stacking")]
    public int maxStacks = 1;
    public int currentStacks;

    public virtual void OnTick() {}
    public virtual void OnApply() {}
    public virtual void OnExpire() {}
    public virtual void OnStack() {}
}