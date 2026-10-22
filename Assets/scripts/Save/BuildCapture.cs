using System;
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
    public static class BuildCapture
    {
        public static BuildData Capture(GameObject player, SaveAssetCatalog c, AnomalyInstance contract)
        {
            var b = new BuildData();
            if (player == null || c == null) return b;

            var effectStats = new List<StatBuff>();
            if (player.TryGetComponent<StatusEffectManager>(out var sem)) sem.CollectAppliedStats(effectStats);
            if (player.TryGetComponent<PlayerUpgradeManager>(out var upgrades)) upgrades.CollectAppliedStats(effectStats);
            if (player.TryGetComponent<EntityStatManager>(out var esm)) b.statsJson = StatsMath.CleanStatsJson(esm.Stats, effectStats);

            if (player.TryGetComponent<PlayerAttackHandler>(out var pah))
                foreach (AttackData a in pah.attacks)
                    if (a != null) b.attacks.Add(new AttackSlot { type = a.type, id = c.GetId(a) });

            if (player.TryGetComponent<PlayerUpgradeManager>(out var pum))
                foreach (PlayerUpgrade u in pum.activeUpgrades)
                    if (u != null) b.upgradeIds.Add(c.GetId(u));

            if (player.TryGetComponent<PlayerSkillTree>(out var tree))
            {
                b.skillTree.unlockedOrder.AddRange(tree.UnlockOrder);
                b.skillTree.skillPoints = tree.SkillPoints;
                b.skillTree.choseStarting = tree.choseStarting;
            }

            if (player.TryGetComponent<StatSynergyManager>(out var ssm))
            {
                var states = new List<(StatType source, StatType target, float pct, float applied)>();
                ssm.GetState(states);
                foreach (var s in states)
                    b.synergies.Add(new SynergyState { source = s.source, target = s.target, pct = s.pct, applied = s.applied });
            }

            if (contract != null && contract.amd != null)
            {
                b.hasContract = true;
                b.contract.anomalyId = c.GetId(contract.amd);
                b.contract.creationState = contract.creationState;
            }

            b.peakDps = DpsMeter.Peak;
            b.playtimeSeconds = RunSession.Playtime;
            return b;
        }

        public static RunData CaptureRun(WaveManager wm, SaveAssetCatalog c)
        {
            var r = new RunData();
            if (wm == null || c == null) return r;

            var dto = new RunStateDto();
            wm.CaptureRunState(dto);

            r.waveManagerKind = KindOf(wm);
            r.sequenceId = dto.sequence == null ? "" : OrMissing(c.GetId(dto.sequence));
            r.waveIndex = dto.waveIndex;
            r.difficultyId = c.GetId(wm.difficulty);
            r.modeId = c.GetId(wm.mode);
            r.ironman = IronmanSelector.Enabled;
            r.rerolls = dto.rerolls;
            r.additionalQuality = dto.additionalQuality;
            r.synergyChanceBonus = dto.synergyChanceBonus;
            foreach (var a in dto.availableRare) r.availableRareIds.Add(c.GetId(a));
            foreach (var u in dto.availableTreasure) r.availableTreasureIds.Add(c.GetId(u));
            foreach (var a in dto.availableCorruptionSpecial) r.availableCorruptionSpecialIds.Add(c.GetId(a));
            r.pendingOccasionalRerolls = dto.pendingOccasionalRerolls;
            r.pendingOccasionalSkillPoints = dto.pendingOccasionalSkillPoints;
            r.pendingAnomalyRerolls = dto.pendingAnomalyRerolls;
            r.pendingAnomalySkillPoints = dto.pendingAnomalySkillPoints;
            r.unlimitedLastBossWave = dto.unlimitedLastBossWave;
            r.unlimitedMaxTotalEnemies = dto.unlimitedMaxTotalEnemies;
            r.mutations = dto.mutations;
            r.lastMutationWave = dto.lastMutationWave;
            r.mutationBonusChance = dto.mutationBonusChance;
            return r;
        }

        private static string OrMissing(string id) => string.IsNullOrEmpty(id) ? "?" : id;

        public static WaveManagerKind KindOf(WaveManager wm) => wm is UnlimitedWaveManager ? WaveManagerKind.Unlimited : WaveManagerKind.Regular;

        public static SaveMeta MakeMeta(SaveKind kind, string name, BuildData b, RunData r, WaveManager wm, int wave, List<string> attackNames)
        {
            b ??= new BuildData();

            var m = new SaveMeta
            {
                kind = kind,
                id = Guid.NewGuid().ToString("N"),
                name = SaveStore.NormalizeName(name),
                savedAtUtc = DateTime.UtcNow.ToString("o"),
                gameVersion = Application.version,
                wave = wave,
                waveManagerKind = wm != null ? KindOf(wm) : WaveManagerKind.Regular,
                difficultyName = wm != null && wm.difficulty != null ? wm.difficulty.displayName : "",
                modeName = wm != null && wm.mode != null ? wm.mode.displayName : "",
                ironman = IronmanSelector.Enabled,
                readOnly = kind == SaveKind.Build && IronmanSelector.Enabled,
                peakDps = b.peakDps,
                playtimeSeconds = b.playtimeSeconds,
                attackNames = attackNames ?? new List<string>()
            };

            void Ref(string id)
            {
                id ??= "";
                if (!m.referencedIds.Contains(id)) m.referencedIds.Add(id);
            }

            foreach (var a in b.attacks) Ref(a.id);
            foreach (var u in b.upgradeIds) Ref(u);
            if (b.hasContract) Ref(b.contract.anomalyId);

            if (kind == SaveKind.Run && r != null)
            {
                if (!string.IsNullOrEmpty(r.sequenceId)) Ref(r.sequenceId);
                Ref(r.difficultyId);
                Ref(r.modeId);
                foreach (var id in r.availableRareIds) Ref(id);
                foreach (var id in r.availableTreasureIds) Ref(id);
                foreach (var id in r.availableCorruptionSpecialIds) Ref(id);
            }

            return m;
        }

        public static List<string> AttackNames(GameObject player)
        {
            var names = new List<string>();
            if (player == null || !player.TryGetComponent<PlayerAttackHandler>(out var pah)) return names;

            foreach (AttackType t in Enum.GetValues(typeof(AttackType)))
            {
                AttackData a = pah.FindAttackOfType(t);
                if (a != null) names.Add(PlayerAttackHandler.NormalizeAttackName(a.name));
            }

            return names;
        }
    }
}
