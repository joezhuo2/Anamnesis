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
    public TextMeshProUGUI anamolyInfoText;
    public TextMeshProUGUI waveText;
    public Transform bossBarContainer;
    public Transform statusEffectDisplayContainer;

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

    [Header("Anomaly Settings")]
    public List<AnomalyData> availableAnamolies = new();
    public GameObject anamolyPrefab = null;
    public AnomalyInstance currentAnomaly = null;
    public int minAnomalyCount = 2;
    public int maxAnomalyCount = 5;
    public float anamolyChance = 10;
    public float anamolyGlobalMinWave = 10;

    private RewardType type = RewardType.Basic;
    private GameObject activeBossBar;
    private EntityStatManager cpsm;
    private PlayerAttackHandler cpah;
    private PlayerUpgradeManager cpum;
    private readonly List<AttackReward> availableRarePool = new();
    private readonly List<PlayerUpgradeReward> availableTreasurePool = new();
    private int currentWaveIndex = 0;
    private int totalSpawned = 0;
    private readonly List<GameObject> currentEnemies = new();
    private bool isWaveActive = false;
    private Coroutine spawnCoroutine;
    private bool pendingStandardRewards = false;
    private float additionalQuality = 0f;
    private readonly List<GameObject> activeRewardButtons = new();
    private readonly List<GameObject> inactiveRewardButtonPool = new();

    private void Awake()
    {
        availableRarePool.AddRange(rarePool);
        availableTreasurePool.AddRange(treasurePool);
    }
    private void Start()
    {
        rewardPanel.SetActive(false);
        waveInfoPanel.SetActive(true);

        rarityData.Sort((a, b) => a.mult.CompareTo(b.mult));

        UpdateRerollUI();
        StartNextWave();
    }
    private void Update()
    {
        if (currentAnomaly != null && currentAnomaly.isActive)
        {
            currentAnomaly.UpdateCheck(Time.deltaTime);

            if (anamolyInfoText != null)
            {
                switch (currentAnomaly.amd.anamolyType)
                {
                    case AnomalyType.TimeTrial: UpdateAnomalyTimeInfo(); break;
                    default: break;
                }
            }
        }
        else
        {
            ClearAnamolyText();
        }
    }
    private void ClearAnamolyText()
    {
        if (anamolyInfoText != null && anamolyInfoText.text != "")
            anamolyInfoText.text = "";
    }

    private void UpdateAnomalyTimeInfo()
    {
        if (currentAnomaly is TimeTrialInstance tt)
            anamolyInfoText.text = $"Time Remaining: {tt.timeRemaining:F1}s";
    }
    private GameObject GetOrCreateRewardButton()
    {
        GameObject btnObj;
        Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;

        if (inactiveRewardButtonPool.Count > 0)
        {
            int lastIndex = inactiveRewardButtonPool.Count - 1;
            btnObj = inactiveRewardButtonPool[lastIndex];
            inactiveRewardButtonPool.RemoveAt(lastIndex);

            btnObj.transform.SetParent(targetParent, false);
            btnObj.SetActive(true);
        }
        else
        {
            btnObj = Instantiate(rewardButtonPrefab, targetParent);
        }

        activeRewardButtons.Add(btnObj);
        return btnObj;
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

        totalSpawned = 0;
        currentEnemies.Clear();

        if (!RollAndGenerateAnomaly()) BeginWave();
    }
    private void BeginWave()
    {
        WaveData currentWave = currentSequence.waves[currentWaveIndex];

        isWaveActive = true;
        currentWaveIndex++;

        waveInfoPanel.SetActive(true);
        waveText.text = $"Wave {GetCurrentWave()}";

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

        if (c.statusEffectDisplayPrefab != null && enemy.TryGetComponent<StatusEffectManager>(out var sem))
        {
            Transform spawnParent = statusEffectDisplayContainer != null ? statusEffectDisplayContainer : waveInfoPanel.transform.parent;
            sem.displayPrefab = c.statusEffectDisplayPrefab;
            sem.displayContainer = spawnParent;
        }

        totalSpawned++;
        currentEnemies.Add(enemy);
    }
    private void CleanEnemyList() => currentEnemies.RemoveAll(enemy => enemy == null);
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

        int actualWave = GetCurrentWave();
        if (actualWave % 5 == 0) rerolls++;

        UpdateRerollUI();

        if (currentAnomaly != null)
        {
            if (currentAnomaly.isActive)
            {
                HandleAnomalyRewards();
            }
            else
            {
                currentAnomaly = null;
                ResumeGameLoop();
            }
        }
        else
        {
            TriggerStandardRewards(actualWave);
        }
    }
    private void HandleAnomalyRewards()
    {
        currentAnomaly.CompleteAnomaly();
        currentAnomaly = null;

        pendingStandardRewards = true;

        rerolls += Random.Range(1, 3);
        UpdateRerollUI();

        additionalQuality += Random.Range(0.1f, 0.3f);

        type = RewardType.Mixed;
        PanelSetup();
        GenerateMixedPool();
    }
    private void TriggerStandardRewards(int w)
    {
        pendingStandardRewards = false;
        currentAnomaly = null;

        if (w % 10 == 0) GenerateTreasurePool();
        else if (w % 5 == 0) GenerateRarePool();
        else GenerateRewards();
    }
    private bool RollAndGenerateAnomaly()
    {
        if (currentAnomaly != null && currentAnomaly.isActive) return false;
        if (availableAnamolies == null || availableAnamolies.Count == 0) return false;
        if (anamolyPrefab == null || GetCurrentWave() <= anamolyGlobalMinWave) return false;
        if (minAnomalyCount <= 0 || maxAnomalyCount <= 0 || anamolyChance <= 0f) return false;

        float roll = Random.Range(0f, 100f);
        if (roll > anamolyChance) return false;

        var available = availableAnamolies.FindAll(a => GetCurrentWave() >= a.minWave);
        if (available.Count == 0) return false;

        PanelSetup();
        type = RewardType.Anomaly;

        int choices = Random.Range(minAnomalyCount, maxAnomalyCount + 1);

        for (int i = 0; i < choices; i++)
        {
            AnomalyData amd = available[Random.Range(0, available.Count)];

            Transform targetParent = buttonContainer != null ? buttonContainer : rewardPanel.transform;
            GameObject btnObj = Instantiate(anamolyPrefab, targetParent);
            activeRewardButtons.Add(btnObj);

            if (btnObj.TryGetComponent<AnomalyButtonUI>(out var anamolyButton))
                anamolyButton.Setup(amd, OnAnomalyButtonClicked);
        }

        return true;
    }
    private void GenerateRewards()
    {
        type = RewardType.Basic;
        int rewardChoices = PoolPreSetup();

        for (int i = 0; i < rewardChoices; i++)
        {
            if (baseBuffPool.Count == 0 || rarityData.Count == 0) break;

            BaseReward randomBuff = GetWeightedRandomBuff();

            RarityData chosenRarity = WaveQuality.GetWeightedRandomRarity(GetCurrentWave(), rarityData, additionalQuality);

            GeneratedReward generated = new() { br = randomBuff, rd = chosenRarity };

            GameObject btnObj = GetOrCreateRewardButton();

            if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                rewardButton.Setup(generated, OnRewardClaimed);
        }
        additionalQuality = 0f;
    }
    private void    GenerateMixedPool()
    {
        int rewardChoices = PoolPreSetup();

        for (int i = 0; i < rewardChoices; i++)
        {
            float poolRoll = Random.Range(0f, 100f);

            if (poolRoll < 60f)
            {
                if (baseBuffPool.Count == 0 || rarityData.Count == 0) continue;

                BaseReward randomBuff = GetWeightedRandomBuff();
                RarityData chosenRarity = WaveQuality.GetWeightedRandomRarity(GetCurrentWave(), rarityData);
                GeneratedReward generated = new() { br = randomBuff, rd = chosenRarity };
                GameObject btnObj = GetOrCreateRewardButton();
                if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(generated, OnRewardClaimed);
            }
            else if (poolRoll < 75f)
            {
                if (availableRarePool.Count == 0) continue;
                AttackReward buff = availableRarePool[Random.Range(0, availableRarePool.Count)];
                GameObject btnObj = GetOrCreateRewardButton();
                if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnAttackRewardClaimed);
            }
            else
            {
                if (availableTreasurePool.Count == 0) continue;
                PlayerUpgradeReward buff = availableTreasurePool[Random.Range(0, availableTreasurePool.Count)];
                GameObject btnObj = GetOrCreateRewardButton();
                if (btnObj.TryGetComponent<RewardButton>(out var rb)) rb.Setup(buff, OnPlayerUpgradeRewardClaimed);
            }
        }
    }
    private void GenerateRarePool()
    {
        type = RewardType.Rare;
        int rewardChoices = PoolPreSetup();

        for (int i = 0; i < rewardChoices; i++)
        {
            if (availableRarePool.Count == 0) break;

            AttackReward buff = availableRarePool[Random.Range(0, availableRarePool.Count)];

            GameObject btnObj = GetOrCreateRewardButton();

            if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                rewardButton.Setup(buff, OnAttackRewardClaimed);
        }
    }
    public void GenerateTreasurePool()
    {
        type = RewardType.Treasure;
        int rewardChoices = PoolPreSetup();

        for (int i = 0; i < rewardChoices; i++)
        {
            if (availableTreasurePool.Count == 0) break;

            PlayerUpgradeReward buff = availableTreasurePool[Random.Range(0, availableTreasurePool.Count)];

            GameObject btnObj = GetOrCreateRewardButton();

            if (btnObj.TryGetComponent<RewardButton>(out var rewardButton))
                rewardButton.Setup(buff, OnPlayerUpgradeRewardClaimed);
        }
    }
    private int PoolPreSetup()
    {
        WaveData completedWave = currentSequence.waves[currentWaveIndex - 1];
        int rewardChoices = Random.Range(completedWave.minRewardChoices, completedWave.maxRewardChoices + 1);

        PanelSetup();

        return rewardChoices;
    }
    private void PanelSetup()
    {
        if (rewardPanel != null) rewardPanel.SetActive(true);
        ClearRewardButtons();

        Time.timeScale = 0f;
    }
    private void UpdateRerollUI()
    {
        if (rerollText != null) rerollText.text = rerolls.ToString();

        if (rerollButton != null) rerollButton.interactable = rerolls > 0;
    }
    public void OnSkipButtonClicked()
    {
        if (type == RewardType.Anomaly)
        {
            CloseRewardUI();
            Time.timeScale = 1f;
            currentAnomaly = null;
            BeginWave();
            return;
        }

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
            case RewardType.Anomaly: RollAndGenerateAnomaly(); break;
            case RewardType.Basic: GenerateRewards(); break;
            case RewardType.Rare: GenerateRarePool(); break;
            case RewardType.Treasure: GenerateTreasurePool(); break;
            case RewardType.Mixed: GenerateMixedPool(); break;
            default: break;
        }
    }
    public void OnAnomalyButtonClicked(AnomalyData amd)
    {
        CloseRewardUI();
        Time.timeScale = 1f;

        if (amd != null)
        {
            currentAnomaly = amd.CreateInstance();
            currentAnomaly.StartAnomaly();
        }

        WaveData currentWave = currentSequence.waves[currentWaveIndex];

        BeginWave();
        HandleWave(currentWave);
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
        if (cpsm == null) cpsm = GameObject.FindWithTag("Player")?.GetComponent<EntityStatManager>();
        if (cpsm != null) cpsm.AddStat(finalBuff);

        ResumeGameLoop();
    }
    private void OnAttackRewardClaimed(AttackReward chosenAttack)
    {
        CloseRewardUI();

        if (cpah == null) cpah = GameObject.FindWithTag("Player")?.GetComponent<PlayerAttackHandler>();
        if (cpah != null) cpah.UpdateAttack(chosenAttack.type, chosenAttack.newAttack);
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
        if (pendingStandardRewards)
        {
            TriggerStandardRewards(GetCurrentWave());
        }
        else
        {
            Time.timeScale = 1f;
            pendingStandardRewards = false;
            StartNextWave();
        }
    }
    private void ClearRewardButtons()
    {
        foreach (var btn in activeRewardButtons)
        {
            if (btn != null)
            {
                if (btn.GetComponent<AnomalyButtonUI>() != null)
                {
                    Destroy(btn);
                }
                else
                {
                    btn.SetActive(false);
                    inactiveRewardButtonPool.Add(btn);
                }
            }
        }
        activeRewardButtons.Clear();
    }
    public int GetCurrentWave() => currentWaveIndex + currentSequence.waveOffset;
}