using UnityEngine;

namespace CrystalFlux.Core
{
    public class EnemyStatManager : EntityStatManager
    {
        protected override void Awake()
        {
            base.Awake();

            if (s != null && s.level > 1) ScaleBaseStats(s.level);
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

            const float atkGrowth = 1.05f;
            const float hpGrowth = 1.1f;
            const float armorGrowth = 1.05f;
            const float utilityGrowth = 1.04f;

            float atkMult = Mathf.Pow(atkGrowth, levelOffset);
            float hpMult = Mathf.Pow(hpGrowth, levelOffset);
            float armorMult = Mathf.Pow(armorGrowth, levelOffset);
            float utilMult = Mathf.Pow(utilityGrowth, levelOffset);

            s.attack = Mathf.RoundToInt(s.attack * atkMult);
            s.critDamage *= atkMult;
            s.maxHp = Mathf.RoundToInt(s.maxHp * hpMult);
            s.hpRegen *= hpMult;
            s.armor = Mathf.RoundToInt(s.armor * armorMult);

            s.aoePct *= utilMult;
            s.moveSpeedPct = Mathf.Clamp(s.moveSpeedPct * utilMult, -100f, 100f);
            s.critChance = Mathf.Clamp(s.critChance * utilMult, 0f, 100f);
        }
    }
}
