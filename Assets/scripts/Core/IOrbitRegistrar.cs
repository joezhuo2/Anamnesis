using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    public interface IOrbitRegister
    {
        void RegisterOrbitingProjectile(Projectile p);
        void UnregisterOrbitingProjectile(Projectile p);
        int Count { get; }
    }
}