using System.ComponentModel.Design;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private readonly List<StatusEffect> activeEffects = new();

    public void AddEffect(StatusEffect se, GameObject source)
    {
        StatusEffect existing = activeEffects.Find(e => e.GetType() == se.GetType());

        if (existing != null)
        {
            if (existing.currentStacks < existing.maxStacks)
            {
                existing.currentStacks++;
                existing.OnStack();
            }
            else
            {
                existing.currentTime = 0;
            }
        }
        else
        {
            StatusEffect runtimeEffect = Instantiate(se);
            runtimeEffect.target = gameObject;
            runtimeEffect.currentStacks = 1;
            runtimeEffect.source = source;
            runtimeEffect.currentTime = 0;

            activeEffects.Add(runtimeEffect);
            runtimeEffect.OnApply();
        }
    }

    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect e = activeEffects[i];

            if (e.tickInterval > 0)
            {
                int oldTicks = Mathf.FloorToInt(e.currentTime / e.tickInterval);
                int newTicks = Mathf.FloorToInt((e.currentTime + Time.deltaTime) / e.tickInterval);
                if (newTicks > oldTicks) e.OnTick();
            }

            e.currentTime += Time.deltaTime;

            if (e.currentTime > e.duration)
            {
                e.OnExpire();
                activeEffects.RemoveAt(i);
            }
        }
    }
}