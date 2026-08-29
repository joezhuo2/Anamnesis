using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    public interface IOnHitEffect
    {
        void OnHit(GameObject projectileOwner, GameObject target, Vector3 hitPosition);
    }
}
