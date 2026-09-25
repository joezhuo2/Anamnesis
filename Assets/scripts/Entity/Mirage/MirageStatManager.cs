using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.Core
{
    [System.Serializable]
    public struct StatShare
    {
        public StatType type;
        [Tooltip("Fraction of the base entity's stat this clone receives (1 = 100%)")] public float share;
    }

    public class MirageStatManager : EntityStatManager
    {
        private static readonly HashSet<StatType> OwnStats = new()
        {
            StatType.currentHp, StatType.overhealth, StatType.isAlive, StatType.isImmune,
            StatType.CanGainHp, StatType.CanMove, StatType.CanAttack, StatType.CanDash,
            StatType.CanGainMana, StatType.CanGainStamina, StatType.IsAttacking, StatType.IsDashing,
            StatType.HurtTime, StatType.CurrentMana, StatType.CurrentStamina, StatType.Level,
            StatType.Xp, StatType.XpReq, StatType.Gold, StatType.goldDrop, StatType.XpDrop,
            StatType.Stealing, StatType.ExpBonus, StatType.DetectionRange
        };

        private static readonly HashSet<StatType> FlatStats = new()
        {
            StatType.attack, StatType.Intelligence, StatType.maxHp, StatType.hpRegen, StatType.armor,
            StatType.maxStamina, StatType.staminaRegen, StatType.maxMana, StatType.defShred, StatType.moveSpeed,
            StatType.EffAtk, StatType.EffInt, StatType.EffMaxHp, StatType.EffHpReg, StatType.EffStReg,
            StatType.EffArmor, StatType.EffSpd, StatType.EffMaxMana, StatType.EffMaxStamina
        };

        private IStatProvider src;
        private Object srcObj;
        private float share = 1f;
        private readonly Dictionary<StatType, float> overrides = new();

        protected override void Awake() => s = ScriptableObject.CreateInstance<EntityStats>();

        protected override void Start()
        {
            base.Start();
            if (s != null) s.currentHp = Mathf.RoundToInt(GetStat(StatType.EffMaxHp));
        }

        public void Setup(GameObject baseObj, float shr, List<StatShare> ovr)
        {
            src = null;
            srcObj = null;
            share = Mathf.Max(0f, shr);
            overrides.Clear();

            if (ovr != null)
                for (int i = 0; i < ovr.Count; i++) overrides[ovr[i].type] = Mathf.Max(0f, ovr[i].share);

            if (baseObj == null) return;

            if (baseObj.TryGetComponent<IStatProvider>(out var isp))
            {
                src = isp;
                srcObj = isp as Object;
            }

            teamID = baseObj.TryGetComponent<ITeamMember>(out var itm) ? itm.TeamID : 0;
        }

        public override float GetStat(StatType type)
        {
            float own = base.GetStat(type);
            if (src == null || srcObj == null || OwnStats.Contains(type)) return own;

            float m = overrides.TryGetValue(type, out var o) ? o : FlatStats.Contains(type) ? share : 1f;
            return own + (src.GetStat(type) * m);
        }
    }
}
