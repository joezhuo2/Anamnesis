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
        protected int RollAnomalyRerolls()
        {
            if (IronmanSelector.Enabled) return 0;

            int min = Mathf.Max(0, 1 + D.anomalyRerollMinAdd);
            int max = Mathf.Max(min, 3 + D.anomalyRerollMaxAdd);
            return Random.Range(min, max + 1);
        }

        protected int AnomalySkillPointGain() => Mathf.Max(0, 1 + D.anomalySkillPointAdd);

        protected string GetAnomalyRewardLine()
        {
            List<string> parts = new();

            if (!IronmanSelector.Enabled)
            {
                int min = Mathf.Max(0, 1 + D.anomalyRerollMinAdd);
                int max = Mathf.Max(min, 3 + D.anomalyRerollMaxAdd);
                if (max > 0) parts.Add(min == max ? $"+{min} Reroll{(min > 1 ? "s" : "")}" : $"+{min}-{max} Rerolls");
            }

            int sp = AnomalySkillPointGain();
            if (sp > 0) parts.Add($"+{sp} Skill Point{(sp > 1 ? "s" : "")}");

            parts.Add("Increased Reward Quality");

            return $"Completion Reward: {string.Join(", ", parts)}";
        }

        private void UpdateContractInfo()
        {
            if (contractInfoText == null) return;

            contractInfoBackground.SetActive(true);

            int key;
            if (currentContract == null || currentContract.amd == null) key = -1;
            else if (contractPaused) key = -2;
            else if (!contractArmed) key = -3;
            else if (!currentContract.isActive) key = -4;
            else if (currentContract is TimeTrialInstance tt) key = Mathf.Max(0, Mathf.RoundToInt(tt.timeRemaining * 10f));
            else key = -3;

            if (key == lastContractKey) return;
            lastContractKey = key;

            string n = key == -1 ? "" : $"Contract: {currentContract.amd.anomalyName}";
            contractInfoText.text = key switch
            {
                -1 => "",
                -2 => $"{n} (Paused)",
                -3 => n,
                -4 => $"{n} - Broken",
                _ => $"{n} - {key * 0.1f:F1}s"
            };
        }

        private void SetAnomalyInfo(string s)
        {
            if (ReferenceEquals(s, lastInfo)) return;
            lastInfo = s;
            anomalyInfoText.text = s;
        }

        private void UpdateAnomalyTimeInfo()
        {
            if (currentAnomaly is TimeTrialInstance tt)
            {
                if (tt.timeRemaining <= 0f)
                {
                    lastInfoTick = int.MinValue;
                    SetAnomalyInfo("Time's Up! Anomaly Failed");
                    GameController?.SetTitleForDuration("Anomaly Failed", 2f, 0.5f, 0.5f);
                    GameController?.SetSubtitleForDuration("Time's Up!", 2f, 0.5f, 0.5f);
                    return;
                }

                int tick = Mathf.RoundToInt(tt.timeRemaining * 10f);
                if (tick == lastInfoTick) return;
                lastInfoTick = tick;
                SetAnomalyInfo($"Time Remaining: {tick * 0.1f:F1}s");
            }
        }

        protected void ArmContract(bool boss)
        {
            contractArmed = false;
            contractPaused = false;
            if (currentContract == null || currentContract.amd == null) return;

            if (boss && currentContract.amd.disallowOnBossWave)
            {
                contractPaused = true;
                return;
            }

            currentContract.StartAnomaly();
            contractArmed = true;
        }

        protected void EvaluateWaveEnd()
        {
            if (currentAnomaly != null && currentAnomaly.isActive) currentAnomaly.OnWaveEnd();
            if (contractArmed && currentContract != null && currentContract.isActive) currentContract.OnWaveEnd();
        }

        protected void DisarmContract()
        {
            if (currentContract != null) currentContract.ResetForWave();
            contractArmed = false;
            contractPaused = false;
        }

        protected void RollContractRewards()
        {
            pendingContractRerolls = 0;
            pendingContractSkillPoints = 0;
            pendingContractPool = false;
            if (!ContractHeld) return;

            AnomalyData amd = currentContract.amd;
            if (!IronmanSelector.Enabled) pendingContractRerolls = Mathf.Max(0, amd.contractRerolls);
            if (Random.Range(0f, 100f) < amd.contractSkillPointChance) pendingContractSkillPoints = 1;
            pendingContractPool = Random.Range(0f, 100f) < amd.contractMixedPoolChance;
        }

        protected void ApplyContractRewards()
        {
            rerolls += pendingContractRerolls;

            if (pendingContractSkillPoints > 0)
            {
                CachePlayerSkillTree();
                if (cpst != null) cpst.AddSkillPoints(pendingContractSkillPoints);
            }

            pendingContractRerolls = 0;
            pendingContractSkillPoints = 0;
        }

        protected void OpenContractPool()
        {
            pendingContractPool = false;
            pendingAnomalyRewards = currentAnomaly != null;
            pendingStandardRewards = !pendingAnomalyRewards;

            type = RewardType.Mixed;
            PanelSetup();
            GenerateMixedPool();
        }

        protected void CleanupAnomaly()
        {
            if (currentAnomaly.isActive)
            {
                HandleAnomalyRewards();
            }
            else
            {
                currentAnomaly.Cleanup();
                currentAnomaly = null;
                ResumeGameLoop();
            }
        }

        protected void HandleAnomalyRewards()
        {
            currentAnomaly.CompleteAnomaly();
            currentAnomaly.Cleanup();
            currentAnomaly = null;

            pendingStandardRewards = true;

            int c = pendingAnomalyRerolls >= 0 ? pendingAnomalyRerolls : RollAnomalyRerolls();
            int sp = pendingAnomalySkillPoints >= 0 ? pendingAnomalySkillPoints : AnomalySkillPointGain();
            pendingAnomalyRerolls = -1;
            pendingAnomalySkillPoints = -1;

            rerolls += c;
            UpdateRerollUI();

            additionalQuality += Random.Range(0.1f, 0.3f) + D.anomalyQualityAdd;

            CachePlayerSkillTree();
            if (cpst != null && sp > 0) cpst.AddSkillPoints(sp);

            type = RewardType.Mixed;
            PanelSetup();
            GenerateMixedPool();
        }
        protected bool RollAndGenerateAnomaly()
        {
            if (currentAnomaly != null && currentAnomaly.isActive) return false;
            if (availableAnomalies == null || availableAnomalies.Count == 0) return false;
            if (anomalyPrefab == null || GetCurrentWave() <= anomalyGlobalMinWave) return false;

            float chance = anomalyChance + D.anomalyChanceAdd;
            if (minAnomalyCount <= 0 || maxAnomalyCount <= 0 || chance <= 0f) return false;

            float roll = Random.Range(0f, 100f);
            if (roll > chance) return false;

            return GenerateAnomalyChoices();
        }
        protected bool HasAnomalyChoices()
        {
            if (availableAnomalies == null || availableAnomalies.Count == 0) return false;
            if (anomalyPrefab == null) return false;
            if (minAnomalyCount <= 0 || maxAnomalyCount <= 0) return false;

            int w = GetCurrentWave();
            bool bossNext = NextWaveIsBoss();
            return availableAnomalies.Exists(a => IsAnomalyEligible(a, w, bossNext));
        }
        protected bool GenerateAnomalyChoices()
        {
            if (availableAnomalies == null || availableAnomalies.Count == 0) return false;
            if (anomalyPrefab == null) return false;
            if (minAnomalyCount <= 0 || maxAnomalyCount <= 0) return false;

            int w = GetCurrentWave();
            bool bossNext = NextWaveIsBoss();
            var available = availableAnomalies.FindAll(a => IsAnomalyEligible(a, w, bossNext));
            if (available.Count == 0) return false;

            type = RewardType.Anomaly;
            OpenAnomalyButtons();
            PanelSetup();

            int minChoices = Mathf.Max(1, minAnomalyCount + D.minAnomalyCountAdd);
            int maxChoices = Mathf.Max(minChoices, maxAnomalyCount + D.maxAnomalyCountAdd);
            int choices = Random.Range(minChoices, maxChoices + 1);
            string rewardLine = GetAnomalyRewardLine();

            for (int i = 0; i < choices; i++)
            {
                AnomalyData amd = available[Random.Range(0, available.Count)];
                AnomalyInstance instance = amd.CreateInstance();

                Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
                GameObject btnObj = PrefabPool.Acquire(anomalyPrefab, targetParent);
                if (btnObj == null) continue;

                activeRewardButtons.Add(btnObj);

                if (btnObj.TryGetComponent<AnomalyButtonUI>(out var anomalyButton))
                    anomalyButton.Setup(instance, OnAnomalyButtonClicked, rewardLine);
            }

            return true;
        }
        protected bool IsAnomalyEligible(AnomalyData a, int w, bool bossNext)
            => a != null && w >= a.minWave && w <= a.maxWave && a.minMode <= RunMode.Tier && !(a.disallowOnBossWave && bossNext) && !(a.bossOnly && !bossNext) && !IsContractType(a);

        protected bool IsContractType(AnomalyData a) => contractType.HasValue && a.anomalyType == contractType.Value;

        protected bool IsContractEligible(AnomalyData a) => a != null && a.isContract && !a.bossOnly && a.minMode <= RunMode.Tier;

        protected bool HasContractChoices()
        {
            if (availableAnomalies == null || contractPrefab == null) return false;
            return availableAnomalies.Exists(IsContractEligible);
        }

        public bool TryStartContract()
        {
            if (currentContract != null || !HasContractChoices()) return false;

            ActiveManager = this;
            return GenerateContractChoices();
        }

        protected bool GenerateContractChoices()
        {
            if (!HasContractChoices()) return false;

            var available = availableAnomalies.FindAll(IsContractEligible);

            type = RewardType.Contract;
            OpenContractButtons();
            PanelSetup();

            int minChoices = Mathf.Max(1, minContractCount + D.minContractCountAdd);
            int maxChoices = Mathf.Max(minChoices, maxContractCount + D.maxContractCountAdd);
            int choices = Random.Range(minChoices, maxChoices + 1);
            Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;

            for (int i = 0; i < choices; i++)
            {
                AnomalyData amd = available[Random.Range(0, available.Count)];
                AnomalyInstance instance = amd.CreateInstance();

                GameObject btnObj = PrefabPool.Acquire(contractPrefab, targetParent);
                if (btnObj == null) continue;

                activeRewardButtons.Add(btnObj);

                if (btnObj.TryGetComponent<ContractButtonUI>(out var cb))
                    cb.Setup(instance, OnContractButtonClicked, GetContractRewardLine(amd));
            }

            return true;
        }

        protected string GetContractRewardLine(AnomalyData amd)
        {
            List<string> parts = new();

            if (!IronmanSelector.Enabled && amd.contractRerolls > 0) parts.Add($"+{amd.contractRerolls} Reroll{(amd.contractRerolls > 1 ? "s" : "")}");
            if (amd.contractSkillPointChance > 0f) parts.Add($"{amd.contractSkillPointChance:0.#}% for +1 Skill Point");
            if (amd.contractMixedPoolChance > 0f) parts.Add($"{amd.contractMixedPoolChance:0.#}% for a bonus reward");

            return parts.Count > 0 ? $"Per Wave: {string.Join(", ", parts)}" : "";
        }

        protected void OnContractButtonClicked(AnomalyInstance instance)
        {
            if (instance != null && instance.amd != null)
            {
                currentContract = instance;
                contractType = instance.amd.anomalyType;
                lastContractKey = int.MinValue;
            }

            ContinueAfterContract();
        }

        protected void ContinueAfterContract()
        {
            CloseRewardUI();
            contractButtonsOpen = false;

            if (TryStartPreRunPicks()) return;

            Time.timeScale = 1f;
            StartNextWave();
        }

        protected virtual void OnAnomalyButtonClicked(AnomalyInstance instance)
        {
            CloseRewardUI();
            Time.timeScale = 1f;

            if (instance != null)
            {
                currentAnomaly = instance;
                currentAnomaly.StartAnomaly();
            }

            BeginWave();
        }
    }
}
