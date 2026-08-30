using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrystalFlux.Core;

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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        private void Start()
        {
            foreach (var u in activeUpgrades)
                if (u != null) u.OnUnlock(gameObject);
        }
        public bool HasUpgradeOfType<T>() where T : PlayerUpgrade
        {
            for (int i = 0; i < activeUpgrades.Count; i++)
                if (activeUpgrades[i] is T) return true;
            return false;
        }
        public bool HasUpgrade(PlayerUpgrade pu)
        {
            if (pu == null) return false;
            for (int i = 0; i < activeUpgrades.Count; i++)
                if (activeUpgrades[i].name.Trim().Equals(pu.name.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
        public PlayerUpgrade GetPlayerUpgradeOfType<T>() where T : PlayerUpgrade
        {
            for (int i = 0; i < activeUpgrades.Count; i++)
                if (activeUpgrades[i] is T) return activeUpgrades[i];
            return null;
        }
        public void AddUpgrade(PlayerUpgrade pu)
        {
            if (pu == null || activeUpgrades.Contains(pu)) return;
            activeUpgrades.Add(pu);
            pu.OnUnlock(gameObject);
        }
        public void RemoveUpgrade(PlayerUpgrade pu)
        {
            if (pu == null || !activeUpgrades.Contains(pu)) return;
            activeUpgrades.Remove(pu);
            lastTriggerTimes.Remove(pu);
            pu.OnRemove(gameObject);
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
