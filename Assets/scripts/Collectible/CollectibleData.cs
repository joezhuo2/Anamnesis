using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.CollectibleSystem
{
    public enum CollectibleType { Heal, Xp, Stamina, Mana, Gold, SkillPoints, Rerolls }

    [CreateAssetMenu(fileName = "Collectible", menuName = "Data/Collectible")]
    public class CollectibleData : ScriptableObject
    {
        [Header("Basic")]
        public Sprite sprite;
        public CollectibleType type = CollectibleType.Gold;
        public Color lightColor = Color.white;

        [Header("Value")]
        public int minVal = 1;
        public int maxVal = 1;

        [Header("Spawning")]
        [Range(0f, 100f)] public float chance = 5f;
        public float cooldown = 10f;
        public float maxTime = 20f;

        public int RollValue() => Random.Range(Mathf.Min(minVal, maxVal), Mathf.Max(minVal, maxVal) + 1);

        public string BuildDesc(int v)
        {
            return type switch
            {
                CollectibleType.Heal => $"+{v}% HP",
                CollectibleType.Xp => $"+{v}% XP",
                CollectibleType.Stamina => $"+{v}% Stamina",
                CollectibleType.Mana => $"+{v}% Mana",
                CollectibleType.Gold => $"+{v} Gold",
                CollectibleType.SkillPoints => $"+{v} Skill Point{(v == 1 ? "" : "s")}",
                CollectibleType.Rerolls => $"+{v} Reroll{(v == 1 ? "" : "s")}",
                _ => $"+{v}"
            };
        }

        public bool Apply(GameObject player, int v)
        {
            if (player == null || v <= 0) return false;

            switch (type)
            {
                case CollectibleType.Heal:
                    return ApplyHeal(player, v);
                case CollectibleType.Xp:
                    return ApplyXp(player, v);
                case CollectibleType.Stamina:
                    return ApplyResource(player, ResourceType.Stamina, StatType.EffMaxStamina, v);
                case CollectibleType.Mana:
                    return ApplyResource(player, ResourceType.Mana, StatType.EffMaxMana, v);
                case CollectibleType.Gold:
                    return player.TryGetComponent<ICurrencyHolder>(out var ich) && ich.AddCurrency(v);
                case CollectibleType.SkillPoints:
                    if (!player.TryGetComponent<ISkillPointHolder>(out var sph)) return false;
                    sph.AddSkillPoints(v);
                    return true;
                case CollectibleType.Rerolls:
                    return WaveManager.GrantRerolls(v);
                default:
                    return false;
            }
        }

        private static bool ApplyXp(GameObject player, int pct)
        {
            if (!player.TryGetComponent<PlayerLevel>(out var pl)) return false;
            if (!player.TryGetComponent<IStatProvider>(out var esm)) return false;

            float amount = esm.GetStat(StatType.XpReq) * pct * 0.01f;
            if (amount <= 0f) return false;

            pl.GainExp(amount);
            return true;
        }

        private static bool ApplyHeal(GameObject player, int pct)
        {
            if (!player.TryGetComponent<EntityHealth>(out var eh)) return false;
            if (!player.TryGetComponent<IStatProvider>(out var esm)) return false;

            float amount = esm.GetStat(StatType.EffMaxHp) * pct * 0.01f;
            if (amount <= 0f) return false;

            return eh.ChangeHealth(amount, false);
        }

        private static bool ApplyResource(GameObject player, ResourceType rt, StatType maxStat, int pct)
        {
            if (!player.TryGetComponent<IResourcePool>(out var rp)) return false;
            if (!player.TryGetComponent<IStatProvider>(out var esm)) return false;

            float amount = esm.GetStat(maxStat) * pct * 0.01f;
            if (amount <= 0f) return false;

            return rp.TryGain(rt, amount);
        }
    }
}
