using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeManager : MonoBehaviour
{
    public static PlayerUpgradeManager Instance { get; private set; }
    public List<PlayerUpgrade> activeUpgrades = new();

    private void Awake() 
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void AddUpgrade(PlayerUpgrade pu)
    {
        if (pu == null || activeUpgrades.Contains(pu)) return;
        activeUpgrades.Add(pu);
    }
    public void RemoveUpgrade(PlayerUpgrade pu)
    {
        if (pu == null || !activeUpgrades.Contains(pu)) return;
        activeUpgrades.Remove(pu);
    }
    public void TriggerUpgrades(PlayerUpgrade.TriggerCondition condition)
    {
        foreach (var u in activeUpgrades)
        {
            if (u == null) continue;

            foreach (var c in u.conditions)
            {
                if (c == condition)
                {
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