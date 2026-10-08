using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SettingsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.WaveSystem
{
    public partial class WaveManager
    {
        protected void GenerateMilestoneRewards()
        {
            type = RewardType.Milestone;
            int rewardChoices = Mathf.Min(Mathf.Max(1, milestoneRewardChoices + D.milestoneRewardChoicesAdd), milestoneRewards.Count) - LockedSlots;

            PanelSetup();

            var selectedRewards = GetWeightedRandomMilestoneRewards(rewardChoices);

            for (int i = 0; i < selectedRewards.Count; i++)
            {
                MilestoneReward sourceReward = selectedRewards[i];
                MilestoneRewardData generated = GenerateMilestoneRewardData(sourceReward);

                GameObject btnObj = GetOrCreateRewardButton();

                if (btnObj.TryGetComponent<RewardButton>(out var rb))
                    rb.Setup(generated, OnMilestoneRewardClaimed);
            }
        }
        protected List<MilestoneReward> GetWeightedRandomMilestoneRewards(int count)
        {
            var result = new List<MilestoneReward>();
            var available = new List<MilestoneReward>(milestoneRewards);
            if (keptReward != null && keptReward.Milestone != null)
            {
                string kn = keptReward.Milestone.rewardName;
                available.RemoveAll(r => r.rewardName == kn);
            }

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                float totalWeight = 0;
                foreach (var r in available) totalWeight += r.weight;

                float roll = Random.Range(0f, totalWeight);
                float weightSum = 0;

                for (int j = 0; j < available.Count; j++)
                {
                    weightSum += available[j].weight;
                    if (roll <= weightSum)
                    {
                        result.Add(available[j]);
                        available.RemoveAt(j);
                        break;
                    }
                }
            }

            return result;
        }
        protected MilestoneRewardData GenerateMilestoneRewardData(MilestoneReward source)
        {
            var data = new MilestoneRewardData
            {
                rewardName = source.rewardName,
                generatedBuffs = new List<StatBuff>()
            };

            foreach (var baseBuff in source.baseStatBuffs)
            {
                float varianceMultiplier = 1f + Random.Range(-source.variance, source.variance);
                float finalValue = baseBuff.value * varianceMultiplier;
                StatBuff generatedBuff = new(baseBuff.type, finalValue);
                data.generatedBuffs.Add(generatedBuff);
            }

            return data;
        }
        protected void RollSynergyOffer(int w)
        {
            if (pendingSynergy || w < synergyMinWave || !HasSynergyChoices()) return;

            if (Random.Range(0f, 100f) < synergyBaseChance + synergyChanceBonus)
            {
                pendingSynergy = true;
                synergyChanceBonus = 0f;
            }
            else synergyChanceBonus += synergyChanceGrowth;
        }

        protected bool CanOfferSynergy(StatType src, StatType tgt)
        {
            if (StatSynergy.Effective(src) == StatSynergy.Effective(tgt)) return false;
            if (cssm != null && cssm.HasSynergy(StatSynergy.Effective(src), tgt)) return false;

            for (int i = 0; i < synergiesThisRoll.Count; i++)
                if (synergiesThisRoll[i].source == StatSynergy.Effective(src) && synergiesThisRoll[i].target == tgt) return false;

            return true;
        }

        protected bool HasSynergyChoices()
        {
            if (synergyStatPool == null || synergyStatPool.Count < 2) return false;
            CachePlayerSynergy();
            synergiesThisRoll.Clear();

            foreach (var a in synergyStatPool)
            {
                if (a == null || !a.canBeSource || a.weight <= 0f) continue;
                foreach (var b in synergyStatPool)
                    if (b != null && b.canBeTarget && b.weight > 0f && CanOfferSynergy(a.stat, b.stat)) return true;
            }
            return false;
        }

        protected SynergyStatOption PickSynergyOption(bool asSource, StatType? src = null)
        {
            SynergyStatOption chosen = null;
            float total = 0f;

            foreach (var o in synergyStatPool)
            {
                if (o == null || o.weight <= 0f) continue;
                if (asSource ? !o.canBeSource : !o.canBeTarget) continue;
                if (src.HasValue && !CanOfferSynergy(src.Value, o.stat)) continue;

                total += o.weight;
                if (Random.Range(0f, total) < o.weight) chosen = o;
            }

            return chosen;
        }

        protected SynergyRewardData GenerateSynergyRewardData()
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                SynergyStatOption src = PickSynergyOption(true);
                if (src == null) return null;

                SynergyStatOption tgt = PickSynergyOption(false, src.stat);
                if (tgt == null) continue;

                float lo = Mathf.Min(minSynergyConversion, maxSynergyConversion);
                float hi = Mathf.Max(minSynergyConversion, maxSynergyConversion);
                float pct = Mathf.Round(Random.Range(lo, hi) * 10f) * 0.1f;

                StatSynergy sy = new(StatSynergy.Effective(src.stat), tgt.stat, pct);
                synergiesThisRoll.Add(sy);
                return new SynergyRewardData { sy = sy };
            }

            return null;
        }

        protected void GenerateSynergyRewards()
        {
            type = RewardType.Synergy;
            PanelSetup();
            CachePlayerStatManager();
            CachePlayerSynergy();
            synergiesThisRoll.Clear();
            if (keptReward != null && keptReward.Synergy?.sy != null) synergiesThisRoll.Add(keptReward.Synergy.sy);

            int choices = Mathf.Max(1, synergyChoices) - LockedSlots;

            for (int i = 0; i < choices; i++)
            {
                SynergyRewardData data = GenerateSynergyRewardData();
                if (data == null) break;

                GameObject btnObj = GetOrCreateRewardButton();
                if (btnObj == null) continue;

                string changeLine = "";
                if (cpsm != null)
                {
                    float gain = cpsm.GetStat(data.sy.source) * data.sy.pct * 0.01f;
                    if (StatSynergy.IsWholeStat(data.sy.target)) gain = Mathf.Floor(gain);
                    changeLine = $"Now: +{gain:0.##} {StatSynergy.StatName(data.sy.target)}";
                }

                if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(data, OnSynergyRewardClaimed, changeLine);
            }
        }

        protected void OnSynergyRewardClaimed(SynergyRewardData chosen)
        {
            CloseRewardUI();

            CachePlayerSynergy(true);
            if (cssm != null && chosen?.sy != null) cssm.AddSynergy(chosen.sy);

            ResumeGameLoop();
        }

        protected void CachePlayerSynergy(bool create = false)
        {
            if (cssm != null) return;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            if (!player.TryGetComponent(out cssm) && create) cssm = player.AddComponent<StatSynergyManager>();
        }

        protected void OnMilestoneRewardClaimed(MilestoneRewardData chosenReward)
        {
            CloseRewardUI();

            CachePlayerStatManager();
            if (cpsm != null)
            {
                foreach (var buff in chosenReward.generatedBuffs)
                    cpsm.AddStat(buff);
            }

            ResumeGameLoop();
        }
    }
}
