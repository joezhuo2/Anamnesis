using CrystalFlux.ProjectileSystem;
using UnityEngine;

namespace CrystalFlux.Core
{
    public interface IOnHitEffect
    {
        void OnHit(GameObject projectileOwner, GameObject target, Vector3 hitPosition);
    }
}
