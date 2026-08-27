using System;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamagePacket packet);
    void TriggerIFrames(float duration);
    bool IsAlive { get; }
    event Action<GameObject> OnDeath;
}