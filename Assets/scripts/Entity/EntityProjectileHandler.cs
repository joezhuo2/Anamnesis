using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityProjectileHandler : MonoBehaviour, IOrbitRegister
{
    [Tooltip("Maximum orbiting projectiles tracked. 0 = unlimited.")]
    public int maxOrbiting = 0;

    private readonly List<Projectile> orbitingProjectiles = new();
    public int Count => orbitingProjectiles.Count;

    private void OnDestroy()
    {
        if (gameObject.activeInHierarchy) StartCoroutine(ClearOrbitsAfterDelay(0.1f));
    }
    public void RegisterOrbitingProjectile(Projectile p)
    {
        if (p == null || orbitingProjectiles.Contains(p)) return;

        if (maxOrbiting > 0 && Count >= maxOrbiting)
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
        if (p != null) orbitingProjectiles.Remove(p);
    }
    public void ReleaseOrbits(int count = 0)
    {
        for (int i = count == 0 ? Count - 1 : Mathf.Min(count, Count) - 1; i >= 0; i--)
        {
            Projectile p = orbitingProjectiles[i];
            if (p != null && p.gameObject != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(PlayerInputHandler.mousePos);
                mouseWorld.z = 0f;
                Vector2 dir = ((Vector2)mouseWorld - (Vector2)p.gameObject.transform.position).normalized;
                p.Launch(dir.normalized);
            }
        }
        StartCoroutine(ClearOrbitsAfterDelay(0.1f));
    }
    public void ReleaseOrbits(Vector2 dir, int count = 0)
    {
        for (int i = count == 0 ? Count - 1 : Mathf.Min(count, Count) - 1; i >= 0; i--)
        {
            Projectile p = orbitingProjectiles[i];
            if (p != null && p.gameObject != null)
                p.Launch(dir.normalized);
        }
        StartCoroutine(ClearOrbitsAfterDelay(0.1f));
    }
    public int AbsorbOrbits(int count = 0, float absorbPct = 0f)
    {
        for (int i = count == 0 ? Count - 1 : Mathf.Min(count, Count) - 1; i >= 0; i--)
        {
            Projectile p = orbitingProjectiles[i];
            if (p != null && p.gameObject != null)
            {
                TriggerStatGain(p, absorbPct * 0.01f);

                Destroy(p.gameObject);
            }
        }
        StartCoroutine(ClearOrbitsAfterDelay(0.1f));
        return count;
    }
    public void RedirectOrbits(int count = 0)
    {
        for (int i = count == 0 ? Count - 1 : Mathf.Min(count, Count) - 1; i >= 0; i--)
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
        StartCoroutine(ClearOrbitsAfterDelay(0.1f));
    }
    public void ExplodeOrbits(int count = 0)
    {
        for (int i = count == 0 ? Count - 1 : Mathf.Min(count, Count) - 1; i >= 0; i--)
        {
            Projectile p = orbitingProjectiles[i];
            if (p != null && p.gameObject != null)
                p.Explode();
        }
        StartCoroutine(ClearOrbitsAfterDelay(0.1f));
    }
    private IEnumerator ClearOrbitsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        orbitingProjectiles.Clear();
    }
    private void TriggerStatGain(Projectile p, float mult = 1f)
    {
        if (p == null || p.pd == null || p.ownerObj == null) return;

        if (p.pd.mainAttack == null) return;

        float hpGain = p.pd.mainAttack.healthGainOnHit * 0.01f * mult;
        float staminaGain = p.pd.mainAttack.staminaGainOnHit * 0.01f * mult;
        float manaGain = p.pd.mainAttack.manaGainOnHit * 0.01f * mult;

        var dp = DamagePacket.BuildDamagePacket(hpGain, DamageType.Heal, false, Color.green, p.ownerObj, true, 1f);

        GameObject target = p.ownerObj;
        if (target.TryGetComponent<IDamageable>(out var eh)) eh.TakeDamage(dp);
        if (target.TryGetComponent<IResourcePool>(out var pr))
        {
            pr.TryGain(ResourceType.Stamina, staminaGain);
            pr.TryGain(ResourceType.Mana, manaGain);
        }
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

            if (col.TryGetComponent<IStatProvider>(out var esm) && esm.GetStat(StatType.isAlive) == 1 && esm.GetStat(StatType.currentHp) <= 0)
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
