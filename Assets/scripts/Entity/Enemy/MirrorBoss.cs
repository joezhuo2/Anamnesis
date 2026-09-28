using System.Collections.Generic;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    [RequireComponent(typeof(EnemyAttackHandler))]
    [RequireComponent(typeof(PlayerUpgradeManager))]
    public class MirrorBoss : MonoBehaviour
    {
        public bool copyAttacks = true;
        public bool copyUpgrades = true;
        [Tooltip("Max upgrades copied, picked at random. 0 = all")] public int maxUpgrades;

        private readonly List<PlayerUpgrade> mirrored = new();
        private readonly List<PlayerUpgrade> puBuf = new();
        private readonly List<AttackData> atkBuf = new();
        private EnemyAttackHandler eah;
        private PlayerUpgradeManager pum;

        private void Awake()
        {
            TryGetComponent(out eah);
            TryGetComponent(out pum);
        }

        private void Start() => Setup(GameObject.FindGameObjectWithTag("Player"));

        public void Setup(GameObject player)
        {
            ClearUpgrades();
            if (player == null) return;

            if (copyAttacks && eah != null && player.TryGetComponent<PlayerAttackHandler>(out var pah))
            {
                atkBuf.Clear();
                atkBuf.AddRange(pah.attacks);
                eah.SetAttacks(atkBuf);
                atkBuf.Clear();
            }

            if (!copyUpgrades || pum == null || !player.TryGetComponent<PlayerUpgradeManager>(out var ppum)) return;

            puBuf.Clear();
            for (int i = 0; i < ppum.activeUpgrades.Count; i++)
            {
                PlayerUpgrade u = ppum.activeUpgrades[i];
                if (u != null && !u.noMirror) puBuf.Add(u);
            }

            int n = maxUpgrades > 0 ? Mathf.Min(maxUpgrades, puBuf.Count) : puBuf.Count;
            for (int i = 0; i < n; i++)
            {
                int j = Random.Range(i, puBuf.Count);
                (puBuf[i], puBuf[j]) = (puBuf[j], puBuf[i]);

                pum.AddUpgrade(puBuf[i]);
                mirrored.Add(puBuf[i]);
            }

            puBuf.Clear();
        }

        private void ClearUpgrades()
        {
            if (pum != null)
                for (int i = mirrored.Count - 1; i >= 0; i--) pum.RemoveUpgrade(mirrored[i]);

            mirrored.Clear();
        }
    }
}
