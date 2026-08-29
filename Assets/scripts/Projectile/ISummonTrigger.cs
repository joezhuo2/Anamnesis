using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    public interface ISummonTrigger
    {
        bool TrySummon(Vector2 position);
    }
}