using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public GameObject displayPrefab = null;
    public Transform displayContainer = null;

    [HideInInspector] public readonly List<StatusEffect> activeEffects = new();
    private IStatProvider cesm;

    private void Awake() => cesm = GetComponent<IStatProvider>();
    public void GetActiveEffectsOfType<T>(List<T> results) where T : StatusEffect
    {
        results.Clear();
        for (int i = 0; i < activeEffects.Count; i++)
            if (activeEffects[i] is T) results.Add(activeEffects[i] as T);
    }

    public T GetActiveFirstEffectOfType<T>() where T : StatusEffect
    {
        for (int i = 0; i < activeEffects.Count; i++)
            if (activeEffects[i] is T) return activeEffects[i] as T;
        return null;
    }

    public StatusEffect GetEffect(StatusEffect se)
    {
        if (activeEffects.Contains(se)) return se;
        return null;
    }

    public void AddEffectAfterDelay(StatusEffect se, GameObject source, float delay, GameObject projectile = null)
        => StartCoroutine(AddEffectAfterDelayCoroutine(se, source, delay, projectile));

    public IEnumerator AddEffectAfterDelayCoroutine(StatusEffect se, GameObject source, float delay, GameObject projectile = null)
    {
        if (se == null) yield break;
        yield return new WaitForSeconds(delay);
        AddEffect(se, source, projectile);
    }

    public void AddEffect(StatusEffect se, GameObject source, GameObject projectile = null)
    {
        if (se == null) return;

        StatusEffect existing = activeEffects.Find(
            e => e.GetType() == se.GetType() ||
            e.GetType().IsSubclassOf(se.GetType()) ||
            se.GetType().IsSubclassOf(e.GetType())
        );

        if (existing != null)
        {
            existing.currentTime = 0f;
            if (existing.currentStacks < existing.maxStacks) existing.currentStacks++;
            existing.OnStack();
            return;
        }

        StatusEffect runtimeEffect = Instantiate(se);
        runtimeEffect.target = gameObject;
        runtimeEffect.source = source;
        runtimeEffect.projectile = projectile;
        runtimeEffect.currentStacks = 1;
        runtimeEffect.currentTime = 0;

        if (source != null && source.TryGetComponent<IStatProvider>(out var sem))
        {
            if (sem.GetStat(StatType.seDurPct) != 0f)
                runtimeEffect.duration *= 1f + (sem.GetStat(StatType.seDurPct) * 0.01f);

            if (sem.GetStat(StatType.seTickRatePct) != 0f && runtimeEffect.tickInterval > 0f)
                runtimeEffect.tickInterval = Mathf.Max(0.1f, runtimeEffect.tickInterval / (1f + (sem.GetStat(StatType.seTickRatePct) * 0.01f)));

            runtimeEffect.potencyMultiplier = 1f + (sem.GetStat(StatType.sePotPct) * 0.01f);
        }

        activeEffects.Add(runtimeEffect);
        runtimeEffect.OnApply();

        CreateDisplayUI(runtimeEffect);
    }

    public void RemoveStacks<T>(int stacksToRemove) where T : StatusEffect
    {
        T existing = GetActiveFirstEffectOfType<T>();
        if (existing == null) return;

        existing.currentStacks = Mathf.Max(0, existing.currentStacks - stacksToRemove);

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

    public void RemoveEffect<T>() where T : StatusEffect => RemoveStacks<T>(int.MaxValue);
    public void RemoveEffect(StatusEffect se)
    {
        if (!activeEffects.Contains(se)) return;

        se.currentStacks = 0;
        se.OnExpire();
        activeEffects.Remove(se);
        Destroy(se);
    }

    public IEnumerator RemoveEffectAfterDelay<T>(float delay) where T : StatusEffect
    {
        yield return new WaitForSeconds(delay);
        RemoveEffect<T>();
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

        if (activeEffects.Count == 0) return;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (i < 0 || i >= activeEffects.Count) return;
            StatusEffect e = activeEffects[i];
            if (e == null) continue;

            if (e.tickInterval > 0)
            {
                int oldTicks = Mathf.FloorToInt(e.currentTime / e.tickInterval);
                int newTicks = Mathf.FloorToInt((e.currentTime + dt) / e.tickInterval);

                if (newTicks > oldTicks) e.OnTick();
            }

            e.currentTime += dt;

            float effDur = e.isBuff ? e.duration : e.duration * (1f - (cesm.GetStat(StatType.EffectRes) * 0.01f));

            if (e != null && e.currentTime > effDur)
            {
                if (e.currentStacks > 1 && !e.loseAllStacksOnExpire)
                {
                    e.currentStacks--;
                    e.currentTime = 0f;
                    e.OnStack();
                }
                else
                {
                    e.OnExpire();
                    if (i >= 0 && i < activeEffects.Count && activeEffects[i] != null)
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
            secui.Setup(se, cesm);
    }
}