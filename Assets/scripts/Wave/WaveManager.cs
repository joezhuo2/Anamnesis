using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [Header("Basic")]
    public WaveSequence currentSequence;
    public float spawnRadius = 2f;

    [Header("Wave Settings")]
    public GameObject waveInfoPanel;
    public TextMeshProUGUI waveText;
    public Transform bossBarContainer;

    [Header("Reward Panel Settings")]
    public GameObject rewardPanel;
    public GameObject rewardButtonPrefab;
    public List<BaseReward> baseBuffPool;
    public List<AttackReward> rarePool;
    public List<PlayerUpgradeReward> treasurePool;
    public List<RarityData> rarityData;
    public Transform buttonContainer;
    public int rerolls;
    public Button rerollButton;
    public TextMeshProUGUI rerollText;

    private RewardType type = RewardType.Basic;
    private GameObject activeBossBar;
    private EntityStatManager cPlayerStatManager;
    private PlayerAttackHandler cpah;
    private PlayerUpgradeManager cpum;
    private List<GameObject> activeRewardButtons = new();
    private readonly List<AttackReward> availableRarePool = new();
    private readonly List<PlayerUpgradeReward> availableTreasurePool = new();
    private int currentWaveIndex = 0;
    private int totalSpawned = 0;
    private readonly List<GameObject> currentEnemies = new();
    private bool isWaveActive = false;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        availableRarePool.AddRange(rarePool);
        availableTreasurePool.AddRange(treasurePool);
    }
    private void Start()
    {
        rewardPanel.SetActive(false);
        waveInfoPanel.SetActive(true);

        UpdateRerollUI();
        StartNextWave();
    }
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

        waveInfoPanel.SetActive(true);
        waveText.text = $"Wave {currentWaveIndex + currentSequence.waveOffset}";

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
        Vector2 spawnPosition = Random.insideUnitCircle * spawnRadius + (Vector2)currentSequence.spawnLocation;
        GameObject enemy = Instantiate(c.enemyPrefab, spawnPosition, Quaternion.identity);

        if (enemy.TryGetComponent<EntityStatManager>(out var statManager))
            statManager.ScaleStatsToLevel(c.enemyLevel);

        if (c.bossBarPrefab != null && activeBossBar == null)
        {
            Transform spawnParent = bossBarContainer != null ? bossBarContainer : waveInfoPanel.transform.parent;
            activeBossBar = Instantiate(c.bossBarPrefab, spawnParent);

            if (activeBossBar.TryGetComponent<BossBarUI>(out var bossBarScript))
                bossBarScript.Setup(c.bossBarName, statManager);
        }

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
        waveInfoPanel.SetActive(false);
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

        if (activeBossBar != null)
        {
            Destroy(activeBossBar);
            activeBossBar = null;
        }

        int actualWave = currentWaveIndex + currentSequence.waveOffset;

        if (actualWave % 5 == 0)
            rerolls++;

        UpdateRerollUI();

        if (actualWave % 10 == 0)
        {
            type = RewardType.Treasure;
            GenerateTreasurePool();
        }
        else if (actualWave % 5 == 0)
        {
            type = RewardType.Rare;
            GenerateRarePool();
        }
        else
        {
            type = RewardType.Basic;
            GenerateRewards();
        }
    }
    private void GenerateRewards()
    {
        int rewardChoices = PoolPreSetup();

        for (int i = 0; i < rewardChoices; i++)
        {
            if (baseBuffPool.Count == 0 || rarityData.Count == 0) break;

            BaseReward randomBuff = GetWeightedRandomBuff();

            RarityData chosenRarity = GetWeightedRandomRarity();

            GeneratedReward generated = new() { br = randomBuff, rd = chosenRarity };

            Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
            GameObject btnObj = Instantiate(rewardButtonPrefab, targetParent);
            activeRewardButtons.Add(btnObj);

            if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                rewardButton.Setup(generated, OnRewardClaimed);
        }
    }
    private void GenerateRarePool()
    {
        int rewardChoices = PoolPreSetup();

        for (int i = 0; i < rewardChoices; i++)
        {
            if (availableRarePool.Count == 0) break;

            AttackReward buff = availableRarePool[Random.Range(0, availableRarePool.Count)];

            Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
            GameObject btnObj = Instantiate(rewardButtonPrefab, targetParent);
            activeRewardButtons.Add(btnObj);

            if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                rewardButton.Setup(buff, OnAttackRewardClaimed);
        }
    }
    public void GenerateTreasurePool()
    {
        int rewardChoices = PoolPreSetup();

        for (int i = 0; i < rewardChoices; i++)
        {
            if (availableTreasurePool.Count == 0) break;

            PlayerUpgradeReward buff = availableTreasurePool[Random.Range(0, availableTreasurePool.Count)];

            Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
            GameObject btnObj = Instantiate(rewardButtonPrefab, targetParent);
            activeRewardButtons.Add(btnObj);

            if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                rewardButton.Setup(buff, OnPlayerUpgradeRewardClaimed);
        }

    }
    private int PoolPreSetup()
    {
        WaveData completedWave = currentSequence.waves[currentWaveIndex - 1];
        int rewardChoices = Random.Range(completedWave.minRewardChoices, completedWave.maxRewardChoices + 1);

        if (rewardPanel != null) rewardPanel.SetActive(true);
        ClearRewardButtons();

        Time.timeScale = 0f;

        return rewardChoices;
    }
    private void UpdateRerollUI()
    {
        if (rerollText != null) rerollText.text = rerolls.ToString();

        if (rerollButton != null) rerollButton.interactable = rerolls > 0;
    }
    public void OnSkipButtonClicked()
    {
        ClearRewardButtons();
        CloseRewardUI();
        ResumeGameLoop();
    }
    public void OnRerollButtonClicked()
    {
        if (rerolls <= 0) return;

        rerolls--;
        UpdateRerollUI();

        ClearRewardButtons();

        switch (type)
        {
            case RewardType.Basic: GenerateRewards(); break;
            case RewardType.Rare: GenerateRarePool(); break;
            case RewardType.Treasure: GenerateTreasurePool(); break;
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

        return rarityData[0];
    }
    private BaseReward GetWeightedRandomBuff()
    {
        float totalWeight = 0;
        foreach (var b in baseBuffPool) totalWeight += b.weight;

        float roll = Random.Range(0f, totalWeight);
        float weightSum = 0;

        foreach (var b in baseBuffPool)
        {
            weightSum += b.weight;
            if (roll <= weightSum) return b;
        }

        return baseBuffPool[0];
    }
    private void OnRewardClaimed(GeneratedReward chosenReward)
    {
        CloseRewardUI();

        StatBuff finalBuff = new(chosenReward.br.baseBuff.type, chosenReward.finalVal);

        if (cPlayerStatManager == null)
            cPlayerStatManager = GameObject.FindWithTag("Player")?.GetComponent<EntityStatManager>();

        cPlayerStatManager?.AddStat(finalBuff);

        ResumeGameLoop();
    }
    private void OnAttackRewardClaimed(AttackReward chosenAttack)
    {
        CloseRewardUI();

        if (cpah == null) cpah = GameObject.FindWithTag("Player")?.GetComponent<PlayerAttackHandler>();

        cpah?.UpdateAttack(chosenAttack.type, chosenAttack.newAttack);

        if (availableRarePool.Contains(chosenAttack)) availableRarePool.Remove(chosenAttack);

        ResumeGameLoop();
    }
    private void OnPlayerUpgradeRewardClaimed(PlayerUpgradeReward chosenUpgrade)
    {
        CloseRewardUI();

        if (cpum == null) cpum = GameObject.FindWithTag("Player")?.GetComponent<PlayerUpgradeManager>();

        if (cpum != null) cpum.AddUpgrade(chosenUpgrade.upgrade);

        if (availableTreasurePool.Contains(chosenUpgrade)) availableTreasurePool.Remove(chosenUpgrade);

        ResumeGameLoop();
    }
    private void CloseRewardUI()
    {
        ClearRewardButtons();
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    private void ResumeGameLoop()
    {
        Time.timeScale = 1f;
        StartNextWave();
    }
    private void ClearRewardButtons()
    {
        foreach (var btn in activeRewardButtons) if (btn != null) Destroy(btn);
        activeRewardButtons.Clear();
    }
}