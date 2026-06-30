using UnityEngine;

public abstract class StatusEffect : ScriptableObject
{
    [Header("Basic")]
    [HideInInspector] public float currentTime;
    public float duration;
    [Tooltip("How often effect triggers")] public float tickInterval;

    [Header("UI")]
    public Sprite icon;
    public string effName;
    [TextArea(3, 10)] public string desc;

    [Header("Stacking")]
    public int maxStacks = 1;

    [HideInInspector] public GameObject target;
    [HideInInspector] public GameObject source;
    [HideInInspector] public int currentStacks = 0;

    public virtual void OnTick() {}
    public virtual void OnApply() {}
    public virtual void OnExpire() {}
    public virtual void OnStack() {}
}