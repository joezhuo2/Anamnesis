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
        protected bool IsAllowed(AttackReward r, int wave)
        {
            if (r == null || r.minWave > wave || r.newAttack == null) return false;
            if (r.newAttack.type == AttackType.Ultimate && !RunMode.UltimatesUnlocked) return false;
            return r.newAttack is not AttackData ad || ad.MinMode <= RunMode.Tier;
        }

        protected bool IsAllowed(PlayerUpgradeReward r, int wave)
        {
            if (r == null || r.minWave > wave) return false;
            return r.upgrade is not PlayerUpgrade pu || pu.minMode <= RunMode.Tier;
        }

        protected void UpdateOccasionalWaveRewards(int wave)
        {
            if (pendingOccasionalRerolls < 0) RollOccasionalWaveRewards(wave);

            if (pendingOccasionalSkillPoints > 0)
            {
                CachePlayerSkillTree();
                if (cpst != null) cpst.AddSkillPoints(pendingOccasionalSkillPoints);
            }
            rerolls += pendingOccasionalRerolls;

            pendingOccasionalRerolls = -1;
            pendingOccasionalSkillPoints = 0;
        }

        private void RollOccasionalWaveRewards(int wave)
        {
            if (wave % 5 == 0)
            {
                pendingOccasionalRerolls = 1;
                pendingOccasionalSkillPoints = 1;
            }
            else
            {
                pendingOccasionalRerolls = Random.value < 0.5f + D.occasionalRerollChanceAdd ? 1 : 0;
                pendingOccasionalSkillPoints = Random.value < 0.5f + D.occasionalSkillPointChanceAdd ? 1 : 0;
            }

            if (IronmanSelector.Enabled) pendingOccasionalRerolls = 0;
        }

        protected void RollAndAnnounceWaveRewards()
        {
            int wave = GetCurrentWave();
            RollOccasionalWaveRewards(wave);

            bool anomalyCompleted = currentAnomaly != null && currentAnomaly.isActive;
            pendingAnomalyRerolls = anomalyCompleted ? RollAnomalyRerolls() : 0;
            pendingAnomalySkillPoints = anomalyCompleted ? AnomalySkillPointGain() : 0;
            RollContractRewards();

            int rerollGain = pendingOccasionalRerolls + pendingAnomalyRerolls + pendingContractRerolls;
            int skillPointGain = pendingOccasionalSkillPoints + pendingAnomalySkillPoints + pendingContractSkillPoints;
            bool broken = ContractBroken;
            if (rerollGain <= 0 && skillPointGain <= 0 && !broken || !showCompletionMessage) return;

            string msg = "";
            if (rerollGain > 0) msg = $"+{rerollGain} Reroll{(rerollGain > 1 ? "s" : "")}";
            if (skillPointGain > 0)
            {
                if (msg.Length > 0) msg += ", ";
                msg += $"+{skillPointGain} Skill Point{(skillPointGain > 1 ? "s" : "")}";
            }
            if (broken) msg += msg.Length > 0 ? ", Contract Broken" : "Contract Broken";

            GameController?.SetSubtitleForDuration(msg, 0.5f, 0.25f, 0.25f);
        }
        protected virtual void TriggerStandardRewards(int w)
        {
            pendingStandardRewards = false;
            if (currentAnomaly != null) currentAnomaly.Cleanup();

            currentAnomaly = null;

            if (w % milestoneInterval == 0) GenerateMilestoneRewards();
            else if (w % 10 == 0 && w <= 20) GenerateTreasurePool();
            else if (w % 5 == 0 && w <= 15) GenerateRarePool();
            else if (w % 5 == 0) GenerateMixedPool();
            else GenerateRewards();
        }

        protected void GenerateRewards()
        {
            type = RewardType.Basic;
            int rewardChoices = PoolPreSetup();

            for (int i = 0; i < rewardChoices; i++)
            {
                if (baseBuffPool.Count == 0 || rarityData.Count == 0) break;

                BaseReward randomBuff = GetWeightedRandomBuff();

                RarityData chosenRarity = WaveQuality.GetWeightedRandomRarity(GetCurrentWave(), rarityData, Quality);

                GeneratedReward generated = new() { br = randomBuff, rd = chosenRarity };

                GameObject btnObj = GetOrCreateRewardButton();

                CachePlayerStatManager();
                string changeLine = BuildChangeLine(randomBuff.baseBuff.type, generated.finalVal);

                if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(generated, OnRewardClaimed, changeLine);
            }
            additionalQuality = 0f;
        }
        protected void GenerateMixedPool()
        {
            type = RewardType.Mixed;
            int rewardChoices = PoolPreSetup();

            for (int i = 0; i < rewardChoices; i++)
            {
                float poolRoll = Random.Range(0f, 100f);

                if (poolRoll < 75f)
                {
                    if (mixedPool.Count == 0 || rarityData.Count == 0) continue;

                    BaseReward randomBuff = GetWeightedRandomMixedBuff();
                    RarityData chosenRarity = WaveQuality.GetWeightedRandomRarity(GetCurrentWave(), rarityData, Quality);
                    GeneratedReward generated = new() { br = randomBuff, rd = chosenRarity };

                    GameObject btnObj = GetOrCreateRewardButton();

                    CachePlayerStatManager();
                    string changeLine = BuildChangeLine(randomBuff.baseBuff.type, generated.finalVal);

                    if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(generated, OnRewardClaimed, changeLine);
                }
                else if (poolRoll < 90f)
                {
                    AttackReward buff = PickRareReward();
                    if (buff == null) continue;
                    GameObject btnObj = GetOrCreateRewardButton();
                    if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnAttackRewardClaimed);
                }
                else
                {
                    PlayerUpgradeReward buff = PickTreasureReward();
                    if (buff == null) continue;
                    GameObject btnObj = GetOrCreateRewardButton();
                    if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnPlayerUpgradeRewardClaimed);
                }
            }

            additionalQuality = 0f;
        }
        protected void GenerateRarePool()
        {
            type = RewardType.Rare;
            int rewardChoices = PoolPreSetup();

            for (int i = 0; i < rewardChoices; i++)
            {
                AttackReward buff = PickRareReward();

                if (buff == null) break;

                GameObject btnObj = GetOrCreateRewardButton();

                if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                    rewardButton.Setup(buff, OnAttackRewardClaimed);
            }
        }
        protected void GenerateTreasurePool()
        {
            type = RewardType.Treasure;
            int rewardChoices = PoolPreSetup();

            for (int i = 0; i < rewardChoices; i++)
            {
                PlayerUpgradeReward buff = PickTreasureReward();

                if (buff == null) break;

                GameObject btnObj = GetOrCreateRewardButton();

                if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                    rewardButton.Setup(buff, OnPlayerUpgradeRewardClaimed);
            }
        }

        public bool TryStartPreRunPicks()
        {
            if (D.preRunPickCount <= 0) return false;
            if (!HasPreRunChoices()) return false;

            ActiveManager = this;
            type = RewardType.PreRun;

            OpenRewardButtons();
            GeneratePreRunPicks();
            UpdateRerollUI();

            return true;
        }

        protected bool HasPreRunChoices()
        {
            int wave = GetCurrentWave();
            return availableRarePool.Exists(a => IsAllowed(a, wave))
                || availableTreasurePool.Exists(t => IsAllowed(t, wave));
        }

        protected void GeneratePreRunPicks()
        {
            type = RewardType.PreRun;
            PanelSetup();

            int picks = Mathf.Max(1, D.preRunPickCount) - LockedSlots;

            for (int i = 0; i < picks; i++)
            {
                if (Random.Range(0f, 100f) < D.preRunTreasureChance && TryAddPreRunTreasure()) continue;
                if (TryAddPreRunRare()) continue;
                TryAddPreRunTreasure();
            }
        }

        private bool TryAddPreRunRare()
        {
            AttackReward buff = PickRareReward();
            if (buff == null) return false;

            GameObject btnObj = GetOrCreateRewardButton();
            if (btnObj == null) return false;

            if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnAttackRewardClaimed);
            return true;
        }
        private bool TryAddPreRunTreasure()
        {
            PlayerUpgradeReward buff = PickTreasureReward();
            if (buff == null) return false;

            GameObject btnObj = GetOrCreateRewardButton();
            if (btnObj == null) return false;

            if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnPlayerUpgradeRewardClaimed);
            return true;
        }
        protected AttackReward PickRareReward()
        {
            int wave = GetCurrentWave();
            AttackReward chosen = null;
            int eligible = 0;

            for (int i = 0; i < availableRarePool.Count; i++)
            {
                AttackReward candidate = availableRarePool[i];
                if (!IsAllowed(candidate, wave)) continue;
                if (keptReward != null && keptReward.Attack == candidate) continue;

                eligible++;
                if (Random.Range(0, eligible) == 0) chosen = candidate;
            }

            return chosen;
        }
        protected AttackReward PickCorruptionSpecialReward()
        {
            int wave = GetCurrentWave();
            AttackReward chosen = null;
            int eligible = 0;

            for (int i = 0; i < availableCorruptionSpecialPool.Count; i++)
            {
                AttackReward candidate = availableCorruptionSpecialPool[i];
                if (!IsAllowed(candidate, wave)) continue;
                if (corruptionSpecialsThisRoll.Contains(candidate)) continue;

                eligible++;
                if (Random.Range(0, eligible) == 0) chosen = candidate;
            }

            return chosen;
        }
        protected PlayerUpgradeReward PickTreasureReward()
        {
            int wave = GetCurrentWave();
            PlayerUpgradeReward chosen = null;
            int eligible = 0;

            for (int i = 0; i < availableTreasurePool.Count; i++)
            {
                PlayerUpgradeReward candidate = availableTreasurePool[i];
                if (!IsAllowed(candidate, wave)) continue;
                if (keptReward != null && keptReward.Upgrade == candidate) continue;

                eligible++;
                if (Random.Range(0, eligible) == 0) chosen = candidate;
            }

            return chosen;
        }

        protected BaseReward GetWeightedRandomBuff() => GetWeightedRandom(baseBuffPool);

        protected BaseReward GetWeightedRandomMixedBuff() => GetWeightedRandom(mixedPool);

        private BaseReward GetWeightedRandom(List<BaseReward> pool)
        {
            CachePlayerStatManager();

            float totalWeight = 0;
            foreach (var b in pool) if (!IsMaxed(b)) totalWeight += b.weight;
            if (totalWeight <= 0f) return pool[0];

            float roll = Random.Range(0f, totalWeight);
            float weightSum = 0;

            foreach (var b in pool)
            {
                if (IsMaxed(b)) continue;
                weightSum += b.weight;
                if (roll <= weightSum) return b;
            }

            return pool[0];
        }

        private bool IsMaxed(BaseReward b) => cpsm != null && b.baseBuff.value > 0f && StatCaps.IsMaxed(b.baseBuff.type, cpsm.GetStat(b.baseBuff.type));
        protected void OnRewardClaimed(GeneratedReward chosenReward)
        {
            CloseRewardUI();

            StatBuff finalBuff = new(chosenReward.br.baseBuff.type, chosenReward.finalVal);
            CachePlayerStatManager();
            if (cpsm != null) cpsm.AddStat(finalBuff);

            ResumeGameLoop();
        }
        protected void OnAttackRewardClaimed(AttackReward chosenAttack)
        {
            CloseRewardUI();

            if (cpah == null) cpah = GameObject.FindWithTag("Player")?.GetComponent<IAttackHandler>();
            if (cpah != null && chosenAttack.newAttack != null)
                cpah.UpdateAttack(chosenAttack.newAttack.type, chosenAttack.newAttack);
            if (availableRarePool.Contains(chosenAttack)) availableRarePool.Remove(chosenAttack);
            if (availableCorruptionSpecialPool.Contains(chosenAttack)) availableCorruptionSpecialPool.Remove(chosenAttack);

            ResumeGameLoop();
        }
        protected void OnPlayerUpgradeRewardClaimed(PlayerUpgradeReward chosenUpgrade)
        {
            CloseRewardUI();

            if (cpum == null) cpum = GameObject.FindWithTag("Player")?.GetComponent<IUpgradeHolder>();
            if (cpum != null) cpum.AddUpgrade(chosenUpgrade.upgrade);
            if (availableTreasurePool.Contains(chosenUpgrade)) availableTreasurePool.Remove(chosenUpgrade);

            ResumeGameLoop();
        }
    }
}
