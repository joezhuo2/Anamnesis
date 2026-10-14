using System;
using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.WaveSystem
{
    public partial class WaveManager
    {
        public static event Action<WaveManager> WaveStarting;
        public static Func<WaveManager, bool> PendingLoadStarter;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSaveStatics()
        {
            WaveStarting = null;
            PendingLoadStarter = null;
        }

        public static WaveManager Active => ActiveManager;

        public bool IsRunStarted { get; private set; }
        public bool IsWaveActive => isWaveActive;
        public bool IsIdleBetweenWaves => IsRunStarted && !isWaveActive && !anomalyButtonsOpen && !contractButtonsOpen
            && activeRewardButtons.Count == 0 && !pendingSynergy && !pendingStandardRewards && !pendingAnomalyRewards;
        public int CurrentWaveNumber => currentSequence != null ? GetCurrentWave() : currentWaveIndex;
        public int WaveIndex => currentWaveIndex;
        public AnomalyInstance CurrentContract => currentContract;

        protected void NotifyWaveStarting()
        {
            IsRunStarted = true;
            WaveStarting?.Invoke(this);
        }

        public virtual void CaptureRunState(RunStateDto dto)
        {
            dto.sequence = currentSequence;
            dto.waveIndex = currentWaveIndex;
            dto.rerolls = rerolls;
            dto.additionalQuality = additionalQuality;
            dto.synergyChanceBonus = synergyChanceBonus;

            dto.availableRare.Clear();
            foreach (var r in availableRarePool)
                if (r?.newAttack != null) dto.availableRare.Add(r.newAttack);

            dto.availableTreasure.Clear();
            foreach (var t in availableTreasurePool)
                if (t?.upgrade != null) dto.availableTreasure.Add(t.upgrade);

            dto.availableCorruptionSpecial.Clear();
            foreach (var r in availableCorruptionSpecialPool)
                if (r?.newAttack != null) dto.availableCorruptionSpecial.Add(r.newAttack);

            dto.pendingOccasionalRerolls = pendingOccasionalRerolls;
            dto.pendingOccasionalSkillPoints = pendingOccasionalSkillPoints;
            dto.pendingAnomalyRerolls = pendingAnomalyRerolls;
            dto.pendingAnomalySkillPoints = pendingAnomalySkillPoints;
        }

        public virtual void RestoreRunState(RunStateDto dto)
        {
            if (dto == null) return;

            if (dto.sequence != null) currentSequence = dto.sequence;
            currentWaveIndex = Mathf.Max(0, dto.waveIndex);
            rerolls = Mathf.Max(0, dto.rerolls);
            additionalQuality = dto.additionalQuality;
            synergyChanceBonus = dto.synergyChanceBonus;

            RebuildPool(availableRarePool, rarePool, dto.availableRare);
            RebuildPool(availableCorruptionSpecialPool, corruptionSpecialPool, dto.availableCorruptionSpecial);

            availableTreasurePool.Clear();
            if (treasurePool != null)
                foreach (var t in treasurePool)
                    if (t?.upgrade != null && dto.availableTreasure.Contains(t.upgrade)) availableTreasurePool.Add(t);

            pendingOccasionalRerolls = dto.pendingOccasionalRerolls;
            pendingOccasionalSkillPoints = dto.pendingOccasionalSkillPoints;
            pendingAnomalyRerolls = dto.pendingAnomalyRerolls;
            pendingAnomalySkillPoints = dto.pendingAnomalySkillPoints;

            UpdateRerollUI();
        }

        private static void RebuildPool(List<AttackReward> available, List<AttackReward> source, List<AttackAsset> keep)
        {
            available.Clear();
            if (source == null) return;

            foreach (var r in source)
                if (r?.newAttack != null && keep.Contains(r.newAttack)) available.Add(r);
        }

        public void RemoveOwnedFromPools(IAttackHandler pah, IUpgradeHolder pum)
        {
            if (pah != null)
            {
                availableRarePool.RemoveAll(r => r?.newAttack != null && pah.HasAttack(r.newAttack));
                availableCorruptionSpecialPool.RemoveAll(r => r?.newAttack != null && pah.HasAttack(r.newAttack));
            }

            if (pum != null)
                availableTreasurePool.RemoveAll(t => t?.upgrade != null && pum.HasUpgrade(t.upgrade));
        }

        public void RestoreContract(AnomalyInstance instance)
        {
            if (instance == null || instance.amd == null) return;

            currentContract = instance;
            contractType = instance.amd.anomalyType;
            lastContractKey = int.MinValue;
        }

        public void StartLoadedRun()
        {
            CloseAllButtons();
            IAnnouncer.Current?.DisableSubtitle();
            IAnnouncer.Current?.DisableTitle();
            StartNextWave();
        }
    }
}
