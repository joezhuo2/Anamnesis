using System;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.WaveSystem
{
    public enum MutationType
    {
        Lifesteal,
        Overhealth,
        DamageRes,
        EffectRes,
        SpellRes,
        PhysicalRes,
        AttackSpeed,
        Shields,
        Thorns,
        IFrames,
        Splitting,
        Dodge
    }

    [Serializable]
    public class MutationState
    {
        public const int MaxLevel = 3;

        public MutationType type;
        public int level;
        public float value;
        public float value2;

        // Per level: (min, max) of value, then of value2.
        private static readonly float[,] lifesteal = { { 8, 16 }, { 12, 24 }, { 25, 48 } };
        private static readonly float[,] overhealthDecay = { { 15, 25 }, { 12, 18 }, { 6, 12 } };
        private static readonly float[,] overhealthConv = { { 80, 120 }, { 120, 180 }, { 200, 300 } };
        private static readonly float[,] stat = { { 10, 30 }, { 25, 40 }, { 45, 70 } };
        private static readonly float[,] shields = { { 35, 70 }, { 20, 60 }, { 15, 45 } };
        private static readonly float[,] thorns = { { 2, 6 }, { 4, 14 }, { 2, 4 } };
        private static readonly float[,] iFrames = { { 0.1f, 0.3f }, { 0.15f, 0.4f }, { 0.3f, 0.7f } };
        private static readonly float[,] splitChance = { { 15, 25 }, { 25, 45 }, { 35, 60 } };
        private static readonly float[,] dodgeChance = { { 25, 35 }, { 30, 40 }, { 35, 50 } };
        private static readonly float[,] dodgeRating = { { 80, 120 }, { 150, 200 }, { 180, 220 } };
        public static readonly int[,] SplitCount = { { 2, 3 }, { 2, 5 }, { 3, 7 } };

        public static int Count => Enum.GetValues(typeof(MutationType)).Length;

        public bool IsMaxed => level >= MaxLevel;

        public void LevelUp()
        {
            level = Mathf.Min(level + 1, MaxLevel);
            int i = level - 1;
            switch (type)
            {
                case MutationType.Lifesteal: value = Roll(lifesteal, i); break;
                case MutationType.Overhealth: value = Roll(overhealthDecay, i); value2 = Roll(overhealthConv, i); break;
                case MutationType.Shields: value = Roll(shields, i); break;
                case MutationType.Thorns: value = Roll(thorns, i); break;
                case MutationType.IFrames: value = Roll(iFrames, i, 0.01f); break;
                case MutationType.Splitting: value = Roll(splitChance, i); break;
                case MutationType.Dodge: value = Roll(dodgeChance, i); value2 = Roll(dodgeRating, i); break;
                default: value = Roll(stat, i); break;
            }
        }

        private static float Roll(float[,] t, int i, float step = 1f) => Mathf.Round(UnityEngine.Random.Range(t[i, 0], t[i, 1]) / step) * step;

        public StatType? Stat => type switch
        {
            MutationType.DamageRes => StatType.damageRes,
            MutationType.EffectRes => StatType.EffectRes,
            MutationType.SpellRes => StatType.spellRes,
            MutationType.PhysicalRes => StatType.physicalRes,
            MutationType.AttackSpeed => StatType.attackSpeedPct,
            _ => null
        };

        public string Name => type switch
        {
            MutationType.DamageRes => "Damage Resistance",
            MutationType.EffectRes => "Effect Resistance",
            MutationType.SpellRes => "Spell Resistance",
            MutationType.PhysicalRes => "Physical Resistance",
            MutationType.AttackSpeed => "Attack Speed",
            MutationType.IFrames => "Evasive",
            _ => type.ToString()
        };

        public string Label => $"{Name} {new string('I', level)}";
    }
}
