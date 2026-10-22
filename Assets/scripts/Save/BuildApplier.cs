using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.SkillTree;
using CrystalFlux.StatusEffectSystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.SaveSystem
{
    public static class BuildApplier
    {
        public static bool CanResolve(BuildData b, SaveAssetCatalog c)
        {
            if (b == null || c == null) return false;

            foreach (var slot in b.attacks)
                if (c.Get<AttackData>(slot.id) == null) return false;

            foreach (string id in b.upgradeIds)
                if (c.Get<PlayerUpgrade>(id) == null) return false;

            return !b.hasContract || c.Get<AnomalyData>(b.contract.anomalyId) != null;
        }

        public static bool ApplyBuild(GameObject player, BuildData b, SaveAssetCatalog c)
        {
            if (player == null || !CanResolve(b, c)) return false;

            if (player.TryGetComponent<PlayerSkillTree>(out var tree))
                tree.RestoreUnlocked(b.skillTree.unlockedOrder, b.skillTree.skillPoints, b.skillTree.choseStarting);

            if (player.TryGetComponent<PlayerAttackHandler>(out var pah))
            {
                var attacks = new List<AttackData>();
                foreach (var slot in b.attacks) attacks.Add(c.Get<AttackData>(slot.id));
                pah.SetAttacksExact(attacks);
            }

            if (player.TryGetComponent<PlayerUpgradeManager>(out var pum))
            {
                var ups = new List<PlayerUpgrade>();
                foreach (string id in b.upgradeIds) ups.Add(c.Get<PlayerUpgrade>(id));
                pum.SetUpgradesExact(ups);
            }

            var permanent = new List<StatBuff>();
            if (player.TryGetComponent<StatusEffectManager>(out var sem)) sem.CollectAppliedStats(permanent);
            if (pum != null) pum.CollectAppliedStats(permanent);

            if (player.TryGetComponent<EntityStatManager>(out var esm))
            {
                esm.RestoreStatsJson(b.statsJson);
                foreach (StatBuff s in permanent)
                    if (!StatsMath.IsFlag(s.type)) esm.AddStat(s);
            }

            if (player.TryGetComponent<StatSynergyManager>(out var ssm))
            {
                var states = new List<(StatType, StatType, float, float)>();
                foreach (var s in b.synergies) states.Add((s.source, s.target, s.pct, s.applied));
                ssm.RestoreState(states);
            }

            if (tree != null) tree.RestoreUnlocked(null, b.skillTree.skillPoints, b.skillTree.choseStarting);

            DpsMeter.SetPeak(b.peakDps);
            RunSession.Playtime = b.playtimeSeconds;
            return true;
        }

        public static AnomalyInstance RecreateContract(BuildData b, SaveAssetCatalog c)
        {
            if (b == null || !b.hasContract || c == null) return null;

            AnomalyData amd = c.Get<AnomalyData>(b.contract.anomalyId);
            return amd != null ? amd.RecreateInstance(b.contract.creationState) : null;
        }

        public static bool ApplyRun(WaveManager wm, RunData r, SaveAssetCatalog c)
        {
            if (wm == null || r == null || c == null) return false;

            var dto = new RunStateDto
            {
                sequence = c.Get<WaveSequence>(r.sequenceId),
                waveIndex = r.waveIndex,
                rerolls = r.rerolls,
                additionalQuality = r.additionalQuality,
                synergyChanceBonus = r.synergyChanceBonus,
                pendingOccasionalRerolls = r.pendingOccasionalRerolls,
                pendingOccasionalSkillPoints = r.pendingOccasionalSkillPoints,
                pendingAnomalyRerolls = r.pendingAnomalyRerolls,
                pendingAnomalySkillPoints = r.pendingAnomalySkillPoints,
                unlimitedLastBossWave = r.unlimitedLastBossWave,
                unlimitedMaxTotalEnemies = r.unlimitedMaxTotalEnemies,
                mutations = r.mutations ?? new(),
                lastMutationWave = r.lastMutationWave,
                mutationBonusChance = r.mutationBonusChance
            };

            foreach (var id in r.availableRareIds)
            {
                var a = c.Get<AttackAsset>(id);
                if (a != null) dto.availableRare.Add(a);
            }

            foreach (var id in r.availableTreasureIds)
            {
                var u = c.Get<UpgradeAsset>(id);
                if (u != null) dto.availableTreasure.Add(u);
            }

            foreach (var id in r.availableCorruptionSpecialIds)
            {
                var a = c.Get<AttackAsset>(id);
                if (a != null) dto.availableCorruptionSpecial.Add(a);
            }

            wm.RestoreRunState(dto);
            return true;
        }
    }
}
