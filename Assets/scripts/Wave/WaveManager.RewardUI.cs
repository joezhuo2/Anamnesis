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
        private void SetupActionButtonTooltips()
        {
            if (!IronmanSelector.Enabled)
            {
                if (rerollButton != null && rerollButton.TryGetComponent<ITooltipDisplay>(out var td))
                    td.ShowTooltip("Reroll", $"Rerolls all unlocked reward choices.\nCost: 1 reroll token or {RerollGoldCost} gold if none are available.");
                if (CorruptionAllowed && corruptButton != null && corruptButton.TryGetComponent<ITooltipDisplay>(out var td2))
                    td2.ShowTooltip("Corrupt", "Chance to corrupt any rewards massively increase or decrease their values.\nCan only be used once per wave and removes all other options.");
            }
            if (nextWaveButton != null && nextWaveButton.TryGetComponent<ITooltipDisplay>(out var td3))
                td3.ShowTooltip("Skip", "Skip rewards and start the next wave immediately.");
        }

        protected GameObject GetOrCreateRewardButton()
        {
            Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
            GameObject btnObj = PrefabPool.Acquire(rewardButtonPrefab, targetParent);

            if (btnObj != null) activeRewardButtons.Add(btnObj);
            RefreshLockButtons();
            return btnObj;
        }

        protected RewardButton GetLockedReward()
        {
            for (int i = 0; i < activeRewardButtons.Count; i++)
            {
                GameObject go = activeRewardButtons[i];
                if (go != null && go.TryGetComponent<RewardButton>(out var rb) && rb.IsLocked) return rb;
            }
            return null;
        }

        protected void RefreshLockButtons()
        {
            bool show = !IronmanSelector.Enabled && type != RewardType.Anomaly && type != RewardType.Contract && activeRewardButtons.Count > 1;
            RewardButton lk = show ? GetLockedReward() : null;

            for (int i = 0; i < activeRewardButtons.Count; i++)
            {
                GameObject go = activeRewardButtons[i];
                if (go == null || !go.TryGetComponent<RewardButton>(out var rb)) continue;

                rb.SetupLock(show, OnRewardLockToggled);
                rb.SetLockInteractable(lk == null || lk == rb);
            }
        }

        protected void HideLockButtons()
        {
            for (int i = 0; i < activeRewardButtons.Count; i++)
            {
                GameObject go = activeRewardButtons[i];
                if (go != null && go.TryGetComponent<RewardButton>(out var rb)) rb.SetupLock(false, null);
            }
        }

        protected void OnRewardLockToggled(RewardButton rb)
        {
            if (IronmanSelector.Enabled || rb == null) return;

            RewardButton lk = GetLockedReward();
            if (lk != null && lk != rb) return;

            rb.SetLocked(!rb.IsLocked);
            RefreshLockButtons();
        }

        protected int PoolPreSetup()
        {
            int rewardChoices = 0;
            if (!gameObject.TryGetComponent<UnlimitedWaveManager>(out var uwm) && currentSequence != null && currentSequence.waves != null && currentWaveIndex > 0)
            {
                WaveData completedWave = currentSequence.waves[currentWaveIndex - 1];
                rewardChoices = Random.Range(completedWave.minRewardChoices, completedWave.maxRewardChoices + 1);
            }
            else if (uwm != null)
            {
                rewardChoices = Random.Range(uwm.minRewardChoices, uwm.maxRewardChoices + 1);
            }
            else
            {
                rewardChoices = 1;
            }

            rewardChoices = Mathf.Max(1, rewardChoices + D.rewardChoicesAdd) - LockedSlots;

            PanelSetup();

            return rewardChoices;
        }

        protected void PanelSetup()
        {
            if (rewardPanel != null) rewardPanel.SetActive(true);
            ClearRewardButtons();

            UpdateRewardTitle();
            UpdateCorruptButton();

            Time.timeScale = 0f;
        }

        protected void UpdateRewardTitle()
        {
            if (rewardTitleText == null || rewardTitleWrapper == null) return;
            if (rewardTitleWrapper != null) rewardTitleWrapper.SetActive(true);
            rewardTitleText.text = type switch
            {
                RewardType.Anomaly => anomalyTitle,
                RewardType.Contract => contractTitle,
                RewardType.Synergy => synergyTitle,
                _ => rewardTitle
            };
        }

        private void UpdateCorruptButton()
        {
            if (corruptButton == null) return;

            bool allowed = CorruptionAllowed && type != RewardType.Anomaly && type != RewardType.Milestone && type != RewardType.PreRun && type != RewardType.Synergy && type != RewardType.Contract && GetCurrentWave() % 5 != 0;
            corruptButton.gameObject.SetActive(allowed);
        }

        public static bool GrantRerolls(int amount)
        {
            if (amount <= 0 || ActiveManager == null || IronmanSelector.Enabled) return false;

            ActiveManager.rerolls += amount;
            ActiveManager.UpdateRerollUI();
            return true;
        }

        protected void UpdateRerollUI()
        {
            if (rerollText != null) rerollText.gameObject.SetActive(!RerollLocked);

            if (RerollLocked)
            {
                if (rerollButton != null) rerollButton.gameObject.SetActive(false);
                return;
            }

            CachePlayerStatManager();

            var canGoldReroll = cich != null && cich.CurrentAmount >= RerollGoldCost;

            if (rerollText != null)
            {
                if (rerolls > 0)
                {
                    rerollText.text = rerolls.ToString();
                }
                else
                {
                    if (canGoldReroll) rerollText.text = $"{RerollGoldCost}g";
                    else rerollText.text = "0";
                }
            }

            if (rerollButton != null) rerollButton.interactable = rerolls > 0 || canGoldReroll;
        }

        protected void OnSkipButtonClicked()
        {
            if (ActiveManager != null && ActiveManager != this)
            {
                ActiveManager.OnSkipButtonClicked();
                return;
            }

            if (SkipLocked) return;

            if (type == RewardType.Contract)
            {
                ContinueAfterContract();
                return;
            }

            if (type == RewardType.Anomaly)
            {
                CloseRewardUI();
                Time.timeScale = 1f;
                if (currentAnomaly != null) currentAnomaly.Cleanup();
                currentAnomaly = null;
                BeginWave();
                return;
            }

            CloseRewardUI();
            ResumeGameLoop();
        }

        protected void OnRerollButtonClicked()
        {
            if (ActiveManager != null && ActiveManager != this)
            {
                ActiveManager.OnRerollButtonClicked();
                return;
            }

            if (RerollLocked) return;

            CachePlayerStatManager();

            if (type == RewardType.Anomaly && !HasAnomalyChoices()) return;
            if (type == RewardType.Contract && !HasContractChoices()) return;
            if (type == RewardType.PreRun && !HasPreRunChoices()) return;
            if (type == RewardType.Synergy && !HasSynergyChoices()) return;

            if (rerolls > 0)
                rerolls--;
            else if (cich == null || !cich.TrySpend(RerollGoldCost)) return;

            UpdateRerollUI();

            keptReward = GetLockedReward();
            int ki = keptReward != null ? activeRewardButtons.IndexOf(keptReward.gameObject) : -1;
            if (ki >= 0) activeRewardButtons.RemoveAt(ki);
            else keptReward = null;

            ClearRewardButtons();

            switch (type)
            {
                case RewardType.Anomaly: GenerateAnomalyChoices(); break;
                case RewardType.Contract: GenerateContractChoices(); break;
                case RewardType.PreRun: GeneratePreRunPicks(); break;
                case RewardType.Basic: GenerateRewards(); break;
                case RewardType.Rare: GenerateRarePool(); break;
                case RewardType.Treasure: GenerateTreasurePool(); break;
                case RewardType.Mixed: GenerateMixedPool(); break;
                case RewardType.Milestone: GenerateMilestoneRewards(); break;
                case RewardType.Synergy: GenerateSynergyRewards(); break;
                default: break;
            }

            RestoreKeptReward(ki);
        }

        protected void RestoreKeptReward(int ki)
        {
            if (keptReward == null) return;

            RewardButton kr = keptReward;
            keptReward = null;
            kr.SetLocked(false);

            int idx = Mathf.Clamp(ki, 0, activeRewardButtons.Count);
            if (idx < activeRewardButtons.Count && activeRewardButtons[idx] != null)
                kr.transform.SetSiblingIndex(activeRewardButtons[idx].transform.GetSiblingIndex());
            else
                kr.transform.SetAsLastSibling();

            activeRewardButtons.Insert(idx, kr.gameObject);
            RefreshLockButtons();
        }

        public void OnCorruptButtonClicked()
        {
            if (!CorruptionAllowed) return;

            if (ActiveManager != null && ActiveManager != this)
            {
                ActiveManager.OnCorruptButtonClicked();
                return;
            }

            CachePlayerStatManager();
            if (cpsm == null) return;

            corruptionSpecialsThisRoll.Clear();

            float cChance = corruptChance + D.corruptChanceAdd;
            float cPosChance = corruptPositiveChance + D.corruptPositiveChanceAdd;
            float cMaxBoost = Mathf.Max(2f, maxCorruptBoost + D.maxCorruptBoostAdd);

            foreach (GameObject rb in activeRewardButtons)
            {
                if (!rb.TryGetComponent<RewardButton>(out var grb) || grb.IsLocked) continue;

                if (Random.value > (cChance * 0.01f)) continue;

                GeneratedReward gr = grb.gr;
                if (gr == null) continue;

                if (M.allowCorruptionSpecials && Random.value < (corruptionSpecialChance * 0.01f))
                {
                    AttackReward special = PickCorruptionSpecialReward();
                    if (special != null)
                    {
                        corruptionSpecialsThisRoll.Add(special);
                        grb.Setup(special, OnAttackRewardClaimed, true);
                        continue;
                    }
                }

                float corruptMult = (Random.value < (cPosChance * 0.01f) ? 1f : -1f) * (1f + (Random.Range(1, cMaxBoost) * 0.01f));

                gr.mult = corruptMult;

                string changeLine = BuildChangeLine(gr.br.baseBuff.type, gr.finalVal);

                grb.CorruptButton(changeLine, corruptMult);
            }

            HideLockButtons();

            if (rerollButton != null) rerollButton.gameObject.SetActive(false);
            if (corruptButton != null) corruptButton.gameObject.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);
        }

        protected void CloseRewardUI()
        {
            ClearRewardButtons();
            if (rewardPanel != null) rewardPanel.SetActive(false);
            if (rewardTitleWrapper != null) rewardTitleWrapper.SetActive(false);
        }

        protected void CloseRewardButtons()
        {
            ClearRewardButtons();

            if (rerollButton != null) rerollButton.gameObject.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);
            if (corruptButton != null) corruptButton.gameObject.SetActive(false);
        }

        public void OpenAnomalyButtons() => OpenActionButtons(true);

        public void OpenRewardButtons() => OpenActionButtons(false);

        public void OpenContractButtons() => OpenActionButtons(false, true);

        protected void OpenActionButtons(bool anomaly, bool contract = false)
        {
            anomalyButtonsOpen = anomaly;
            contractButtonsOpen = contract;
            if (rerollButton != null) rerollButton.gameObject.SetActive(!RerollLocked);
            if (skipButton != null) skipButton.gameObject.SetActive(!SkipLocked);
            UpdateRerollUI();
        }

        protected void ResumeGameLoop()
        {
            if (pendingAnomalyRewards)
            {
                pendingAnomalyRewards = false;
                CleanupAnomaly();
            }
            else if (pendingStandardRewards)
            {
                TriggerStandardRewards(GetCurrentWave());
            }
            else if (pendingSynergy)
            {
                pendingSynergy = false;
                OpenRewardButtons();
                UpdateRerollUI();
                GenerateSynergyRewards();
            }
            else
            {
                Time.timeScale = 1f;
                pendingStandardRewards = false;
                StartNextWave();
            }
        }
        protected void ClearRewardButtons()
        {
            for (int i = 0; i < activeRewardButtons.Count; i++)
            {
                GameObject btn = activeRewardButtons[i];
                if (btn == null) continue;

                if (btn.TryGetComponent<RewardButton>(out var rewardButton)) rewardButton.ResetForPooling();
                else if (btn.TryGetComponent<AnomalyButtonUI>(out var anomalyButton)) anomalyButton.ResetForPooling();
                else if (btn.TryGetComponent<ContractButtonUI>(out var contractButton)) contractButton.ResetForPooling();
                else if (btn.TryGetComponent<Button>(out var button)) button.onClick.RemoveAllListeners();

                PrefabPool.Release(ref btn);
            }
            activeRewardButtons.Clear();
        }
        public void CloseAllButtons()
        {
            ClearRewardButtons();

            foreach (var b in actionButtonContainer.GetComponentsInChildren<Button>())
                b.gameObject.SetActive(false);
        }
    }
}
