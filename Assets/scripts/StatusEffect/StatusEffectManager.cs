using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityStatManager))]
public class StatusEffectManager : MonoBehaviour
{
    public GameObject displayPrefab = null;
    public Transform displayContainer = null;

    [HideInInspector] public readonly List<StatusEffect> activeEffects = new();
    private EntityStatManager cesm;

    private void Awake()
    {
        cesm = GetComponent<EntityStatManager>();
    }
    public void GetActiveEffectsOfType<T>(List<StatusEffect> results) where T : StatusEffect
    {
        results.Clear();
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] is T)
            {
                results.Add(activeEffects[i]);
            }
        }
    }
    public T GetActiveFirstEffectOfType<T>() where T : StatusEffect
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] is T) return activeEffects[i] as T;
        }
        return null;
    }

    public void AddEffect(StatusEffect se, GameObject source)
    {
        if (se == null) return;

        StatusEffect existing = activeEffects.Find(e => e.GetType() == se.GetType());

        if (existing != null)
        {
            existing.currentTime = 0f;
            if (existing.currentStacks < existing.maxStacks) existing.currentStacks++;
            existing.OnStack();
        }
        else
        {
            StatusEffect runtimeEffect = Instantiate(se);
            runtimeEffect.target = gameObject;
            runtimeEffect.source = source;
            runtimeEffect.currentStacks = 1;
            runtimeEffect.currentTime = 0;

            activeEffects.Add(runtimeEffect);
            runtimeEffect.OnApply();

            CreateDisplayUI(runtimeEffect);
        }
    }
    public void RemoveStacks<T>(int stacksToRemove) where T : StatusEffect
    {
        T existing = GetActiveFirstEffectOfType<T>();
        if (existing == null) return;

        existing.currentStacks -= stacksToRemove;

        if (existing.currentStacks <= 0)
        {
            existing.OnExpire();
            activeEffects.Remove(existing);
            Destroy(existing);
        }
        else
        {
            existing.OnStack();
        }
    }
    public IEnumerator RemoveEffectAfterDelay<T>(float delay) where T : StatusEffect
    {
        yield return new WaitForSeconds(delay);
        RemoveStacks<T>(int.MaxValue);
    }
    public void ClearAllEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeEffects[i];
            if (effect == null) continue;

            effect.OnExpire();
            activeEffects.RemoveAt(i);
            Destroy(effect);
        }
    }
    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect e = activeEffects[i];
            if (e == null) continue;

            if (e.tickInterval > 0)
            {
                int oldTicks = Mathf.FloorToInt(e.currentTime / e.tickInterval);
                int newTicks = Mathf.FloorToInt((e.currentTime + dt) / e.tickInterval);

                if (newTicks > oldTicks) e.OnTick();
            }

            e.currentTime += dt;

            if (e.currentTime > e.duration)
            {
                if (e.currentStacks > 1)
                {
                    e.currentStacks--;
                    e.currentTime = 0f;
                    e.OnStack();
                }
                else
                {
                    e.OnExpire();
                    activeEffects.RemoveAt(i);
                    Destroy(e);
                }
            }
        }
    }
    private void CreateDisplayUI(StatusEffect se)
    {
        if (displayPrefab == null || displayContainer == null) return;
        GameObject uiObj = Instantiate(displayPrefab, displayContainer);

        if (uiObj.TryGetComponent<StatusEffectCooldownUI>(out var secui))
        {
            PlayerStats ps = (cesm != null && cesm.s is PlayerStats) ? cesm.s as PlayerStats : null;
            secui.Setup(se, ps, cesm);
        }
    }
}