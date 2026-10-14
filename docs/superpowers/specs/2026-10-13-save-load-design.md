# Run & Build Save/Load — Design

Date: 2026-10-13. Branch: `feat/save`.

## Goal

Let players keep a run across sessions and reuse strong builds.

- **Run save**: Escape menu "Save Progress" writes the run and returns to the home screen. "Load Run" on the home screen resumes at the start of the saved wave.
- **Build save**: "Save Build" on the death screen, or in the Escape menu between waves. "Load Build" on the home screen starts a fresh run at wave 1 with that build.
- Saves are named by the player and listed in one panel with Builds and Runs tabs. The list has search, sort, rename, delete, and hover info.

## Decisions (from Q&A)

| Topic | Decision |
|---|---|
| Build content | Full player state incl. gold: stats, attacks, awakenings, skill tree + unspent points, synergies, level/xp, contract taken |
| Build load | New run at wave 1, difficulty/mode/ironman from current home-screen selection. Pre-run picks skipped. Saved contract re-applied, contract offer skipped. Owned attacks/awakenings removed from reward pools |
| Build saved during an ironman run | Read-only: listed with hover info, Load disabled |
| Run save in ironman | Allowed, continues as ironman |
| Run save after load | Consumed (file deleted) as soon as the load is confirmed |
| Save Progress mid-wave | Writes the snapshot taken when the current wave started |
| Save Progress with unresolved picks | Disabled (reward/synergy/milestone/anomaly/contract panel open) |
| After Save Progress | Return to home screen |
| Resave in same run | Name prompt prefilled with the run's last save name; same name asks to overwrite |
| Save Build buttons | Death screen + Escape menu. Waves chain with no idle gap, so Escape Save Build mid-wave writes the build captured at the start of the current wave (right after picks resolved); outside an active wave it captures live |
| Death build state | State at death (mid-wave gold/xp included), transient status-effect stats removed |
| Hover info | Name, wave reached, mode + difficulty (+ironman), highest DPS, equipped attacks, date saved, playtime, incompatibility reason if any |
| List actions | Delete (confirm), rename, overwrite on same name, sort (date / wave / DPS / name) |
| Search | Skill-tree style multi-term: every term must match name, attack names, mode or difficulty |
| Unlimited waves | Supported for run saves |
| Compatibility | Incompatible only if a referenced asset (attack, awakening, node, contract, difficulty, mode, sequence, reward) is missing. Rebalanced assets load with new values; rolled stat values kept as saved. Incompatible saves still show hover info but cannot load |
| RNG | Fresh randomness on load |
| Cap | None |
| UI | Scripts with serialized refs; user wires objects in editor. Search field auto-built if unassigned, like the skill tree |
| Testing | New EditMode test assembly + manual playtest checklist |
| Constraint | No ScriptableObject asset is ever deleted |

## Architecture

New folder `Assets/scripts/Save/`, asmdef `CrystalFlux.Save`, referencing Core, Settings, Entity, Wave, SkillTree, StatusEffect, Projectile, TextMeshPro, UI, InputSystem. Nothing references `CrystalFlux.Save` except `Assembly-CSharp` (Misc UI). Existing assemblies expose hooks (events and capture/restore methods). They never call into Save.

### Units

**`SaveAssetCatalog`** (ScriptableObject at `Assets/Resources/SaveAssetCatalog.asset`)
- Entries `{ id (asset GUID), name, Object asset }` for every `AttackAsset`, `UpgradeAsset`, `SkillTreeDefinition`, `AnomalyData`, `DifficultyData`, `ModeData`, `WaveSequence`, and `BaseReward` subclass in the project.
- Lookups: `TryGet<T>(id)`, `GetId(Object)`. A runtime copy (`Instantiate`d upgrade) falls back to name match (`PlayerAttackHandler.NormalizeAttackName` / trimmed name).
- Editor: menu `Tools/Anamnesis/Rebuild Save Catalog`, plus a build preprocessor that rebuilds it before every player build. It only writes the catalog asset and never deletes assets.
- Skill nodes are referenced by their existing `nodeID` string, not by catalog.

**DTOs** (`[Serializable]`, `JsonUtility`)
- `SaveMeta`: `formatVersion`, `gameVersion`, `kind` (Build/Run), `id` (guid, also the file name), `name`, `savedAtUtc` (ISO string), `wave`, `waveManagerKind` (Regular/Unlimited), `difficultyName`, `modeName`, `ironman`, `readOnly`, `peakDps`, `playtimeSeconds`, `attackNames[]`, `referencedIds[]` (for the compatibility check).
- `BuildData`:
  - `statsJson`: `EntityStats` via `JsonUtility.ToJson`, transient effect stats removed
  - `attackIds[]` (per slot type)
  - `upgradeIds[]`
  - `skillTree { unlockedOrder[], skillPoints, choseStarting, effectState[] }`
  - `synergies[] { source, target, pct, applied }`
  - `contract { anomalyId, instanceJson }` (optional)
  - `peakDps`, `playtimeSeconds`
- `RunData`: `BuildData build`, `waveManagerKind`, `sequenceId`, `waveIndex`, `difficultyId`, `modeId`, `ironman`, `rerolls`, `additionalQuality`, `synergyChanceBonus`, `availableRareIds[]`, `availableTreasureIds[]`, `availableCorruptionSpecialIds[]`, pending counters (`pendingOccasionalRerolls`, `pendingOccasionalSkillPoints`, `pendingAnomalyRerolls`, `pendingAnomalySkillPoints`), `lastSaveName`.
- `SaveFile { SaveMeta meta; BuildData build; RunData run; }`, one per file.

**`SaveStore`** (static)
- Root `persistentDataPath/saves/{builds|runs}/<id>.json`.
- `List(kind)` reads every file in the folder. Unreadable files are skipped with a warning.
- `Write(file)` uses an atomic tmp + `File.Replace`/`Move`, the same pattern as `GameSettings.Save`.
- Also `Delete(kind, id)`, `Rename(kind, id, name)`, `FindByName(kind, name)`.
- `CheckCompatibility(meta)` returns null or a reason string listing the missing ids.

**`BuildCapture` / `BuildApplier`**: turn the live player into `BuildData` and back (algorithm below).

**`RunSession`** (static, survives scene reload like `GameRestart`)
- `PendingLoad`: a `SaveFile` and kind, set by the home screen before reload and consumed by the run starter after reload.
- `CurrentRunName`: prefill for the resave prompt.
- `WaveStartSnapshot`: the `RunData` captured when the current wave began.
- `RunClock`: playtime accumulator (scaled time while a run is active), restored from saves.

**`RunSaveController`** (MonoBehaviour in scene)
- Subscribes to new `WaveManager` events.
- Exposes `CanSaveProgress`, `CanSaveBuild`, `SaveProgress(name)`, `SaveBuild(name)`.
- On scene start, if `RunSession.PendingLoad` is set, it applies the load and starts the run.

### Hooks added to existing code

- `WaveManager`:
  - `public static event Action<WaveManager> WaveStarting`, fired at the top of `StartNextWave` after the sequence rollover and before the anomaly roll.
  - `public bool IsIdleBetweenWaves` (no active wave, no reward/anomaly/contract panel open).
  - `CaptureRunState(RunData)` / `RestoreRunState(RunData)` for the wave fields above.
  - `RemoveOwnedFromPools(IAttackHandler, IUpgradeHolder)`.
  - `RestoreContract(AnomalyInstance)`.
  - `StartLoadedRun()`: lock-in path without contract or pre-run picks.
- `PlayerSkillTree`:
  - Ordered `unlockOrder` list kept alongside `unlockedNodes`.
  - `RestoreUnlocked(order, points, choseStarting)` re-runs `UnlockNode` effects without cost or requirement checks or consumption, in saved order, so `UnlockEffect` runtime maps rebuild.
- `StatSynergyManager`: `GetState()` and `RestoreState(list)`. Restore adds entries with saved `applied` and does not touch stats.
- `EntityStatManager`: `CaptureStatsJson()` and `RestoreStatsJson(json)`. Restore overwrites `s` via `JsonUtility.FromJsonOverwrite`, clears `flagDepth`, then runs the same runtime normalization as `Start()` (hp = max, overhealth 0, gates on, alive).
- `StatusEffect` (base): `virtual void CollectAppliedStats(List<StatBuff>)`, implemented by every effect that changes non-flag stats (`StatBuffs`, `StatReduction`, `SoulRend`, plus any other found during implementation). `StatusEffectManager.CollectAppliedStats(list)` sums all active effects.
- `PlayerAttackHandler`: `GetAttacksSnapshot()` and `SetAttacksExact(list)`.
- `PlayerUpgradeManager`: reconcile helper `SetUpgradesExact(list)`, using normal `AddUpgrade`/`RemoveUpgrade` so `OnUnlock` side effects (coroutines, static flags, permanent effects) happen.
- `DpsMeter`: `SetPeak(float)` for restore.
- `DeathScreenUI`, `SettingsPanelUI`: Save Build / Save Progress buttons. These live in Assembly-CSharp and can reference Save.

## Capture / restore algorithm

The problem: stats are mutated in place by rewards, nodes, awakenings, synergies, level-ups, and also by active status effects. Some awakenings have non-stat side effects in `OnUnlock` (coroutines, permanent effects, static fields), so they must run again on load.

**Capture (`BuildCapture`)**
1. `effectStats` = `StatusEffectManager.CollectAppliedStats` (all active effects, flags excluded).
2. `statsJson` = live stats with `effectStats` subtracted, done on a temporary `Instantiate` copy of the stats object, never on the live one.
3. Attacks, upgrades, skill tree order + points, synergies (with `applied`), contract, peak DPS, playtime.

**Restore (`BuildApplier`)**, on a freshly loaded scene before the first wave:
1. Resolve every id via the catalog. If any is missing, abort (the UI already prevents this through the compatibility check).
2. Skill tree `RestoreUnlocked` in saved order. Node effects set attacks, awakenings and buffs as they did originally.
3. `SetAttacksExact`, then `SetUpgradesExact`, which reconcile to the saved final sets. Awakening `OnUnlock` runs for anything added.
4. `permanentEffectStats` = `CollectAppliedStats` now (permanent effects recreated by step 2–3).
5. `RestoreStatsJson(statsJson)`, then re-add `permanentEffectStats` via `AddStat`.
6. Synergy `RestoreState` (entries with saved `applied`, no stat change).
7. Skill points set exactly. Peak DPS and playtime restored.

The result: stats equal the saved clean stats, plus whatever the still-active permanent effects contribute. They stay consistent when those effects are later removed.

**Run restore** = build restore, then `WaveManager.ApplyDifficulty/ApplyMode` from the save, then `RestoreRunState`. `ApplyDifficulty`'s starting rerolls and skill points are overwritten by the saved values. Then `StartNextWave()` replays the saved wave from its start.

## Flows

**Save Progress (Escape menu)**
- Button enabled when a run is active and no pick panel is open, and disabled once the player is dead.
- Source: if `IsIdleBetweenWaves`, capture now. Otherwise use `RunSession.WaveStartSnapshot`.
- The name prompt opens, prefilled with `CurrentRunName` or else the default name `"{Mode} {Difficulty} - Wave {N}"`. An existing name triggers the overwrite confirm.
- Write, then `GameRestart.ToHomeScreen()`.

**Save Build (Escape menu between waves / death screen)**
- Capture now. Name prompt default `"{first attack names} - Wave {N}"`.
- `readOnly = ironman`. The death screen stays open after saving and the button shows "Saved".

**Home screen Load Build / Load Run**
- Opens `SaveListPanelUI` on the matching tab.
- Selecting a row: incompatible rows and read-only builds show disabled Load.
- **Load Run**: confirm dialog "Load '{name}'? The save is consumed." On confirm the file is deleted and `RunSession.PendingLoad` is set. If already on a fresh home screen, apply directly; otherwise `GameRestart.ToHomeScreen()` and apply on scene start. The run begins at the saved wave.
- **Load Build**: sets `PendingLoad` (build, no delete). The player still picks difficulty/mode/ironman and presses Regular or Unlimited. The wave button controllers check `RunSession.PendingLoad` and use `StartLoadedRun()` instead of the contract/pre-run path. The home screen shows a "Build loaded: {name}" label with a clear button.

**Rename / delete**: row buttons. Delete asks for confirmation. Rename reuses the name prompt, which also checks for name collisions.

## UI components (wired in editor)

- `SaveListPanelUI`:
  - Tab buttons (Builds/Runs), search field (auto-built if missing), sort dropdown, scroll content + `SaveRowUI` prefab, Load/Rename/Delete/Close buttons.
  - Pushes `MenuPause` while open.
  - Search reuses the skill-tree term splitting (all terms must match a lowercase haystack of name + attacks + mode + difficulty).
- `SaveRowUI`: name, wave and date labels, selected highlight, incompatible/read-only badge. Implements hover through the existing `ITooltipDisplay`/`TooltipUI` (title = name, body = info lines).
- `SaveNamePromptUI`: `TMP_InputField`, confirm/cancel, overwrite confirm sub-step. Trims names, max 40 chars, rejects empty names.
- `ConfirmDialogUI`: generic message + yes/no callbacks.
- Escape menu: `SaveProgressButton`, `SaveBuildButton` components that set interactable from `RunSaveController`.
- Death screen: `saveBuildButton` field on `DeathScreenUI`.
- Home screen: `LoadBuildButton`, `LoadRunButton` opening the panel tab, plus the loaded-build label.

## Error handling

- Disk write failure: warning log and a toast/label "Save failed". The run is not left, so Save Progress does not return to the home screen on failure.
- Corrupt/unreadable file: skipped in the list, warning logged.
- Incompatible save: shown greyed out with the reason in hover. Load is disabled.
- Load apply failure (exception mid-restore): the run save was already deleted, so the `SaveFile` is kept in memory and written back before applying, then deleted only after a successful restore.
- `formatVersion` newer than the game: incompatible.

## Testing

New `Assets/scripts/Save/Tests/` EditMode asmdef (`CrystalFlux.Save.Tests`, references `CrystalFlux.Save` + test framework):
- DTO JSON round-trip (SaveFile with build + run).
- `SaveStore` write/list/rename/delete/overwrite-by-name in a temp root (store root injectable).
- Compatibility check with a fake catalog (missing id produces a reason).
- Search and sort over a list of metas.
- Stats subtraction helper (clean = live - effect stats) on an `EntityStats` instance.

Manual playtest checklist (in the plan):
- Save Progress mid-wave and between waves.
- Load run (consumed, correct wave, stats/attacks/awakenings/tree/synergies/gold match).
- Death Save Build, then Load Build (wave 1, pools exclude owned, contract kept).
- Ironman read-only build.
- Unlimited mode run.
- Rename/delete/overwrite, search/sort, hover info.
- Incompatible save (temporarily point an id at nothing by editing the JSON).

## Non-goals

- Cloud sync, multiple profiles, autosave.
- Restoring mid-wave enemy state or open reward panels.
- Deterministic RNG replay.
- Save file encryption/anti-tamper.
