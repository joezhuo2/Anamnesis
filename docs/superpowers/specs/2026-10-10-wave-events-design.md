# Wave Events — Design

Date: 2026-10-10
Branch: `feat/event` (small local commits, no pushes)

## Goal

Random events that can fire during a wave (TODO.md "[6] wave events"). A spawner rolls on an
interval; when an event fires it announces itself with a title/subtitle and then plays out
around the player. First two events: **Slime Rain** (enemy spawns) and **Blessing Drop**
(collectible spawns).

## Scope

In:
- `WaveEventSpawner` MonoBehaviour, `WaveEventData` ScriptableObject hierarchy, two concrete event types.
- `WaveManager` enemy reservation API so staggered event spawns hold the wave open.
- `CollectibleSpawner.SpawnAt` made public.

Out (for now):
- GAME.md / TODO.md / CHANGELOG.md updates.
- Spawn telegraphs or "rain" visuals.
- Changes to the external CrystalFlux-Core package.
- Automated tests (verified by playtesting in the editor).

## Assembly

New folder `Assets/scripts/Event/` with `Event.asmdef`:
- name `CrystalFlux.Event`, rootNamespace `CrystalFlux.WaveEventSystem`
- references: `CrystalFlux.Core`, `CrystalFlux.Wave`, `CrystalFlux.Collectible`, `CrystalFlux.Settings`

## Data

### `WaveEventData` (abstract ScriptableObject)

| Field | Type | Notes |
|---|---|---|
| `title` | string | Shown on event start |
| `subtitle` | string | Shown on event start |
| `titleColor` | Color | Default white. Applied with a TMP `<color=#RRGGBB>` tag on title and subtitle |
| `titleDurationOverride` | float | 0 = use the spawner's `titleDuration` |
| `chance` | float, Range(0,100) | Percent chance per roll |
| `radiusIncrease` | float | Added to the spawner's `baseRadius`; may be negative |
| `minMode` | int, Min(0) | Same semantics as `CollectibleData.minMode` (`RunMode.Tier`) |
| `minWave` | int | 0 = no lower bound |
| `maxWave` | int | 0 = no upper bound |
| `allowOnIronman` | bool | Default true. When false the event is skipped if Ironman is on |

Methods:
- `bool IsEligible(int wave, int tier, bool ironman)`: chance > 0, tier >= minMode, wave within [minWave, maxWave] (0 = unbounded), and not (ironman && !allowOnIronman).
- `abstract IEnumerator Run(WaveEventContext ctx)`: plays the event. The spawner treats the event as running until the coroutine finishes.

The title text is fixed. It never includes a spawn count.

### `SpawnEnemyEventData : WaveEventData` (menu `Data/Events/Spawn Enemy`)

| Field | Type | Notes |
|---|---|---|
| `enemies` | List<GameObject> | Picked uniformly per spawn; null entries skipped |
| `minSpawns` / `maxSpawns` | int | Inclusive total spawn count roll |
| `minInterval` / `maxInterval` | float | Seconds between consecutive spawns |
| `levelBonus` | int | Added to the current enemy level (via `SpawnAmbushEnemy`) |

**Slime Rain** is one asset of this type that lists `Slime1`, `Slime_frost` and `Slime_magma`.

### `BlessingDropEventData : WaveEventData` (menu `Data/Events/Blessing Drop`)

| Field | Type | Notes |
|---|---|---|
| `rewards` | List<SpawnerBoxReward> | Reuses the existing weighted `{ CollectibleData data; float weight; }` |
| `minDrops` / `maxDrops` | int | Inclusive total drop count roll |
| `minInterval` / `maxInterval` | float | Seconds between consecutive drops |

Reward picking skips null entries, weight <= 0, `SpawnerBox` types, entries above `RunMode.Tier`,
and `Rerolls` when Ironman is on. These are the same rules as `CollectibleData.IsValidReward`.

## Runtime

### `WaveEventSpawner` (MonoBehaviour, one in the scene)

| Field | Default | Notes |
|---|---|---|
| `events` | — | List of `WaveEventData` |
| `rollInterval` | 10 | Seconds between rolls |
| `baseRadius` | 8 | Outer spawn radius before `radiusIncrease` |
| `minDistance` | 3 | Inner radius; nothing spawns closer to the player |
| `minTimeBetweenEvents` | 30 | Measured from the previous event's end |
| `titleDuration` | 3 | Total on-screen time D: fade in 0.2D, hold 0.6D, fade out 0.2D |

Update loop (skips when `Time.timeScale == 0`):
1. If an event is running, do nothing. The running coroutine handles its own pausing.
2. If `!WaveManager.WaveActive` or `WaveManager.DroughtActive`, do nothing. The roll timer does not advance.
3. Advance `sinceLastEvent`. If it is below `minTimeBetweenEvents`, return.
4. Advance the roll timer. When it reaches `rollInterval`, subtract `rollInterval` and roll.

Roll:
- Collect the events where `IsEligible(WaveManager.CurrentWave, RunMode.Tier, IronmanSelector.Enabled)` is true, then shuffle them.
- The first event with `Random.value * 100 < chance` fires. At most one event per roll.

Fire:
- Show the title and subtitle through `IAnnouncer.Current`. The hold time is `0.6D` and each fade is `0.2D`. This overwrites any title already on screen.
- Start `Run(ctx)` as a coroutine on the spawner and mark the event as running.
- When the coroutine finishes, clear the running flag and reset `sinceLastEvent` and the roll timer to 0.
- `OnDisable` stops any running event and resets all timers.

Boss waves and Duel waves are **not** paused. Only "no active wave" and Drought pause the spawner.

### `WaveEventContext`

Passed to `Run`. It exposes:
- `bool Paused`: true while `!WaveManager.WaveActive || WaveManager.DroughtActive`.
- `IEnumerator Wait(float seconds)`: counts time only while not paused, and yields while paused.
- `IEnumerator WaitWhilePaused()`.
- `bool TryGetSpawnPoint(out Vector2 pos)`: reads the player position fresh on every call, so spawns follow the player. It picks a uniform angle and a distance in `[minDistance, max(minDistance, baseRadius + radiusIncrease)]`, and returns false if there is no player.

The player is found with `GameObject.FindWithTag("Player")` and cached, as in `CollectibleSpawner`.

### Spawn Enemy flow

1. Roll N in [minSpawns, maxSpawns]. If N <= 0, end.
2. `WaveManager.ReserveEnemies(N)`. If it returns false (no active wave), end.
3. Repeat N times:
   - Wait for the rolled interval with a plain `WaitForSeconds`, **not** `ctx.Wait`. Skip this wait before the first spawn.
   - Pick a prefab and a spawn point, then call `WaveManager.SpawnAmbushEnemy(prefab, point, 0f, levelBonus)`.
   - Always call `WaveManager.ConsumeReservation()`.
4. The reservation holds the wave open, so a Spawn Enemy event never spans two waves. It must not pause: if it did, pending reservations would keep the wave from ever ending, which is a deadlock. A forced reset such as a restart stops the coroutine in `OnDisable`, and `BeginWave` zeroes the reservations.

### Blessing Drop flow

1. Roll N in [minDrops, maxDrops].
2. Repeat N times:
   - Wait for the interval with `ctx.Wait`, which pauses between waves and during Drought, so drops resume in the next wave. Skip this wait before the first drop.
   - Wait while paused.
   - Pick a reward and a spawn point, then call `CollectibleSpawner.Active.SpawnAt(data, point)`.
3. Drops ignore `CollectibleSpawner.maxConcurrent`. They still go through its pool and `live` list, so its `ReleaseAll` cleans them up.

## WaveManager changes (Wave assembly)

Ambush enemies already count as wave enemies through `RegisterSplitEnemy`, so they need no change.
New reservation support, shared by `WaveManager` and `UnlimitedWaveManager`:

- `protected int reservedEnemies`. Reset to 0 next to `totalSpawned = 0` in `WaveManager.StartNextWave` and in `UnlimitedWaveManager`'s equivalent.
- `protected int RemainingToSpawn => waveMaxTotalEnemies - totalSpawned - reservedEnemies`.
- `public static bool ReserveEnemies(int n)`: requires an active wave and n > 0. Adds n to `waveMaxTotalEnemies` and `reservedEnemies`, then calls `UpdateWaveText()`.
- `public static void ConsumeReservation()`: if `reservedEnemies > 0`, decrements both `reservedEnemies` and `waveMaxTotalEnemies`, then calls `UpdateWaveText()`. This is the same for success and failure:
  - On success, `RegisterSplitEnemy` has already added +1 to both `totalSpawned` and `waveMaxTotalEnemies`, so the reserved slot is handed back.
  - On failure, the reserved slot is simply dropped.
  - Either way `RemainingToSpawn` stays unchanged, so the regular spawn loop is unaffected.
- The spawn loop condition becomes `RemainingToSpawn > 0` in both managers.
- `waveMaxTotalEnemies - totalSpawned` in `SpawnEnemies` (Twin, Stampede, extra spawns) becomes `RemainingToSpawn` in both managers.
- The end-phase wait becomes `while (currentEnemies.Count > 0 || reservedEnemies > 0)` in both managers.
- `StopSpawning` sets `waveMaxTotalEnemies = totalSpawned + reservedEnemies`.

## CollectibleSpawner change

`private void SpawnAt(CollectibleData d, Vector2 pos)` becomes `public`.

## Assets and scene

The user creates these in the Unity editor after the code lands:
- `Assets/data/Events/Slime Rain.asset` (Spawn Enemy, with the 3 slime prefabs).
- `Assets/data/Events/Blessing Drop.asset` (Blessing Drop, with a weighted list of the existing collectibles).
- A `WaveEventSpawner` GameObject in `Assets/New.unity` that lists both assets.

## Verification (playtest in the editor)

- An event fires only during an active wave. Nothing fires during Drought or between waves.
- Only one event runs at a time, and the gap since the previous event's end is respected.
- The title and subtitle show in the event color and fade in, hold and fade out over D.
- Slime Rain: "Enemies: x/y" grows by N immediately, and the wave does not end until every slime has spawned and died. Slimes are at the current level plus the bonus, and they spawn in the ring around the player's current position.
- Blessing Drop: drops land in the ring around the player, pause between waves and during Drought, and resume in the next wave. Ironman never drops Rerolls.
- Gating: minMode, minWave/maxWave and allowOnIronman are respected.
- Regular, Unlimited, Twin Crowns, Stampede and Time Trial waves still end correctly.

## Commit sequence

1. docs: wave events spec
2. feat(Wave): enemy reservation API
3. feat(Collectible): public SpawnAt
4. feat(Event): asmdef + WaveEventData base + context
5. feat(Event): WaveEventSpawner
6. feat(Event): SpawnEnemyEventData
7. feat(Event): BlessingDropEventData
