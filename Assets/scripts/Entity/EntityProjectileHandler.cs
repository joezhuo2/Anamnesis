using System.Collections.Generic;
using UnityEngine;

public class EntityProjectileHandler : MonoBehaviour
{
    [Tooltip("Maximum orbiting projectiles tracked. 0 = unlimited.")]
    public int maxOrbiting = 0;
    public float orbitGainPct = 50f;

    private readonly List<Projectile> orbitingProjectiles = new();
    public int Count => orbitingProjectiles.Count;

    private void OnDestroy()
    {
        orbitingProjectiles.Clear();
    }
    public void RegisterOrbitingProjectile(Projectile p)
    {
        if (p == null || orbitingProjectiles.Contains(p)) return;

        if (maxOrbiting > 0 && orbitingProjectiles.Count >= maxOrbiting)
        {
            Projectile oldest = orbitingProjectiles[0];
            orbitingProjectiles.RemoveAt(0);
            if (oldest != null && oldest.gameObject != null)
                Destroy(oldest.gameObject);
        }

        orbitingProjectiles.Add(p);
    }
    public void UnregisterOrbitingProjectile(Projectile p)
    {
        if (p != null)
            orbitingProjectiles.Remove(p);
    }
    public void ReleaseAll(Vector2 direction)
    {
        for (int i = orbitingProjectiles.Count - 1; i >= 0; i--)
        {
            Projectile p = orbitingProjectiles[i];
            if (p != null && p.gameObject != null)
                p.Launch(direction.normalized);
        }
        orbitingProjectiles.Clear();
    }
    public int AbsorbAll()
    {
        int count = orbitingProjectiles.Count;

        for (int i = count - 1; i >= 0; i--)
        {
            Projectile p = orbitingProjectiles[i];
            if (p != null && p.gameObject != null)
            {
                TriggerStatGain(p);

                Destroy(p.gameObject);
            }
        }
        orbitingProjectiles.Clear();
        return count;
    }
    public void RedirectAll()
    {
        for (int i = orbitingProjectiles.Count - 1; i >= 0; i--)
        {
            Projectile p = orbitingProjectiles[i];
            if (p == null || p.gameObject == null)
            {
                orbitingProjectiles.RemoveAt(i);
                continue;
            }

            Transform target = FindNearestEnemy(p.transform.position);
            if (target != null)
            {
                Vector2 dir = ((Vector2)target.position - (Vector2)p.transform.position).normalized;
                p.Launch(dir);
            }
        }
        orbitingProjectiles.Clear();
    }
    public void ExplodeAll()
    {
        for (int i = orbitingProjectiles.Count - 1; i >= 0; i--)
        {
            Projectile p = orbitingProjectiles[i];
            if (p != null && p.gameObject != null)
                p.Explode();
        }
        orbitingProjectiles.Clear();
    }
    private void TriggerStatGain(Projectile p)
    {
        if (p == null || p.pd == null || p.ownerObj == null) return;

        if (p.pd.mainAttack == null) return;

        float hpGain = p.pd.mainAttack.healthGainOnHit * orbitGainPct * 0.01f;
        float staminaGain = p.pd.mainAttack.staminaGainOnHit * orbitGainPct * 0.01f;
        float manaGain = p.pd.mainAttack.manaGainOnHit * orbitGainPct * 0.01f;

        GameObject target = p.ownerObj;
        if (target.TryGetComponent<EntityHealth>(out var eh)) eh.ChangeHealth(hpGain, 0f);
        if (target.TryGetComponent<PlayerStamina>(out var ps)) ps.ChangeStamina(staminaGain);
        if (target.TryGetComponent<PlayerMana>(out var pm)) pm.ChangeMana(manaGain, 0f);
    }
    private Transform FindNearestEnemy(Vector3 position)
    {
        Transform closest = null;
        float closestDist = float.MaxValue;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 20f);
        foreach (Collider2D col in colliders)
        {
            if (!col.CompareTag("Enemy")) continue;
            if (col.gameObject == gameObject) continue;

            if (col.TryGetComponent<EntityStatManager>(out var esm) && !esm.s.isAlive && esm.s.currentHp <= 0)
                continue;

            float dist = Vector2.Distance(position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = col.transform;
            }
        }
        return closest;
    }
}
