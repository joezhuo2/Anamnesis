using System.Collections.Generic;
using UnityEngine;

public static class WaveQuality
{
    public static float CalculateQualityBoost(int wave)
    {
        if (wave == 1) return 0.5f;
        if (wave <= 30) return (wave / 5f) * 0.1f;
        if (wave <= 105) return 0.6f + (((wave - 30) / 10f) * 0.01f);
        return 1.5f;
    }
    public static RarityData GetWeightedRandomRarity(int wave, List<RarityData> rarityData, float qual = 0f)
    {
        float qualityBoost = CalculateQualityBoost(wave) + qual;

        float totalWeight = 0;
        for (int i = 0; i < rarityData.Count; i++)
        {
            float scalingFactor = 1f + (qualityBoost * i);
            totalWeight += rarityData[i].weight * scalingFactor;
        }

        float roll = Random.Range(0f, totalWeight);
        float weightSum = 0;

        for (int i = 0; i < rarityData.Count; i++)
        {
            float scalingFactor = 1f + (qualityBoost * i);
            weightSum += rarityData[i].weight * scalingFactor;

            if (roll <= weightSum) return rarityData[i];
        }

        return rarityData[^1];
    }
}