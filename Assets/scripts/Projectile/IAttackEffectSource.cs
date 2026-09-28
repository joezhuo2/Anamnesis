using System.Collections.Generic;
using CrystalFlux.Core;

namespace CrystalFlux.ProjectileSystem
{
    public interface IAttackEffectSource
    {
        IReadOnlyList<EffectData> GetExtraEffects(AttackType type);
    }
}
