# Wave Events Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A wave event system. A spawner rolls a random event (Slime Rain, Blessing Drop) during waves, announces it with a colored title and subtitle, and plays it out around the player.

**Architecture:** There is a new `CrystalFlux.Event` assembly. An abstract `WaveEventData` ScriptableObject has concrete subclasses that each implement `IEnumerator Run(WaveEventContext)`. A single `WaveEventSpawner` MonoBehaviour handles rolling, gating, announcing and coroutine lifetime. `WaveManager` gains an enemy reservation API, so staggered event spawns count toward the wave total and hold the wave open.

**Tech Stack:** Unity 6000.4, C# 9 (`LangVersion` 9.0), TextMeshPro rich text. There is no test framework: compile with `dotnet build` on the Unity-generated csproj files and playtest in the editor.

**Spec:** `docs/superpowers/specs/2026-10-10-wave-events-design.md`

## Global Constraints

- Work on branch `feat/event`. Make one small local commit per task. **Never push.**
- Do not edit GAME.md, TODO.md or CHANGELOG.md.
- Do not modify the external `com.crystalflux.core` package.
- Match the surrounding code style: Allman braces, `[Tooltip]` and `[Min]` on fields, no XML doc comments, minimal comments.
- C# 9 only. No file-scoped namespaces, no `required`, no raw strings.
- Don't `using System;` in files that also use `UnityEngine.Random`. Fully qualify `System.Func` instead.
- `*.csproj` files are gitignored. Never commit them.
- Unity is closed, so `.meta` files for new files and folders are generated when the user opens the editor. They're committed afterwards and are not part of this plan.

## Review Focus

1. **Event coroutine finishes synchronously** (zero spawns rolled, no player, empty list). The spawner must not get stuck "running". This is covered in Task 4 by the `isRunning` flag being set before `StartCoroutine` and cleared inside the routine.
2. **Wave force-ended or advanced mid Slime Rain.** Leftover reservations must not leak into the next wave. This is covered in Task 1 (`reservedEnemies = 0` on wave start) and Task 5 (abort when `CurrentWave` changes or no wave is active).
3. **Slime Rain must never pause.** If it did, pending reservations would keep the wave from ever ending (deadlock). This is covered in Task 5, which uses plain `WaitForSeconds` and never `ctx.Wait`.
4. **Twin Crowns, Stampede and extra-spawn waves** must not spawn regular enemies into reserved slots. This is covered in Task 1, where every `waveMaxTotalEnemies - totalSpawned` becomes `RemainingToSpawn`.
5. **Negative `radiusIncrease` below `minDistance`.** Spawns must still respect `minDistance`. This is covered in Task 3, where the `WaveEventContext` constructor clamps the max to at least the min.

---

### Task 1: WaveManager enemy reservation API

**Files:**
- Modify: `Assets/scripts/Wave/WaveManager.cs` (field block ~line 149-152, static block ~line 310-313)
- Modify: `Assets/scripts/Wave/WaveManager.Spawning.cs` (lines ~31, 82, 94, 120, 127, 138, 210, after `RegisterSplitEnemy` ~257)
- Modify: `Assets/scripts/Wave/UnlimitedWaveManager.cs` (lines ~62, 105, 119, 145, 152, 158)

**Interfaces:**
- Produces:
  - `public static int WaveManager.CurrentWave`, which is 0 when no wave is active.
  - `public static bool WaveManager.ReserveEnemies(int count)`.
  - `public static void WaveManager.ConsumeReservation()`.
  - `protected int RemainingToSpawn`.

- [ ] **Step 1: Add the field and statics to `WaveManager.cs`**

After `protected int waveMaxTotalEnemies = 0;` add:

```csharp
        protected int reservedEnemies = 0;
```

After `public static int WaveEnemyTotal => ...;` add:

```csharp
        public static int CurrentWave => WaveActive ? ActiveManager.GetCurrentWave() : 0;
```

- [ ] **Step 2: Edit `WaveManager.Spawning.cs`**

In `StartNextWave`, after `totalSpawned = 0;`:

```csharp
            reservedEnemies = 0;
```

In `WaveSpawnRoutine`:

```csharp
            while (RemainingToSpawn > 0)
```

replaces `while (totalSpawned < waveMaxTotalEnemies)`, and

```csharp
            while (currentEnemies.Count > 0 || reservedEnemies > 0)
```

replaces the end-phase `while (currentEnemies.Count > 0)`.

In `SpawnEnemies`, replace all three `waveMaxTotalEnemies - totalSpawned` with `RemainingToSpawn`:

```csharp
                int bosses = Twin != null ? RemainingToSpawn : 1;
```
```csharp
                int all = RemainingToSpawn;
```
```csharp
            int spawnCount = enableExtraSpawns ? Mathf.Min(Mathf.RoundToInt(GetCurrentWave() / 10) + 1, RemainingToSpawn) : 1;
```

In `StopSpawning`:

```csharp
            wm.waveMaxTotalEnemies = wm.totalSpawned + wm.reservedEnemies;
```

Directly after the `RegisterSplitEnemy` method, add:

```csharp
        protected int RemainingToSpawn => waveMaxTotalEnemies - totalSpawned - reservedEnemies;

        public static bool ReserveEnemies(int count)
        {
            WaveManager wm = ActiveManager;
            if (count <= 0 || wm == null || !wm.isWaveActive) return false;

            wm.reservedEnemies += count;
            wm.waveMaxTotalEnemies += count;
            wm.UpdateWaveText();
            return true;
        }

        public static void ConsumeReservation()
        {
            WaveManager wm = ActiveManager;
            if (wm == null || wm.reservedEnemies <= 0) return;

            wm.reservedEnemies--;
            wm.waveMaxTotalEnemies--;
            wm.UpdateWaveText();
        }
```

`ConsumeReservation` handles success and failure the same way. On success, `RegisterSplitEnemy` (called inside `SpawnAmbushEnemy`) has already added +1 to `totalSpawned` and `waveMaxTotalEnemies`, so the reserved slot is handed back. On failure, the slot is simply dropped. Either way `RemainingToSpawn` stays unchanged.

- [ ] **Step 3: Edit `UnlimitedWaveManager.cs`**

In `StartNextWave`, after `totalSpawned = 0;`:

```csharp
            reservedEnemies = 0;
```

In `WaveSpawnRoutine`:

```csharp
            while (RemainingToSpawn > 0)
```
```csharp
            while (currentEnemies.Count > 0 || reservedEnemies > 0)
```

In `SpawnEnemies`:

```csharp
                int bosses = Twin != null ? RemainingToSpawn : 1;
```
```csharp
                int all = RemainingToSpawn;
```
```csharp
            int spawnCount = enableExtraSpawns ? Mathf.Min(Mathf.RoundToInt(wave / 10) + 1, RemainingToSpawn) : 1;
```

- [ ] **Step 4: Verify that nothing still uses the old math**

Run: `grep -rn "waveMaxTotalEnemies - totalSpawned\|totalSpawned < waveMaxTotalEnemies" Assets/scripts`
Expected: no output.

- [ ] **Step 5: Compile**

Run: `dotnet build CrystalFlux.Wave.csproj -nologo -v q 2>&1 | grep -E "error|Error\(s\)"`
Expected: `0 Error(s)`

- [ ] **Step 6: Commit**

```bash
git add Assets/scripts/Wave/WaveManager.cs Assets/scripts/Wave/WaveManager.Spawning.cs Assets/scripts/Wave/UnlimitedWaveManager.cs
git commit -m "feat(Wave): enemy reservation API for staggered event spawns"
```

---

### Task 2: Collectible: public SpawnAt and shared weighted pick

**Files:**
- Modify: `Assets/scripts/Collectible/CollectibleSpawner.cs` (`SpawnAt`)
- Modify: `Assets/scripts/Collectible/CollectibleData.cs` (`PickReward`)

**Interfaces:**
- Produces:
  - `public void CollectibleSpawner.SpawnAt(CollectibleData d, Vector2 pos)`.
  - `public static CollectibleData CollectibleData.PickWeighted(List<SpawnerBoxReward> list, bool ironman)`.

- [ ] **Step 1: Make `SpawnAt` public and guard against null**

Replace the method header and first lines in `CollectibleSpawner.cs`:

```csharp
        public void SpawnAt(CollectibleData d, Vector2 pos)
        {
            if (prefab == null || d == null) return;

            Collectible c = PrefabPool.Acquire(prefab, null);
```

The rest of the body is unchanged.

- [ ] **Step 2: Extract `PickWeighted` in `CollectibleData.cs`**

Replace the whole `PickReward` method with:

```csharp
        public CollectibleData PickReward(bool ironman) => PickWeighted(rewards, ironman);

        public static CollectibleData PickWeighted(List<SpawnerBoxReward> list, bool ironman)
        {
            if (list == null || list.Count == 0) return null;

            float total = 0f;
            for (int i = 0; i < list.Count; i++)
                if (IsValidReward(list[i], ironman)) total += list[i].weight;

            if (total <= 0f) return null;

            float roll = Random.value * total;
            for (int i = 0; i < list.Count; i++)
            {
                SpawnerBoxReward r = list[i];
                if (!IsValidReward(r, ironman)) continue;

                roll -= r.weight;
                if (roll <= 0f) return r.data;
            }

            for (int i = list.Count - 1; i >= 0; i--)
                if (IsValidReward(list[i], ironman)) return list[i].data;

            return null;
        }
```

- [ ] **Step 3: Compile**

Run: `dotnet build CrystalFlux.Collectible.csproj -nologo -v q 2>&1 | grep -E "error|Error\(s\)"`
Expected: `0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add Assets/scripts/Collectible/CollectibleSpawner.cs Assets/scripts/Collectible/CollectibleData.cs
git commit -m "feat(Collectible): public SpawnAt and shared weighted reward pick"
```

---

### Task 3: Event assembly, WaveEventData base, WaveEventContext

**Files:**
- Create: `Assets/scripts/Event/Event.asmdef`
- Create: `Assets/scripts/Event/WaveEventData.cs`
- Create: `Assets/scripts/Event/WaveEventContext.cs`
- Create (gitignored, compile check only): `CrystalFlux.Event.csproj`

**Interfaces:**
- Consumes: `WaveManager.WaveActive`, `WaveManager.DroughtActive`.
- Produces:
  - `abstract class WaveEventData : ScriptableObject`, with:
    - Fields: `title`, `subtitle`, `titleColor`, `titleDurationOverride`, `chance`, `radiusIncrease`, `minMode`, `minWave`, `maxWave`, `allowOnIronman`.
    - `bool IsEligible(int wave, int tier, bool ironman)`.
    - `abstract IEnumerator Run(WaveEventContext ctx)`.
    - `protected static int RollCount(int min, int max)`.
    - `protected static float RollInterval(float min, float max)`.
  - `class WaveEventContext`, with:
    - `WaveEventContext(System.Func<GameObject> player, float minDistance, float maxDistance)`.
    - `static bool Paused`.
    - `IEnumerator Wait(float seconds)`.
    - `IEnumerator WaitWhilePaused()`.
    - `bool TryGetSpawnPoint(out Vector2 pos)`.

- [ ] **Step 1: Create `Event.asmdef`**

```json
{
    "name": "CrystalFlux.Event",
    "rootNamespace": "CrystalFlux.EventSystem",
    "references": [
        "CrystalFlux.Core",
        "CrystalFlux.Wave",
        "CrystalFlux.Collectible",
        "CrystalFlux.Settings"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: Create `WaveEventData.cs`**

```csharp
using System.Collections;
using UnityEngine;

namespace CrystalFlux.EventSystem
{
    public abstract class WaveEventData : ScriptableObject
    {
        [Header("Display")]
        public string title;
        public string subtitle;
        public Color titleColor = Color.white;
        [Tooltip("Total title on-screen time in seconds. 0 = use the spawner's titleDuration")] [Min(0f)] public float titleDurationOverride;

        [Header("Spawning")]
        [Tooltip("Chance (0-100) to fire on each spawner roll")] [Range(0f, 100f)] public float chance = 10f;
        [Tooltip("Added to the spawner's baseRadius. Can be negative")] public float radiusIncrease;

        [Header("Gating")]
        [Tooltip("Minimum mode tier required for this event. 0 = Simple, 1 = Expert, 2 = Master")] [Min(0)] public int minMode;
        [Tooltip("First wave this event can fire on. 0 = no lower bound")] [Min(0)] public int minWave;
        [Tooltip("Last wave this event can fire on. 0 = no upper bound")] [Min(0)] public int maxWave;
        public bool allowOnIronman = true;

        public bool IsEligible(int wave, int tier, bool ironman)
        {
            if (chance <= 0f || tier < minMode) return false;
            if (minWave > 0 && wave < minWave) return false;
            if (maxWave > 0 && wave > maxWave) return false;
            return allowOnIronman || !ironman;
        }

        public abstract IEnumerator Run(WaveEventContext ctx);

        protected static int RollCount(int min, int max) => Random.Range(Mathf.Max(0, Mathf.Min(min, max)), Mathf.Max(min, max) + 1);

        protected static float RollInterval(float min, float max) => Mathf.Max(0f, Random.Range(Mathf.Min(min, max), Mathf.Max(min, max)));
    }
}
```

- [ ] **Step 3: Create `WaveEventContext.cs`**

```csharp
using System.Collections;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.EventSystem
{
    public class WaveEventContext
    {
        private readonly System.Func<GameObject> player;

        public float MinDistance { get; }
        public float MaxDistance { get; }

        public WaveEventContext(System.Func<GameObject> player, float minDistance, float maxDistance)
        {
            this.player = player;
            MinDistance = Mathf.Max(0f, minDistance);
            MaxDistance = Mathf.Max(MinDistance, maxDistance);
        }

        public static bool Paused => !WaveManager.WaveActive || WaveManager.DroughtActive;

        public IEnumerator WaitWhilePaused()
        {
            while (Paused) yield return null;
        }

        public IEnumerator Wait(float seconds)
        {
            float remaining = seconds;
            while (remaining > 0f)
            {
                yield return null;
                if (!Paused) remaining -= Time.deltaTime;
            }
        }

        public bool TryGetSpawnPoint(out Vector2 pos)
        {
            GameObject p = player?.Invoke();
            if (p == null)
            {
                pos = default;
                return false;
            }

            float dist = Random.Range(MinDistance, MaxDistance);
            float ang = Random.Range(0f, Mathf.PI * 2f);
            pos = (Vector2)p.transform.position + (new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * dist);
            return true;
        }
    }
}
```

- [ ] **Step 4: Generate a temporary csproj for compile checks**

Unity is closed, so there is no csproj for the new assembly yet. Derive one from the Collectible csproj. It is gitignored, and Unity overwrites it on its next open.

```bash
sed -e 's#<RootNamespace>CrystalFlux.CollectibleSystem<#<RootNamespace>CrystalFlux.EventSystem<#' \
    -e 's#<AssemblyName>CrystalFlux.Collectible<#<AssemblyName>CrystalFlux.Event<#' \
    -e 's#<Compile Include="Assets\\scripts\\Collectible\\CollectibleSpawner.cs" />#<Compile Include="Assets\\scripts\\Event\\*.cs" />#' \
    -e '/<Compile Include="Assets\\scripts\\Collectible\\/d' \
    -e 's#<ProjectReference Include="CrystalFlux.Pooling.csproj" />#<ProjectReference Include="CrystalFlux.Collectible.csproj" />#' \
    CrystalFlux.Collectible.csproj > CrystalFlux.Event.csproj
grep -n "Compile Include\|AssemblyName\|ProjectReference" CrystalFlux.Event.csproj
```

Expected: one `Assets\scripts\Event\*.cs` compile line, `AssemblyName` set to `CrystalFlux.Event`, and a `CrystalFlux.Collectible.csproj` reference.

- [ ] **Step 5: Compile**

Run: `dotnet build CrystalFlux.Event.csproj -nologo -v q 2>&1 | grep -E "error|Error\(s\)"`
Expected: `0 Error(s)`

- [ ] **Step 6: Commit**

```bash
git add Assets/scripts/Event/Event.asmdef Assets/scripts/Event/WaveEventData.cs Assets/scripts/Event/WaveEventContext.cs
git commit -m "feat(Event): event assembly, WaveEventData base and context"
```

---

### Task 4: WaveEventSpawner

**Files:**
- Create: `Assets/scripts/Event/WaveEventSpawner.cs`

**Interfaces:**
- Consumes:
  - `WaveEventData.IsEligible`, `WaveEventData.Run`.
  - `WaveEventContext` constructor and `WaveEventContext.Paused`.
  - `WaveManager.CurrentWave`, `RunMode.Tier`, `IronmanSelector.Enabled`, `IAnnouncer.Current`.
- Produces: a `WaveEventSpawner` MonoBehaviour with public fields `events`, `rollInterval`, `minTimeBetweenEvents`, `baseRadius`, `minDistance` and `titleDuration`.

- [ ] **Step 1: Create `WaveEventSpawner.cs`**

```csharp
using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.EventSystem
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

        private void OnDisable()
        {
            if (routine != null) StopCoroutine(routine);

            routine = null;
            isRunning = false;
            rollTimer = 0f;
            sinceLastEvent = float.PositiveInfinity;
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
            routine = StartCoroutine(RunEvent(pick));
        }

        private WaveEventData Roll()
        {
            if (events == null || events.Count == 0) return null;

            int wave = WaveManager.CurrentWave;
            int tier = RunMode.Tier;
            bool ironman = IronmanSelector.Enabled;

            rollBuffer.Clear();
            for (int i = 0; i < events.Count; i++)
            {
                WaveEventData e = events[i];
                if (e != null && e.IsEligible(wave, tier, ironman)) rollBuffer.Add(e);
            }

            Shuffle(rollBuffer);

            WaveEventData pick = null;
            for (int i = 0; i < rollBuffer.Count; i++)
            {
                if (Random.value * 100f >= rollBuffer[i].chance) continue;

                pick = rollBuffer[i];
                break;
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

        private static void Shuffle(List<WaveEventData> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
```

`isRunning` is set **before** `StartCoroutine` and cleared inside `RunEvent`. A `Run` that finishes synchronously (for example, zero spawns rolled) therefore still clears it. If the routine handle is stale afterwards, that is harmless.

- [ ] **Step 2: Compile**

Run: `dotnet build CrystalFlux.Event.csproj -nologo -v q 2>&1 | grep -E "error|Error\(s\)"`
Expected: `0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add Assets/scripts/Event/WaveEventSpawner.cs
git commit -m "feat(Event): WaveEventSpawner rolls, gates and announces events"
```

---

### Task 5: SpawnEnemyEventData (Slime Rain)

**Files:**
- Create: `Assets/scripts/Event/SpawnEnemyEventData.cs`

**Interfaces:**
- Consumes:
  - `WaveEventData.RollCount`, `WaveEventData.RollInterval`.
  - `WaveEventContext.TryGetSpawnPoint`.
  - `WaveManager.ReserveEnemies`, `WaveManager.ConsumeReservation`, `WaveManager.SpawnAmbushEnemy(GameObject, Vector2, float, int)`.
  - `WaveManager.CurrentWave`, `WaveManager.WaveActive`.
- Produces: the asset menu entry `Data/Events/Spawn Enemy`.

- [ ] **Step 1: Create `SpawnEnemyEventData.cs`**

```csharp
using System.Collections;
using System.Collections.Generic;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.EventSystem
{
    [CreateAssetMenu(fileName = "Spawn Enemy Event", menuName = "Data/Events/Spawn Enemy")]
    public class SpawnEnemyEventData : WaveEventData
    {
        [Header("Spawn Enemy")]
        [Tooltip("Picked uniformly for each spawn")] public List<GameObject> enemies = new();
        [Min(0)] public int minSpawns = 6;
        [Min(0)] public int maxSpawns = 10;
        [Tooltip("Seconds between consecutive spawns")] [Min(0f)] public float minInterval = 0.2f;
        [Min(0f)] public float maxInterval = 0.6f;
        [Tooltip("Added to the current wave enemy level")] public int levelBonus;

        public override IEnumerator Run(WaveEventContext ctx)
        {
            int n = RollCount(minSpawns, maxSpawns);
            if (n <= 0 || PickEnemy() == null || !WaveManager.ReserveEnemies(n)) yield break;

            int wave = WaveManager.CurrentWave;

            for (int i = 0; i < n; i++)
            {
                // Never pause here: pending reservations hold the wave open, so pausing would deadlock it.
                if (i > 0)
                {
                    float wait = RollInterval(minInterval, maxInterval);
                    if (wait > 0f) yield return new WaitForSeconds(wait);
                }

                if (!WaveManager.WaveActive || WaveManager.CurrentWave != wave) yield break;

                GameObject prefab = PickEnemy();
                if (prefab != null && ctx.TryGetSpawnPoint(out Vector2 pos))
                    WaveManager.SpawnAmbushEnemy(prefab, pos, 0f, levelBonus);

                WaveManager.ConsumeReservation();
            }
        }

        private GameObject PickEnemy()
        {
            if (enemies == null) return null;

            int count = 0;
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i] != null) count++;

            if (count == 0) return null;

            int k = Random.Range(0, count);
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null) continue;
                if (k-- == 0) return enemies[i];
            }

            return null;
        }
    }
}
```

- [ ] **Step 2: Compile**

Run: `dotnet build CrystalFlux.Event.csproj -nologo -v q 2>&1 | grep -E "error|Error\(s\)"`
Expected: `0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add Assets/scripts/Event/SpawnEnemyEventData.cs
git commit -m "feat(Event): SpawnEnemyEventData for staggered enemy spawns (Slime Rain)"
```

---

### Task 6: BlessingDropEventData

**Files:**
- Create: `Assets/scripts/Event/BlessingDropEventData.cs`

**Interfaces:**
- Consumes:
  - `WaveEventData.RollCount`, `WaveEventData.RollInterval`.
  - `WaveEventContext.Wait`, `WaveEventContext.WaitWhilePaused`, `WaveEventContext.TryGetSpawnPoint`.
  - `CollectibleSpawner.Active`, `CollectibleSpawner.SpawnAt`.
  - `CollectibleData.PickWeighted`, `SpawnerBoxReward`, `IronmanSelector.Enabled`.
- Produces: the asset menu entry `Data/Events/Blessing Drop`.

- [ ] **Step 1: Create `BlessingDropEventData.cs`**

```csharp
using System.Collections;
using System.Collections.Generic;
using CrystalFlux.CollectibleSystem;
using CrystalFlux.WaveSystem;
using UnityEngine;

namespace CrystalFlux.EventSystem
{
    [CreateAssetMenu(fileName = "Blessing Drop Event", menuName = "Data/Events/Blessing Drop")]
    public class BlessingDropEventData : WaveEventData
    {
        [Header("Blessing Drop")]
        [Tooltip("Weighted collectible pool. Spawner boxes are never dropped")] public List<SpawnerBoxReward> rewards = new();
        [Min(0)] public int minDrops = 3;
        [Min(0)] public int maxDrops = 6;
        [Tooltip("Seconds between consecutive drops. Paused between waves and during Drought")] [Min(0f)] public float minInterval = 0.5f;
        [Min(0f)] public float maxInterval = 1.5f;

        public override IEnumerator Run(WaveEventContext ctx)
        {
            int n = RollCount(minDrops, maxDrops);

            for (int i = 0; i < n; i++)
            {
                if (i > 0) yield return ctx.Wait(RollInterval(minInterval, maxInterval));
                yield return ctx.WaitWhilePaused();

                CollectibleSpawner cs = CollectibleSpawner.Active;
                if (cs == null) yield break;

                CollectibleData d = CollectibleData.PickWeighted(rewards, IronmanSelector.Enabled);
                if (d != null && ctx.TryGetSpawnPoint(out Vector2 pos)) cs.SpawnAt(d, pos);
            }
        }
    }
}
```

- [ ] **Step 2: Compile**

Run: `dotnet build CrystalFlux.Event.csproj -nologo -v q 2>&1 | grep -E "error|Error\(s\)"`
Expected: `0 Error(s)`

- [ ] **Step 3: Commit**

```bash
git add Assets/scripts/Event/BlessingDropEventData.cs
git commit -m "feat(Event): BlessingDropEventData for staggered collectible drops"
```

---

### Task 7: Editor setup and playtest (user)

These steps happen in the Unity editor and are done by the user. They are not automated.

- [ ] **Step 1:** Open Unity and let it import. Check the Console for compile errors.
- [ ] **Step 2:** Commit the generated `.meta` files for `Assets/scripts/Event/` and its files.
- [ ] **Step 3:** Create `Assets/data/Events/Slime Rain.asset` with Create > Data/Events/Spawn Enemy:
  - `enemies` = `Slime1`, `Slime_frost`, `Slime_magma`.
  - Set the title, subtitle, color and counts.
- [ ] **Step 4:** Create `Assets/data/Events/Blessing Drop.asset` with Create > Data/Events/Blessing Drop:
  - `rewards` = weighted entries for Health, Mana, Stamina, Gold, XP and so on.
- [ ] **Step 5:** In `New.unity`, add a `WaveEventSpawner` GameObject and list both assets. For testing, temporarily use a low `rollInterval` and `minTimeBetweenEvents`, and high chances.
- [ ] **Step 6:** Playtest against the spec's Verification list. In particular, check that the title color is visible during the fades (the TMP `<color>` alpha interaction).
- [ ] **Step 7:** Commit the assets and the scene.
