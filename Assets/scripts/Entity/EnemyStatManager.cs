using CrystalFlux.EntitySystem;
using UnityEngine;

namespace CrystalFlux.Core
{
    public class EnemyStatManager : EntityStatManager
    {
        public string displayName;

        protected override void Awake()
        {
            base.Awake();

            if (s != null && s.level > 1) ScaleBaseStats(s.level);
        }

        public override float GetStat(StatType type)
        {
            float v = base.GetStat(type);
            if (s == null || teamID != EnemyGlobalBuffs.EnemyTeam) return v;

            return type switch
            {
                StatType.damagePct => v + EnemyGlobalBuffs.DamagePct,
                StatType.moveSpeedPct => v + EnemyGlobalBuffs.MoveSpeedPct,
                StatType.EffSpd => v + (s.moveSpeed * EnemyGlobalBuffs.MoveSpeedPct * 0.01f),
                _ => v
            };
        }

        public void ScaleStatsToLevel(int targetLevel)
        {
            if (s == null) return;

            s.level = targetLevel;

            if (s.level > 1) ScaleBaseStats(s.level);
        }
        private void ScaleBaseStats(int currentLevel)
        {
            int levelOffset = currentLevel - 1;
            if (levelOffset <= 0) return;

            const float atkGrowth = 1.04f;
            const float hpGrowth = 1.1f;
            const float armorGrowth = 1.07f;
            const float utilityGrowth = 1.04f;

            float atkMult = Mathf.Pow(atkGrowth, levelOffset);
            float hpMult = Mathf.Pow(hpGrowth, levelOffset);
            float hprMult = Mathf.Pow(hpGrowth, levelOffset * 0.3f);
            float armorMult = Mathf.Pow(armorGrowth, levelOffset);
            float utilMult = Mathf.Pow(utilityGrowth, levelOffset);

            s.attack = Scale(s.attack, atkMult);
            s.intelligence = Scale(s.intelligence, atkMult);
            s.critDamage = Cap(s.critDamage * atkMult);
            s.maxHp = Scale(s.maxHp, hpMult);
            s.hpRegen = Cap(s.hpRegen * hprMult);
            s.armor = Scale(s.armor, armorMult);
            s.arcaneShield = Scale(s.arcaneShield, armorMult);
            s.dodgeRating = Cap(s.dodgeRating * armorMult);

            s.aoePct = Cap(s.aoePct * utilMult);
            s.moveSpeedPct = Mathf.Clamp(s.moveSpeedPct * utilMult, -100f, 100f);
            s.critChance = Mathf.Clamp(s.critChance * utilMult, 0f, 100f);
        }

        private static float Cap(float v) => float.IsNaN(v) ? 0f : Mathf.Clamp(v, -EntityStats.MaxStatValue, EntityStats.MaxStatValue);

        private static float Scale(float v, float mult) => Mathf.Round(Cap(v * mult));
    }
}
