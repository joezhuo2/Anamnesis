using System.Collections.Generic;
using CrystalFlux.Core;
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace CrystalFlux.SkillTree
{
    [Serializable]
    [MovedFrom(true, sourceAssembly: "Assembly-CSharp")]
    public class UnlockEffect : IUnlockEffect
    {
        public List<StatBuff> buffs = new();
        public List<AttackAsset> attacks = new();
        public List<UpgradeAsset> awakenings = new();

        [NonSerialized] private readonly Dictionary<AttackType, AttackAsset> replaced = new();
        [NonSerialized] private readonly HashSet<UpgradeAsset> preOwned = new();

        public void Apply(GameObject target)
        {
            if (target == null) return;

            if (buffs.Count > 0 && target.TryGetComponent<IStatProvider>(out var isp))
                foreach (var b in buffs)
                    isp.AddStat(b);
            if (attacks.Count > 0 && target.TryGetComponent<IAttackHandler>(out var pah))
                foreach (var a in attacks)
                {
                    if (a == null) continue;
                    replaced[a.type] = pah.FindAttackOfType(a.type);
                    pah.UpdateAttack(a.type, a);
                }
            if (awakenings.Count > 0 && target.TryGetComponent<IUpgradeHolder>(out var pum))
                foreach (var u in awakenings)
                {
                    if (u == null) continue;
                    if (pum.HasUpgrade(u)) { preOwned.Add(u); continue; }
                    preOwned.Remove(u);
                    pum.AddUpgrade(u);
                }
        }
        public void Remove(GameObject target)
        {
            if (target == null) return;

            if (buffs.Count > 0 && target.TryGetComponent<IStatProvider>(out var isp))
                foreach (var b in buffs)
                    isp.AddStat(b, false);
            if (attacks.Count > 0 && target.TryGetComponent<IAttackHandler>(out var pah))
                foreach (var a in attacks)
                {
                    if (a == null || !pah.HasAttack(a)) continue;
                    if (replaced.TryGetValue(a.type, out var prev) && prev != null && prev != a) pah.UpdateAttack(a.type, prev);
                    else pah.RemoveAttack(a.type);
                    replaced.Remove(a.type);
                }
            if (awakenings.Count > 0 && target.TryGetComponent<IUpgradeHolder>(out var pum))
                foreach (var u in awakenings)
                    if (u != null && !preOwned.Remove(u)) pum.RemoveUpgrade(u);
        }
    }
}
