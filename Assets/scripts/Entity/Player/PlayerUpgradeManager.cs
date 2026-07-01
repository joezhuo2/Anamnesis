using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeManager : MonoBehaviour
{
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
    public bool HasUpgradeOfType<T>() where T : PlayerUpgrade
    {
        for (int i = 0; i < activeUpgrades.Count; i++)
            if (activeUpgrades[i] is T) return true;
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
                    if (Random.Range(0f, 100f) > u.chance) continue;
                    lastTriggerTimes[u] = now;
                    if (u.delay > 0) StartCoroutine(TriggerWithDelay(u));
                    else u.TriggerUpgradeEffect(gameObject);
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
}