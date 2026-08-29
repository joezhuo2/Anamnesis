using System.Collections.Generic;
using CrystalFlux.Core;
using System;
using UnityEngine;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.EntitySystem;

namespace CrystalFlux.SkillTree
{
    [Serializable]
    public class NodeRequirement : IUnlockRequirement
    {
        public List<AttackData> requiredAttacks = new();
        public List<PlayerUpgrade> requiredAwakenings = new();

        public bool Has(GameObject target)
        {
            if (target == null) return false;

            if (requiredAttacks.Count > 0)
            {
                if (!target.TryGetComponent<PlayerAttackHandler>(out var pah)) return false;
                foreach (var a in requiredAttacks)
                {
                    if (a == null) continue;
                    if (!pah.HasAttack(a)) return false;
                }
            }

            if (requiredAwakenings.Count > 0)
            {
                if (!target.TryGetComponent<PlayerUpgradeManager>(out var pum)) return false;
                foreach (var u in requiredAwakenings)
                {
                    if (u == null) continue;
                    if (!pum.HasUpgrade(u)) return false;
                }
            }

            return true;
        }
    }
}