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

        GameObject IStatusEffectReceiver.DisplayPrefab { set => displayPrefab = value; }
        Transform IStatusEffectReceiver.DisplayContainer { set => displayContainer = value; }

        [HideInInspector] public readonly List<StatusEffect> activeEffects = new();
        private IStatProvider cesm;

        private void Awake()
        {
            cesm = GetComponent<IStatProvider>();
            if (cesm == null)
                Debug.LogError($"StatusEffectManager on '{name}' requires a component implementing IStatProvider on the same GameObject. Effect resistance and stat-scaled durations will be ignored.", this);
        }

        private bool isQuitting;

        private void OnApplicationQuit() => isQuitting = true;

        private void OnDestroy()
        {
            if (isQuitting)
            {
                for (int i = activeEffects.Count - 1; i >= 0; i--)
                    if (activeEffects[i] != null) Destroy(activeEffects[i]);

                activeEffects.Clear();
                return;
            }

            ClearAllEffects();
        }

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

        private static bool IsSameEffect(StatusEffect a, StatusEffect b)
        {
            if (a == null || b == null) return false;
            if (ReferenceEquals(a, b)) return true;

            string an = a.effName, bn = b.effName;
            if (!string.IsNullOrEmpty(an) && !string.IsNullOrEmpty(bn))
                return ReferenceEquals(an, bn) || string.Equals(an, bn, System.StringComparison.OrdinalIgnoreCase);

            System.Type at = a.GetType(), bt = b.GetType();
            return at == bt || at.IsSubclassOf(bt) || bt.IsSubclassOf(at);
        }

        public void Apply(EffectAsset effect, GameObject source, Vector2 location = default)
        {
            if (effect is not StatusEffect se) return;

            StatusEffect existing = null;
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (IsSameEffect(activeEffects[i], se))
                {
                    existing = activeEffects[i];
                    break;
                }
            }

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

        public void RemoveDebuffs(int count)
        {
            if (count <= 0) return;

            for (int i = activeEffects.Count - 1; i >= 0 && count > 0; i--)
            {
                StatusEffect e = activeEffects[i];
                if (e == null) continue;
                if (e.isBuff) continue;

                e.OnExpire();
                activeEffects.RemoveAt(i);
                Destroy(e);
                count--;
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
                StatusEffect e = activeEffects[i];
                if (e == null) continue;

                e.OnExpire();
                activeEffects.RemoveAt(i);
                Destroy(e);
            }
        }
        private void Update()
        {
            if (Time.timeScale == 0f || activeEffects.Count == 0) return;

            float dt = Time.deltaTime;
            float resMult = 1f - ((cesm != null ? cesm.GetStat(StatType.EffectRes) : 0f) * 0.01f);

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect e = activeEffects[i];
                if (e == null) continue;

                if (e.tickInterval > 0)
                {
                    int oldTicks = Mathf.FloorToInt(e.currentTime / e.tickInterval);
                    int newTicks = Mathf.FloorToInt((e.currentTime + dt) / e.tickInterval);

                    if (newTicks > oldTicks)
                    {
                        e.OnTick();
                        if (i >= activeEffects.Count || activeEffects[i] != e)
                        {
                            i = Mathf.Min(i, activeEffects.Count);
                            continue;
                        }
                    }
                }

                e.currentTime += dt;

                float effDur = e.isBuff ? e.duration : e.duration * resMult;

                if (e.currentTime > effDur)
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
                        if (i < activeEffects.Count && activeEffects[i] == e)
                            activeEffects.RemoveAt(i);
                        else
                            activeEffects.Remove(e);
                        Destroy(e);
                    }
                }
            }
        }
        private void CreateDisplayUI(StatusEffect se)
        {
            if (displayPrefab == null || displayContainer == null) return;
            GameObject uiObj = PrefabPool.Acquire(displayPrefab, displayContainer);
            if (uiObj == null) return;

            if (uiObj.TryGetComponent<StatusEffectCooldownUI>(out var secui))
                secui.Setup(se, cesm);
        }
    }
}
