using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.ItemSystem
{
    public class EntityGearManager : MonoBehaviour
    {
        public List<GearItem> available = new();
        public Dictionary<EquipmentSlot, GearItem> equipped = new();

        private static readonly StatType[] RollableStats =
        {
            StatType.attack, StatType.atkPct, StatType.damagePct,
            StatType.physicalDmgPct, StatType.spellDmgPct,
            StatType.critChance, StatType.critDamage, StatType.aoePct,
            StatType.maxHp, StatType.hpPct, StatType.hpRegen, StatType.hpRegPct,
            StatType.armor, StatType.armorPct,
            StatType.damageRes, StatType.physicalRes, StatType.spellRes,
            StatType.dodgeChance, StatType.dodgeResPct,
            StatType.moveSpeed, StatType.moveSpeedPct, StatType.attackSpeedPct,
            StatType.defShred, StatType.resPen,
            StatType.maxStamina, StatType.maxStaminaPct, StatType.staminaRegen, StatType.stRegPct,
            StatType.maxMana, StatType.maxManaPct, StatType.manaGainPct,
            StatType.Intelligence, StatType.IntPct,
            StatType.addPhysDmgPct, StatType.addSplDmgPct, StatType.addTrueDmgPct, StatType.addDmgPct,
            StatType.BasicDmgPct, StatType.SkillDmgPct, StatType.UltDmgPct,
            StatType.basicCdRedPct, StatType.skillCdRedPct, StatType.ultCdRedPct,
            StatType.EffectRes, StatType.kbRes, StatType.kbPct,
            StatType.sePotPct, StatType.seDurPct, StatType.seTickRatePct,
            StatType.ProjSpd, StatType.stCostPct, StatType.ExpBonus, StatType.Stealing,
            StatType.dashCooldownRedPct, StatType.dashDistancePct, StatType.dashStaminaCostRedPct
        };

        public void IdentifyGear(GearItem item)
        {
            available ??= new List<GearItem>();
            if (item == null || available.Contains(item)) return;

            GearItem i = Instantiate(item);
            i.rolls = new();

            i.baseRoll = ProcessRoll(i.potentialBaseRoll);

            foreach (StatRoll r in i.potentialRolls)
            {
                StatBuff s = ProcessRoll(r);
                i.rolls.Add(s);
            }

            available.Add(i);
        }
        private StatBuff ProcessRoll(StatRoll r)
        {
            StatType type;
            float roll;

            if (r.rollType == StatRollType.PureRandomStatAndRoll)
            {
                type = RollableStats[UnityEngine.Random.Range(0, RollableStats.Length)];
                roll = UnityEngine.Random.Range(r.minRoll, r.maxRoll);
            }
            else if (r.rollType == StatRollType.GuaranteedStatRandomRoll)
            {
                type = r.statType;
                roll = UnityEngine.Random.Range(r.minRoll, r.maxRoll);
            }
            else
            {
                type = r.statType;
                roll = r.minRoll;
            }

            return new StatBuff(type, roll);
        }
        public void EquipGear(EquipmentSlot slot) {}
        public void RemoveGear(EquipmentSlot slot) {}
    }
}
