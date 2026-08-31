using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;

namespace CrystalFlux.EntitySystem
{
    public class PlayerUpgradeManager : MonoBehaviour, IOnHitEffect, IUpgradeHolder
    {
        bool IUpgradeHolder.HasUpgrade(UpgradeAsset pu) => HasUpgrade(pu as PlayerUpgrade);
        void IUpgradeHolder.AddUpgrade(UpgradeAsset pu) => AddUpgrade(pu as PlayerUpgrade);
        void IUpgradeHolder.RemoveUpgrade(UpgradeAsset pu) => RemoveUpgrade(pu as PlayerUpgrade);

        public static PlayerUpgradeManager Instance { get; private set; }
        public List<PlayerUpgrade> activeUpgrades = new();
        private readonly Dictionary<PlayerUpgrade, float> lastTriggerTimes = new();
        private readonly HashSet<PlayerUpgrade> runtimeCopies = new();
        private bool isTriggeringOnSpawnProjectile;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        private void OnEnable() => ProjectileSpawner.ProjectileSpawned += HandleProjectileSpawned;
        private void OnDisable() => ProjectileSpawner.ProjectileSpawned -= HandleProjectileSpawned;

        private void HandleProjectileSpawned(GameObject sourceObj, GameObject projectile, Vector2 spawnPos)
        {
            if (sourceObj != gameObject || isTriggeringOnSpawnProjectile) return;

            isTriggeringOnSpawnProjectile = true;
            TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnSpawnProjectile, spawnPos);
            isTriggeringOnSpawnProjectile = false;
        }
        private void Start()
        {
            for (int i = 0; i < activeUpgrades.Count; i++)
            {
                if (activeUpgrades[i] == null) continue;

                PlayerUpgrade runtime = ToRuntimeCopy(activeUpgrades[i]);
                activeUpgrades[i] = runtime;
                runtime.OnUnlock(gameObject);
            }

            WarnOnDuplicateNames();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            foreach (var u in activeUpgrades)
                if (u != null && runtimeCopies.Contains(u)) Destroy(u);

            activeUpgrades.Clear();
            runtimeCopies.Clear();
            lastTriggerTimes.Clear();
        }

        private PlayerUpgrade ToRuntimeCopy(PlayerUpgrade source)
        {
            if (runtimeCopies.Contains(source)) return source;

            PlayerUpgrade copy = Instantiate(source);
            copy.name = source.name;
            runtimeCopies.Add(copy);
            return copy;
        }

        private void WarnOnDuplicateNames()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var u in activeUpgrades)
            {
                if (u == null) continue;
                if (!seen.Add(u.name.Trim()))
                    Debug.LogError($"Two upgrades on '{name}' share the name '{u.name}'. Upgrades are matched by name, so one of them is unreachable.", this);
            }
        }
        public bool HasUpgradeOfType<T>() where T : PlayerUpgrade
        {
            for (int i = 0; i < activeUpgrades.Count; i++)
                if (activeUpgrades[i] is T) return true;
            return false;
        }
        public bool HasUpgrade(PlayerUpgrade pu) => FindActive(pu) != null;

        private PlayerUpgrade FindActive(PlayerUpgrade pu)
        {
            if (pu == null) return null;

            string wanted = pu.name.Trim();
            for (int i = 0; i < activeUpgrades.Count; i++)
            {
                if (activeUpgrades[i] == null) continue;
                if (activeUpgrades[i].name.Trim().Equals(wanted, StringComparison.OrdinalIgnoreCase)) return activeUpgrades[i];
            }
            return null;
        }
        public PlayerUpgrade GetPlayerUpgradeOfType<T>() where T : PlayerUpgrade
        {
            for (int i = 0; i < activeUpgrades.Count; i++)
                if (activeUpgrades[i] is T) return activeUpgrades[i];
            return null;
        }
        public void AddUpgrade(PlayerUpgrade pu)
        {
            if (pu == null || FindActive(pu) != null) return;

            PlayerUpgrade runtime = ToRuntimeCopy(pu);
            activeUpgrades.Add(runtime);
            runtime.OnUnlock(gameObject);
        }
        public void RemoveUpgrade(PlayerUpgrade pu)
        {
            PlayerUpgrade active = FindActive(pu);
            if (active == null) return;

            activeUpgrades.Remove(active);
            lastTriggerTimes.Remove(active);
            active.OnRemove(gameObject);

            if (runtimeCopies.Remove(active)) Destroy(active);
        }
        public void TriggerUpgrades(PlayerUpgrade.TriggerCondition condition)
        {
            float now = Time.time;

            foreach (var u in activeUpgrades)
            {
                if (u == null) continue;

                if (u.cooldown > 0f && lastTriggerTimes.TryGetValue(u, out float lastTriggerTime) && now < lastTriggerTime + u.cooldown)
                    continue;

                foreach (var c in u.conditions)
                {
                    if (c == condition)
                    {
                        if (UnityEngine.Random.Range(0f, 100f) > u.chance) continue;
                        lastTriggerTimes[u] = now;
                        if (u.delay > 0) StartCoroutine(TriggerWithDelay(u));
                        else u.TriggerUpgradeEffect(gameObject);
                        break;
                    }
                }
            }
        }
        public void TriggerUpgrades(PlayerUpgrade.TriggerCondition condition, Vector2 spawnCenter)
        {
            float now = Time.time;

            foreach (var u in activeUpgrades)
            {
                if (u == null) continue;

                if (u.cooldown > 0f && lastTriggerTimes.TryGetValue(u, out float lastTriggerTime) && now < lastTriggerTime + u.cooldown)
                    continue;

                foreach (var c in u.conditions)
                {
                    if (c == condition)
                    {
                        if (UnityEngine.Random.Range(0f, 100f) > u.chance) continue;
                        lastTriggerTimes[u] = now;
                        if (u.delay > 0) StartCoroutine(TriggerWithDelay(u));
                        else u.TriggerUpgradeEffect(gameObject, spawnCenter);
                        break;
                    }
                }
            }
        }
        public void TriggerUpgrades(PlayerUpgrade.TriggerCondition condition, GameObject target, float damageDealt)
        {
            float now = Time.time;

            foreach (var u in activeUpgrades)
            {
                if (u == null) continue;

                if (u.cooldown > 0f && lastTriggerTimes.TryGetValue(u, out float lastTriggerTime) && now < lastTriggerTime + u.cooldown)
                    continue;

                foreach (var c in u.conditions)
                {
                    if (c == condition)
                    {
                        if (UnityEngine.Random.Range(0f, 100f) > u.chance) continue;
                        lastTriggerTimes[u] = now;
                        if (u.delay > 0) StartCoroutine(TriggerWithDelay(u, target, damageDealt));
                        else u.TriggerUpgradeEffect(gameObject, target, damageDealt);
                        break;
                    }
                }
            }
        }
        private IEnumerator TriggerWithDelay(PlayerUpgrade u)
        {
            yield return new WaitForSeconds(u.delay);
            u.TriggerUpgradeEffect(gameObject);
        }
        private IEnumerator TriggerWithDelay(PlayerUpgrade u, GameObject target, float damageDealt)
        {
            yield return new WaitForSeconds(u.delay);
            u.TriggerUpgradeEffect(gameObject, target, damageDealt);
        }

        public void OnHit(GameObject projectileOwner, GameObject _, Vector3 hitPosition)
        {
            if (projectileOwner.TryGetComponent<PlayerUpgradeManager>(out var pum))
                pum.TriggerUpgrades(PlayerUpgrade.TriggerCondition.OnProjectileHit, hitPosition);
        }
    }
}
