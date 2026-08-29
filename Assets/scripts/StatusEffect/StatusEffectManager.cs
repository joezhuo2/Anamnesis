using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    public class StatusEffectManager : MonoBehaviour, IStatusEffectReceiver
    {
        public GameObject displayPrefab = null;
        public Transform displayContainer = null;

        [HideInInspector] public readonly List<StatusEffect> activeEffects = new();
        private IStatProvider cesm;

        private void Awake() => cesm = GetComponent<IStatProvider>();

        public void GetActiveEffectsOfType<T>(List<T> results) where T : EffectAsset
        {
            results.Clear();
            for (int i = 0; i < activeEffects.Count; i++)
                if (activeEffects[i] is T) results.Add(activeEffects[i] as T);
        }

        public T GetActiveFirstEffectOfType<T>() where T : EffectAsset
        {
            for (int i = 0; i < activeEffects.Count; i++)
                if (activeEffects[i] is T) return activeEffects[i] as T;
            return null;
        }

        public void Apply(EffectAsset effect, GameObject source, Vector2 location = default)
        {
            if (effect is not StatusEffect se) return;

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
            runtimeEffect.location = location;
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

        public void RemoveStacks<T>(int stacksToRemove) where T : EffectAsset
        {
            StatusEffect existing = GetActiveFirstEffectOfType<T>() as StatusEffect;
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

        public void RemoveEffect<T>() where T : EffectAsset => RemoveStacks<T>(int.MaxValue);

        public void RemoveEffectAfterDelay<T>(float delay) where T : EffectAsset
            => StartCoroutine(RemoveEffectAfterDelayInternal<T>(delay));

        public IEnumerator RemoveEffectAfterDelayInternal<T>(float delay) where T : EffectAsset
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
}
