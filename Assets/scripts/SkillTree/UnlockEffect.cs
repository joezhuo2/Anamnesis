using System.Collections.Generic;
using CrystalFlux.Core;
using System;
using UnityEngine;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.EntitySystem;

namespace CrystalFlux.SkillTree
{
    [Serializable]
    public class UnlockEffect : IUnlockEffect
    {
        public List<StatBuff> buffs = new();
        public List<AttackData> attacks = new();
        public List<PlayerUpgrade> awakenings = new();

        public void Apply(GameObject target) 
        {
            if (buffs.Count > 0 && target.TryGetComponent<IStatProvider>(out var isp))
                foreach (var b in buffs)
                    isp.AddStat(b);
            if (attacks.Count > 0 && target.TryGetComponent<PlayerAttackHandler>(out var pah))
                foreach (var a in attacks)
                    if (a != null) pah.UpdateAttack(a.type, a);
            if (awakenings.Count > 0 && target.TryGetComponent<PlayerUpgradeManager>(out var pum))
                foreach (var u in awakenings)
                    if (u != null) pum.AddUpgrade(u);
        }
        public void Remove(GameObject target) 
        {
            if (buffs.Count > 0 && target.TryGetComponent<IStatProvider>(out var isp))
                foreach (var b in buffs)
                    isp.AddStat(b, false);
            if (attacks.Count > 0 && target.TryGetComponent<PlayerAttackHandler>(out var pah))
                foreach (var a in attacks)
                    if (a != null) pah.RemoveAttack(a.type);
            if (awakenings.Count > 0 && target.TryGetComponent<PlayerUpgradeManager>(out var pum))
                foreach (var u in awakenings)
                    if (u != null) pum.RemoveUpgrade(u);
        }
    }
}