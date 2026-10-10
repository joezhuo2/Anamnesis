using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.WaveEventSystem
{
    public class WaveEventSpawner : MonoBehaviour
    {
        [Header("Events")]
        public List<WaveEventData> events = new();

        [Header("Timing")]
        [Tooltip("Seconds between event rolls while a wave is active")] [Min(0.1f)] public float rollInterval = 10f;
        [Tooltip("Minimum seconds after an event ends before the next roll")] [Min(0f)] public float minTimeBetweenEvents = 30f;

        [Header("Placement")]
        [Tooltip("Outer spawn radius around the player, before each event's radiusIncrease")] public float baseRadius = 8f;
        [Tooltip("Nothing spawns closer to the player than this")] [Min(0f)] public float minDistance = 3f;

        [Header("Announcement")]
        [Tooltip("Total title/subtitle time: 20% fade in, 60% hold, 20% fade out. Events can override")] [Min(0f)] public float titleDuration = 3f;

        private readonly List<WaveEventData> rollBuffer = new();
        private Coroutine routine;
        private bool isRunning;
        private float rollTimer;
        private float sinceLastEvent = float.PositiveInfinity;
        private GameObject player;
        private WaveEventData lastEvent;

        private void OnDisable()
        {
            if (routine != null) StopCoroutine(routine);

            routine = null;
            isRunning = false;
            rollTimer = 0f;
            sinceLastEvent = float.PositiveInfinity;
            lastEvent = null;
        }

        private void Update()
        {
            if (Time.timeScale == 0f || isRunning || WaveEventContext.Paused) return;

            float dt = Time.deltaTime;
            sinceLastEvent += dt;
            if (sinceLastEvent < minTimeBetweenEvents) return;

            rollTimer += dt;
            if (rollTimer < rollInterval) return;
            rollTimer -= rollInterval;

            WaveEventData pick = Roll();
            if (pick == null) return;

            isRunning = true;
            lastEvent = pick;
            routine = StartCoroutine(RunEvent(pick));
        }

        private WaveEventData Roll()
        {
            if (events == null || events.Count == 0) return null;

            int wave = WaveManager.CurrentWave;
            int tier = RunMode.Tier;
            bool ironman = IronmanSelector.Enabled;
            float chanceMult = WaveManager.Difficulty.eventChanceMult;

            rollBuffer.Clear();
            bool repeatOnly = true;
            for (int i = 0; i < events.Count; i++)
            {
                WaveEventData e = events[i];
                if (e == null || !e.IsEligible(wave, tier, ironman)) continue;

                rollBuffer.Add(e);
                if (e != lastEvent) repeatOnly = false;
            }

            // Never the same event twice in a row, unless it's the only one eligible.
            if (!repeatOnly) rollBuffer.Remove(lastEvent);

            float totalWeight = 0f;
            for (int i = rollBuffer.Count - 1; i >= 0; i--)
            {
                if (Random.value * 100f >= Mathf.Min(100f, rollBuffer[i].chance * chanceMult)) rollBuffer.RemoveAt(i);
                else totalWeight += rollBuffer[i].weight;
            }

            WaveEventData pick = null;
            float r = Random.value * totalWeight;
            for (int i = 0; i < rollBuffer.Count; i++)
            {
                pick = rollBuffer[i];
                r -= pick.weight;
                if (r < 0f) break;
            }

            rollBuffer.Clear();
            return pick;
        }

        private IEnumerator RunEvent(WaveEventData e)
        {
            Announce(e);

            WaveEventContext ctx = new(Player, minDistance, baseRadius + e.radiusIncrease);
            yield return e.Run(ctx);

            routine = null;
            isRunning = false;
            rollTimer = 0f;
            sinceLastEvent = 0f;
        }

        private void Announce(WaveEventData e)
        {
            IAnnouncer a = IAnnouncer.Current;
            if (a == null) return;

            float total = e.titleDurationOverride > 0f ? e.titleDurationOverride : titleDuration;
            float hold = total * 0.6f;
            float fade = total * 0.2f;
            string hex = ColorUtility.ToHtmlStringRGB(e.titleColor);

            if (!string.IsNullOrEmpty(e.title)) a.SetTitleForDuration($"<color=#{hex}>{e.title}</color>", hold, fade, fade);
            if (!string.IsNullOrEmpty(e.subtitle)) a.SetSubtitleForDuration($"<color=#{hex}>{e.subtitle}</color>", hold, fade, fade);
        }

        private GameObject Player()
        {
            if (player == null) player = GameObject.FindWithTag("Player");
            return player;
        }
    }
}
