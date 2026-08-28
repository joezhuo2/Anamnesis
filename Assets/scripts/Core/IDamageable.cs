using System;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public interface IDamageable
    {
        void TakeDamage(DamagePacket packet);
        void TriggerIFrames(float duration);
        bool IsAlive { get; }
        event Action<GameObject> OnDeath;
    }
}