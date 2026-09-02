using System.Collections.Generic;
using UnityEngine;

namespace CrystalFlux.Core
{
    public class EntityStatManager : MonoBehaviour, IStatProvider, ICurrencyHolder, ITeamMember
    {
        public EntityStats baseStats;
        protected EntityStats s;
        [HideInInspector] public List<StatBuff> currentBuffs = new();

        public int teamID = 0;

        public int CurrentAmount => s.gold;
        public int TeamID => teamID;

        private static readonly HashSet<StatType> GateFlags = new()
        {
            StatType.CanMove, StatType.CanAttack, StatType.CanDash,
            StatType.CanGainHp, StatType.CanGainMana, StatType.CanGainStamina
        };
        private static readonly HashSet<StatType> GrantFlags = new() { StatType.isImmune };
        private readonly Dictionary<StatType, int> flagDepth = new();

        protected virtual void Awake()
        {
            if (baseStats != null) s = Instantiate(baseStats);
        }
        protected virtual void Start()
        {
            flagDepth.Clear();

            if (s != null)
            {
                s.currentHp = s.EffMaxHp;
                s.overhealth = 0f;
                s.canAttack = true;
                s.isAttacking = false;
                s.canMove = true;
                s.canGainHp = true;
                s.isAlive = true;
                s.isImmune = false;
            }
        }

        private void OnDestroy()
        {
            if (s != null) Destroy(s);
        }

        public float GetStat(StatType type) => s == null ? 0f : s.GetValue(type);

        public void AddStat(StatBuff b, bool isAdding = true)
        {
            if (s == null) return;
            float mod = b.value * (isAdding ? 1f : -1f);

            if (TryApplyCountedFlag(b.type, mod)) return;

            s.Apply(b.type, mod);
        }

        private bool TryApplyCountedFlag(StatType type, float mod)
        {
            bool isGate = GateFlags.Contains(type);
            if (!isGate && !GrantFlags.Contains(type)) return false;

            if (mod == 0f) return true;

            flagDepth.TryGetValue(type, out int depth);

            bool releasing = isGate ? mod > 0f : mod < 0f;
            depth = releasing ? Mathf.Max(0, depth - 1) : depth + 1;
            flagDepth[type] = depth;

            bool on = isGate ? depth == 0 : depth > 0;
            s.Apply(type, on ? 1f : -1f);
            return true;
        }

        public bool TrySpend(int amount)
        {
            if (s == null || s.gold < amount) return false;
            s.gold -= amount;
            return true;
        }

        public bool AddCurrency(int amount)
        {
            if (s == null) return false;
            s.gold += amount;
            return true;
        }
    }
}
