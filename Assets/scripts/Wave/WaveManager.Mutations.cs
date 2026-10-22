using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using UnityEngine;

namespace CrystalFlux.WaveSystem
{
    public partial class WaveManager
    {
        [Header("Mutations")]
        public int mutationStartWave = 20;
        [Tooltip("% chance at wave start once eligible")] public float mutationBaseChance = 5f;
        [Tooltip("% added to the chance after each eligible wave without a mutation")] public float mutationChanceRamp = 5f;
        public int mutationMinGap = 5;
        public float mutationAnnounceDuration = 3f;
        public Color mutationAnnounceColor = new(0.7f, 0.4f, 1f);

        private readonly List<MutationState> mutations = new();
        private int lastMutationWave;
        private float mutationBonusChance;

        public IReadOnlyList<MutationState> Mutations => mutations;

        private bool FissionActive => currentAnomaly is SplitInstance s && s.isActive;

        protected void RollMutation()
        {
            int wave = GetCurrentWave();
            if (!D.mutations || wave < mutationStartWave) return;
            if (lastMutationWave > 0 && wave - lastMutationWave < mutationMinGap) return;

            if (Random.Range(0f, 100f) >= mutationBaseChance + mutationBonusChance)
            {
                mutationBonusChance += mutationChanceRamp;
                return;
            }

            MutationState m = AddMutation();
            if (m == null) return;

            lastMutationWave = wave;
            mutationBonusChance = 0f;
            AnnounceMutation(m);
        }

        private MutationState AddMutation()
        {
            var open = new List<MutationType>();
            for (int i = 0; i < MutationState.Count; i++)
            {
                MutationState s = FindMutation((MutationType)i);
                if (s == null || !s.IsMaxed) open.Add((MutationType)i);
            }
            if (open.Count == 0) return null;

            MutationType pick = open[Random.Range(0, open.Count)];
            MutationState m = FindMutation(pick);
            if (m == null)
            {
                m = new MutationState { type = pick };
                mutations.Add(m);
            }
            m.LevelUp();
            return m;
        }

        private MutationState FindMutation(MutationType t) => mutations.Find(m => m.type == t);

        private void AnnounceMutation(MutationState added)
        {
            if (GameController == null) return;

            string hex = ColorUtility.ToHtmlStringRGB(mutationAnnounceColor);
            GameController.SetTitleForDuration($"<color=#{hex}>Mutation: {added.Label}</color>", mutationAnnounceDuration, 0.5f, 0.5f);

            var active = new List<string>();
            foreach (var m in mutations)
                if (m != added) active.Add(m.Label);
            if (active.Count > 0) GameController.SetSubtitleForDuration($"<color=#{hex}>{string.Join(", ", active)}</color>", mutationAnnounceDuration, 0.5f, 0.5f);
        }

        protected void ApplyMutations(GameObject enemy, GameObject prefab, int level, IStatProvider esm, int splitGeneration = 0)
        {
            if (mutations.Count == 0 || enemy == null) return;

            EnemyMutationEffects fx = null;
            EnemyMutationEffects Fx() => fx != null ? fx : fx = enemy.AddComponent<EnemyMutationEffects>();

            foreach (var m in mutations)
            {
                if (m.Stat is StatType stat)
                {
                    esm?.AddStat(new StatBuff(stat, m.value));
                    continue;
                }

                switch (m.type)
                {
                    case MutationType.Lifesteal: Fx().lifestealPct = m.value; break;
                    case MutationType.Shields: Fx().shieldCapPct = m.value; break;
                    case MutationType.IFrames: Fx().iFrameDuration = m.value; break;
                    case MutationType.Thorns:
                        Fx().thornsPct = m.value;
                        Fx().thornsType = m.IsMaxed ? DamageType.True : DamageType.Physical;
                        break;
                    case MutationType.Overhealth:
                        if (enemy.TryGetComponent<EntityHealth>(out var eh)) eh.SetOverhealth(m.value2, m.value, EntityHealth.DefaultOverhealthDecayInterval, true);
                        break;
                    case MutationType.Dodge:
                        esm?.AddStat(new StatBuff(StatType.dodgeChance, m.value));
                        esm?.AddStat(new StatBuff(StatType.dodgeRating, m.value2));
                        break;
                    case MutationType.Splitting:
                        AddMutationSplitter(enemy, prefab, level, m, splitGeneration);
                        break;
                }
            }
        }

        private void AddMutationSplitter(GameObject enemy, GameObject prefab, int level, MutationState m, int generation)
        {
            // Fission lets mutation clones split one more time; capped so lv3 splitting can't grow forever.
            if (generation > 1 || (generation == 1 && !FissionActive)) return;

            int i = m.level - 1;
            AnomalySplitter splitter = enemy.AddComponent<AnomalySplitter>();
            splitter.Setup(prefab, level, m.value, MutationState.SplitCount[i, 0], MutationState.SplitCount[i, 1], 0);
            splitter.onSplit = clone => ApplyMutations(clone, prefab, level, clone.TryGetComponent<IStatProvider>(out var s) ? s : null, generation + 1);
        }
    }
}
