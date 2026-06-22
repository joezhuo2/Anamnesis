using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public WaveSequence currentSequence;
    private int currentWaveIndex = 0;
    private int totalSpawned = 0;
    private readonly List<GameObject> currentEnemies = new();
    private bool isWaveActive = false;
    private Coroutine spawnCoroutine;

    public GameObject rewardPanel;
    public GameObject rewardButtonPrefab;
    public List<BaseReward> baseBuffPool;
    public List<RarityData> rarityData;
    private List<GameObject> activeRewardButtons = new();

    private EntityStatManager cPlayerStatManager;

    public void StartNextWave()
    {
        if (isWaveActive) return;

        if (currentWaveIndex >= currentSequence.waves.Count)
        {
            if (currentSequence.nextSequence != null)
            {
                currentSequence = currentSequence.nextSequence;
                currentWaveIndex = 0;
            }
            else
            {
                return;
            }
        }

        WaveData currentWave = currentSequence.waves[currentWaveIndex];

        totalSpawned = 0;
        currentEnemies.Clear();
        isWaveActive = true;
        currentWaveIndex++;

        HandleWave(currentWave);
    }
    private void HandleWave(WaveData c)
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(WaveSpawnRoutine(c));
    }
    private IEnumerator WaveSpawnRoutine(WaveData c)
    {
        while (totalSpawned < c.maxTotalEnemies)
        {
            if (currentEnemies.Count >= c.maxCurrentEnemies)
            {
                yield return null;
                continue;
            }

            HandleSpawns(c);

            float spawnDelay = Random.Range(c.minSpawnFrequency, c.maxSpawnFrequency);
            yield return new WaitForSeconds(spawnDelay);
        }
        while (currentEnemies.Count > 0)
        {
            CleanEnemyList();
            yield return new WaitForSeconds(0.5f);
        }
        EndWave();
    }
    private void HandleSpawns(WaveData c)
    {
        GameObject enemy = Instantiate(c.enemyPrefab, currentSequence.spawnLocation, Quaternion.identity);

        if (enemy.TryGetComponent<EntityStatManager>(out var statManager))
            statManager.ScaleStatsToLevel(c.enemyLevel);

        totalSpawned++;
        currentEnemies.Add(enemy);
    }
    private void CleanEnemyList()
    {
        currentEnemies.RemoveAll(enemy => enemy == null);
    }
    private void EndWave()
    {
        isWaveActive = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

        GenerateRewards();
    }
    private void GenerateRewards()
    {
        WaveData completedWave = currentSequence.waves[currentWaveIndex - 1];
        int rewardChoices = Random.Range(completedWave.minRewardChoices, completedWave.maxRewardChoices + 1);

        if (rewardPanel != null) rewardPanel.SetActive(true);
        ClearRewardButtons();

        for (int i = 0; i < rewardChoices; i++)
        {
            if (baseBuffPool.Count == 0 || rarityData.Count == 0) break;

            BaseReward randomBuff = baseBuffPool[Random.Range(0, baseBuffPool.Count)];

            RarityData chosenRarity = GetWeightedRandomRarity();

            GeneratedReward generated = new() { br = randomBuff, rd = chosenRarity };

            GameObject btnObj = Instantiate(rewardButtonPrefab, rewardPanel.transform);
            activeRewardButtons.Add(btnObj);

            if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                rewardButton.Setup(generated, OnRewardClaimed);
        }
    }

    private RarityData GetWeightedRandomRarity()
    {
        float totalWeight = 0;
        foreach (var d in rarityData) totalWeight += d.weight;

        float roll = Random.Range(0f, totalWeight);
        float weightSum = 0;

        foreach (var d in rarityData)
        {
            weightSum += d.weight;
            if (roll <= weightSum) return d;
        }

        return rarityData[0]; // Fallback
    }
    private void OnRewardClaimed(GeneratedReward chosenReward)
    {
        ClearRewardButtons();
        if (rewardPanel != null) rewardPanel.SetActive(false);

        StatBuff finalBuff = new(chosenReward.br.baseBuff.type, chosenReward.finalVal);

        if (cPlayerStatManager == null)
            cPlayerStatManager = GameObject.FindWithTag("Player")?.GetComponent<EntityStatManager>();

        cPlayerStatManager.AddStat(finalBuff);

        StartNextWave();
    }

    private void ClearRewardButtons()
    {
        foreach (var btn in activeRewardButtons) if (btn != null) Destroy(btn);
        activeRewardButtons.Clear();
    }
}