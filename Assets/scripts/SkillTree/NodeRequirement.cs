using System.Collections.Generic;
using CrystalFlux.Core;
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace CrystalFlux.SkillTree
{
    [Serializable]
    [MovedFrom(true, sourceAssembly: "Assembly-CSharp")]
    public class NodeRequirement : IUnlockRequirement
    {
        public List<AttackAsset> requiredAttacks = new();
        public List<UpgradeAsset> requiredAwakenings = new();

        public bool Has(GameObject target)
        {
            if (target == null) return false;

            if (requiredAttacks.Count > 0)
            {
                if (!target.TryGetComponent<IAttackHandler>(out var pah)) return false;
                foreach (var a in requiredAttacks)
                {
                    if (a == null) continue;
                    if (!pah.HasAttack(a)) return false;
                }
            }

            if (requiredAwakenings.Count > 0)
            {
                if (!target.TryGetComponent<IUpgradeHolder>(out var pum)) return false;
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