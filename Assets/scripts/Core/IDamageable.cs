using System;
using CrystalFlux.Core;
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