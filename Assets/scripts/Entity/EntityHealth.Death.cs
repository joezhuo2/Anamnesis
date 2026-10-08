using System;
using System.Collections;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SettingsSystem;
using CrystalFlux.StatusEffectSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.EntitySystem
{
    public partial class EntityHealth
    {
        private void StartDeathSequence()
        {
            AddOverhealth(-Overhealth);
            esm.AddStat(new StatBuff(StatType.isAlive, -1));

            if (cpum != null) cpum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnDeath);

            OnDeath?.Invoke(gameObject);

            TrySplit();

            if (killSrc != null && killSrc.TryGetComponent<PlayerUpgradeManager>(out var killPum))
                killPum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnKill, gameObject);

            if (ownSem != null)
                ownSem.ClearAllEffects();

            barRetired = true;
            PrefabPool.Release(ref healthBarInstance);
            PrefabPool.Release(ref healthBarTextInstance);

            if (animator != null && !IsAlive && deathAnimTime > 0f)
            {
                animator.SetBool(IsDeadHash, true);
                StartCoroutine(DeathDelay(deathAnimTime));
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void DropGold(GameObject target)
        {
            if (target == null) return;
            if (!GameSettings.Current.goldDropsEnabled) return;
            if (esm.GetStat(StatType.goldDrop) <= 0) return;

            float stealing = target.TryGetComponent<IStatProvider>(out var tsm) ? tsm.GetStat(StatType.Stealing) : 0f;

            int gold = Mathf.RoundToInt(esm.GetStat(StatType.goldDrop) * UnityEngine.Random.Range(0.7f, 1.3f) * (1f + (stealing * 0.01f)));

            if (gold > 0 && target.TryGetComponent<ICurrencyHolder>(out var ich))
            {
                ich.AddCurrency(gold);

                TextIndicatorSpawner tis = TextIndicatorSpawner.Instance;
                if (tis != null)
                {
                    Color goldColor = new(1f, 0.843f, 0f);
                    tis.SpawnTextIndicator(
                        gold,
                        transform.position,
                        goldColor,
                        0.7f + UnityEngine.Random.Range(0f, 0.15f),
                        UnityEngine.Random.Range(0.5f, 0.7f),
                        UnityEngine.Random.Range(0.8f, 1.2f),
                        UnityEngine.Random.Range(0f, 0.2f),
                        TextType.Gold
                    );
                }
            }
        }

        private void TrySplit()
        {
            if (TryGetComponent<EntitySplitting>(out var splitting))
                splitting.Split();
        }

        private IEnumerator DeathDelay(float delay)
        {
            yield return null;
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}
