using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.EntitySystem;
using UnityEngine;

namespace CrystalFlux.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "se_mirage", menuName = "Status Effects/Buff/Ethereal Mirage")]
    public class EtherealMirage : StatusEffect
    {
        [Header("Clones")]
        [Tooltip("Optional prefab with MirageClone + EntityHealth + trigger collider. Null = built at runtime from the base entity")]
        public GameObject clonePrefab;
        [Min(1)] public int cloneCount = 2;
        public float spawnRadius = 1.5f;
        [Tooltip("Angle in degrees of the first clone, others are spaced evenly")] public float startAngle = 90f;
        [Range(0f, 1f)] public float opacity = 0.5f;
        public bool copyMovement = true;
        public bool copyAttacks = true;

        [Header("Stat Sharing")]
        [Tooltip("Fraction of flat stats (attack, int, max hp, armor, regen...) clones receive. % stats copy at 100% unless overridden")]
        public float statShare = 0.5f;
        public List<StatShare> shareOverrides = new();

        [Header("Base Rewards")]
        [Tooltip("Applied to the base for each living clone")] public List<StatBuff> perCloneBuffs = new();
        [Tooltip("Applied to the base each time a clone is killed")] public List<StatusEffect> onCloneDeathEffects = new();

        private readonly List<MirageClone> clones = new();
        private readonly List<bool> buffed = new();

        private void Reset() => isBuff = true;

        protected override void ResetRuntime()
        {
            clones.Clear();
            buffed.Clear();
        }

        public override void OnApply() => FillSlots();
        public override void OnStack() => FillSlots();
        public override void OnExpire() => DespawnAll();

        private void FillSlots()
        {
            if (target == null) return;

            int n = Mathf.Max(1, cloneCount);
            while (clones.Count < n)
            {
                clones.Add(null);
                buffed.Add(false);
            }

            for (int i = 0; i < n; i++)
            {
                if (clones[i] != null) continue;

                ClearSlot(i, false);
                clones[i] = SpawnClone(i, n);
                if (clones[i] == null) continue;

                SetBuffs(true);
                buffed[i] = true;
            }
        }

        private MirageClone SpawnClone(int i, int n)
        {
            float ang = (startAngle + (i * 360f / n)) * Mathf.Deg2Rad;
            Vector2 off = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * spawnRadius;
            Vector2 pos = (Vector2)target.transform.position + off;

            GameObject go = clonePrefab != null
                ? Instantiate(clonePrefab, pos, Quaternion.identity)
                : MirageClone.CreateDefault(target, pos);

            if (go == null) return null;
            if (!go.TryGetComponent<MirageClone>(out var mc)) mc = go.AddComponent<MirageClone>();

            mc.Setup(target, off, statShare, shareOverrides, opacity, copyMovement, copyAttacks);

            if (go.TryGetComponent<IDamageable>(out var dmg)) dmg.OnDeath += OnCloneDeath;

            return mc;
        }

        private void OnCloneDeath(GameObject go)
        {
            if (go.TryGetComponent<IDamageable>(out var dmg)) dmg.OnDeath -= OnCloneDeath;

            int i = -1;
            for (int j = 0; j < clones.Count; j++)
            {
                if (clones[j] != null && clones[j].gameObject == go)
                {
                    i = j;
                    break;
                }
            }
            if (i < 0) return;

            clones[i] = null;
            if (buffed[i])
            {
                buffed[i] = false;
                SetBuffs(false);
            }

            if (target == null || onCloneDeathEffects.Count == 0) return;
            if (!target.TryGetComponent<IStatusEffectReceiver>(out var sem)) return;

            for (int j = 0; j < onCloneDeathEffects.Count; j++)
                if (onCloneDeathEffects[j] != null) sem.Apply(onCloneDeathEffects[j], target);
        }

        private void ClearSlot(int i, bool destroy)
        {
            MirageClone mc = clones[i];
            clones[i] = null;

            if (buffed[i])
            {
                buffed[i] = false;
                SetBuffs(false);
            }

            if (mc == null) return;
            if (mc.TryGetComponent<IDamageable>(out var dmg)) dmg.OnDeath -= OnCloneDeath;
            if (destroy) Destroy(mc.gameObject);
        }

        private void DespawnAll()
        {
            for (int i = 0; i < clones.Count; i++) ClearSlot(i, true);

            clones.Clear();
            buffed.Clear();
        }

        private void SetBuffs(bool adding)
        {
            if (perCloneBuffs.Count == 0 || target == null) return;
            if (!target.TryGetComponent<IStatProvider>(out var esm)) return;

            for (int i = 0; i < perCloneBuffs.Count; i++) esm.AddStat(perCloneBuffs[i], adding);
        }
    }
}
