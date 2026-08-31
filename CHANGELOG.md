# Changelog

All *notable* changes to Anamnesis are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project *roughly* follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

⚠️ Represents potentially unstable/low-tested version.

## [v0.3.10] - 2026-08-31

### Added
- **Solar Wind Awakening** — `SolarWind.asset`, the first asset built on the `GrantStatusEffect` type added in v0.3.9. Fires on `OnHealthRegen` with a **30%** chance and a 3s cooldown, granting one stack of the new `Solar Wind` effect. It is not in `treasurePool`; like `Decoy Upgraded` it can only be obtained from its capstone node
- **`Solar Wind` status effect** in `Assets/data/StatusEffect/` — `StatBuffs`, 6s, up to 6 stacks, granting **+3 `hpRegen`**, **+8% `hpRegPct`** and **+6% `moveSpeedPct`** per stack, and dropping all stacks at once when the duration runs out rather than decaying one at a time
- **Solar Wind capstone node** — `Node_solarwind` (3 skill points, 50g refund) hangs off `Node_hprp5` and requires the Stellar Surge Awakening, granting Solar Wind alongside it
- **Five skill tree nodes**, taking the tree from 108 to **114**: `Node_hp3a` and `Node_hp3aa` extend the health branch past `Node_hp2ba`, `Node_hp3b` and `Node_hp3ba` extend it past `Node_hp2b`, and `Node_hprp5` (**+3% `hpRegPct`**, 1 point) leads into the Solar Wind capstone
- **`__Copyable/Node_`** — a blank `SkillNodeDef` asset and matching prefab kept as a template to duplicate when authoring new nodes

### Changed
- **Decoy Burst** projectile size 2.5 to **4**
- **Unlimited waves no longer set a "Choose Wave Reward" title** when standard rewards open, matching the quieter between-wave presentation introduced with `showCompletionMessage` in v0.3.9

## [v0.3.9] - 2026-08-31

### Added
- **`GrantStatusEffect` player upgrade** — a `PlayerUpgrade` type (`PlayerUpgrade/GrantStatusEffect`) that applies an authored `StatusEffect` to the player when its trigger fires, with a `stacks` count applied one `Apply` call at a time. It overrides all three `TriggerUpgradeEffect` overloads, so it works under every trigger condition regardless of which overload that condition dispatches to, and its `OnRemove` strips the effect again when the upgrade is removed. Its tooltip lists the effect name, the stack count when above 1, the duration and the effect's own description
- **`HoverScale` UI component** (`Assets/scripts/Misc/HoverScale.cs`) — scales a target transform to `hoverScale` (default 1.1) while the pointer is over it, easing at `speed` (default 12) on unscaled time so it keeps animating while the game is paused. It handles both `IPointerEnter/ExitHandler` for UI and `OnMouseEnter/Exit` for colliders, resets to the base scale on enable and disable so pooled buttons never come back mid-animation, and exposes `SetHoverScale` for runtime tuning. Added to the reward, anomaly, cooldown-indicator and skill-node prefabs and to 113 objects in the scene
- **`WaveManager.showCompletionMessage`** — a serialized toggle (default `true`, and disabled on both wave managers in the scene) that suppresses the "Wave N Complete" / "Boss Defeated" / "Anomaly Complete" subtitle, the reroll and skill-point announcement, and the 1.5s pause that followed them. With it off, `EndWave` runs immediately after rewards are rolled instead of waiting out the title. Honoured by both `WaveManager` and `UnlimitedWaveManager`
- **Lich phase buffs** — `Lich.prefab` carries an `EnemyPhase` with a single 40% HP threshold that grants **+40% `moveSpeedPct`** and **+20% `attackSpeedPct`**, the first enemy rebuilt onto the new phase system
- **New UI sprite sheet** — `Assets/data/images/UI/20250420manaSoulHeaderB-Sheet.png`

### Changed
- **Button prefabs moved** into `Assets/data/prefabs/Buttons/` — `AnomalyButtonPrefab`, `CooldownIndicatorPrefab`, `RewardButtonPrefab` and `SkillNodePrefab` keep their GUIDs, so every existing reference still resolves. `SkillNodePrefab`'s button transition switched from ColorTint to None now that `HoverScale` carries the hover feedback, and `CooldownIndicatorPrefab` dropped its stale `borderImage` reference
- **Blizzard applies `Slow 6 15 5`** instead of `Slow 5 3 15` — the same total slow ceiling, but built out of 15 stacks of 5% rather than 3 stacks of 15%, so the field ramps up gradually as a target lingers in it. `Slow 6 15 5` was authored in v0.3.8 and previously unreferenced

### Fixed
- **`Overheat` description** read "reduces attack by 6%" while the asset serialized `atkPct: -8`. The text now matches the value

### Rebalance
- **Cultist** — base attack 18 to 14, clone attack 12 to 11. `Summon PD` 80% Physical to **115% Physical + 40% Spell**; `Wave PD` 110% Spell to **75% Spell**
- **Jellyfish** — base attack 5 to 9. `BallSpam PD` 65% to **80% Physical**; `Ripple PD` 35% to **65% Spell**; `SplashB PD` 35% True to **35% Physical + 45% Spell**
- **Lich** — global cooldown 3s to 2s, with every attack's own cooldown raised to compensate: Wave 3s to 4s, Whirl 6s to 7s, Plant 8s to 9s. Wave 25% to **55% Spell**, Whirl 45% to **90% Physical**, Plant 30% to **45% True**

## [v0.3.8_1] - 2026-08-31

### Fixed
- **ApplyingProjectileHit** - static flag, set around the TakeDamage call in HandleHitEntity (saved/restored, so nested damage cascades don't leak state).
- **PlayerEvents.RaisePlayerTakeDamage** - now fires only when a damage instance passed IsEnemyHit (enemy team, non-DoT, dmg > 0, not iframe-bypass) and came from a projectile. DoT ticks, self-damage from attack costs, Consume/Heal packets, and dashing through a projectile with iframes no longer fail the trial.
- **Enemy Scaling** - magma and frost slime stayed at level 1.

### Added
- Kill accelerates next spawn by 15%

### Rebalance
- increased hp scaling (10% > 12%)
- increased armor scaling (5% > 8%)

## [v0.3.8] - 2026-08-31

### Added
- **Two new slime variants**, each with its own attack, animator, sprites and stat asset under `Assets/data/entity/enemy/`:
  - **Slime (Frost)** — `Slime_frost.prefab` / `slime frost base`. 80 HP, 4 attack, 15% crit chance, 30% crit damage, 60 armor, 30% `damageRes`, 5% `physicalRes`, 15% `spellRes`, 35 `effectRes`, 55 `kbRes`, 0.7 move speed, 9 detection range, 16 XP and 6 gold on death. Its attack **Blizzard** (`Blizzard AD`) is a 5s-cooldown Circle of 2 (+3 random) projectiles at 6 range with a 0.35s spawn delay: a slow-drifting (speed 1.2) 12s field of size 2 with effectively unlimited pierce, re-hitting the same target at most once per second, dealing **18% True** scaled off `EffAtk` and applying `Slow 5 3 15` at **65%** and the new `Freeze 2` at **15%**
  - **Slime (Magma)** — `Slime_magma.prefab` / `slime magma base`. 45 HP, 9 attack, 25% crit chance, 70% crit damage, 60 armor, 15% `damageRes`, 15% `physicalRes`, 5% `spellRes`, 20 `effectRes`, 40 `kbRes`, 0.55 move speed, 5 detection range, 18 XP and 7 gold on death. Its attack **Eruption** (`Eruption AD`) is a 4s-cooldown Circle of 4 (+3 random) projectiles at 3 range with 45° random spread and a 0.65s spawn delay: 0.5s bursts of speed 4, size 2, 4 pierce, dealing **70% Physical + 15% True** scaled off `EffAtk` and applying the new `Overheat` at **85%**
- **Three status effects** in `Assets/data/StatusEffect/`:
  - `Freeze 2` — `Stun`, 2s, single stack, shown as *Frozen*. Applied by Blizzard
  - `Overheat` — `StatBuffs`, 7s, up to 5 stacks, **-8% `atkPct`** and **-12% `stRegPct`** per stack. Applied by Eruption
  - `Slow 6 15 5` — `StatReduction`, 6s, up to 15 stacks, **-5% `moveSpeed`** per stack, capped at 90% reduction. Authored but not yet referenced by any projectile
- **Wave gating on rewards.** `AttackReward` and `PlayerUpgradeReward` carry a `minWave` (default `-1`, always eligible), and the rare and treasure pools now draw only from entries whose `minWave` is at or below the current wave. Seven rare-pool attacks are gated to **wave 20** — Shattered Singularity, Solar Collapse, Starfury, Revelation, Nirvana, Exodus and Luminaria — and five Awakenings to **wave 35** — Paradox, Soul Rend, Reminiscence, Serenade and Dash Advance. Everything else stays available from wave 1
- **Wave gating on enemies.** `UnlimitedWaveManager.enemyPrefabs` is now a `List<EnemySpawnInfo>` pairing each prefab with a `minWave`, and `GetRandomEnemy` picks only from the prefabs unlocked at the current wave: Slime at 0, Crab at 5, Slime (Magma) at 10, Bat at 15, Slime (Frost) at 20. An empty eligible set returns `null` rather than indexing the list
- **`EntityHealth.deathAnimTime`** — the death-animation delay is serialized per entity instead of the hardcoded 1s, and a value of `0` skips the animated death path entirely

### Changed
- **Enemy health bars resolve their own canvas.** `EntityHealth` previously grabbed whatever `FindAnyObjectByType<Canvas>()` returned first, which could be a world-space or unrelated canvas. It now looks for a screen-space canvas named `HealthBarCanvas` and, failing that, creates one at sorting order `-1`, copying its `CanvasScaler` settings from an existing screen-space canvas so bar sizing matches the rest of the UI. The bar is created lazily on the first `Update` rather than in `Start`, and is rebuilt if its canvas is destroyed or disabled mid-run
- **Boss waves spawn exactly one enemy.** `IsBossWave` (a wave carrying a `bossBarPrefab`) clamps both `waveMaxTotalEnemies` and the concurrent-enemy cap to 1 in `WaveManager` and `UnlimitedWaveManager`, and the multi-spawn burst is skipped on boss waves. Previously a boss wave used the wave's normal caps and could spawn the boss prefab several times
- **Unlimited-mode boss pacing** — `maxSpawnFrequency` `4` → `3`, `minWavesBetweenBossWaves` `6` → `4`
- **Tuning:**
  - **Autopilot** — `physicalMult` `2.95` → `3.35`; on-hit gains changed from a flat `staminaGainOnHit: 4` / `healthGainOnHit: 6` to `staminaGainOnHit: 2` and `healthPctGainOnHit: 3`, so its healing scales with max HP
  - **Feedback Loop** — `trueMult` `0.08` → `0.04`
  - **Slime** — `EnemyMovement.stoppingDistance` `0.85` → `1.2`
- **`Vulnerable 4 30` renamed to `Vulnerable 6 30`**, duration `4` → `6`s. The `.meta` GUID is unchanged, so the Decoy expiry burst still resolves it
- **Dash Advance's reward description** now reads *"Dashing advances all cooldowns by 12%."*, matching the `amt` `15` → `12` change made in v0.3.7

### Fixed
- **Wave spawn bursts could overshoot the wave's enemy budget.** `SpawnEnemies` spawned `wave / 10 + 1` enemies without checking how many the wave had left, so a wave capped at 3 could spawn 4 or more. The burst count is now clamped to `waveMaxTotalEnemies - totalSpawned`. `UnlimitedWaveManager.HandleWave` also looped against the raw `maxTotalEnemies` field instead of the per-wave `waveMaxTotalEnemies`, which is what the boss-wave clamp writes to
- **Rerolling an anomaly could consume the reroll and return nothing.** The reroll routed back through `RollAndGenerateAnomaly`, which re-rolls `anomalyChance` and re-checks the active-anomaly guard, so a paid reroll could legitimately produce an empty menu. Choice generation is split out into `GenerateAnomalyChoices`, which the reroll now calls directly, and a new `HasAnomalyChoices` check refuses the reroll — before spending a reroll charge or gold — when no anomaly is eligible for the current wave
- **Reward generation silently produced fewer buttons than requested.** The mixed, rare and treasure pool generators indexed their lists directly and `continue`d or `break`ed on an empty pool. Selection moved into `PickRareReward` / `PickTreasureReward`, which walk the pool once with reservoir sampling, skip `null` entries and honour `minWave`, returning `null` only when nothing is eligible
- **Health bars leaked and drifted.** A bar is now destroyed with its owner in `OnDestroy`, a `barRetired` flag stops a dead or destroyed entity from re-creating one, the shared canvas reference is cleared on `SubsystemRegistration` so it does not survive a domain-reload-disabled play session, and a bar whose owner is behind the camera (`screenPos.z <= 0`) is hidden instead of being drawn at a mirrored screen position. Bar value and text are refreshed through a single `RefreshHealthBar` that early-outs when neither current nor max HP changed
- **`EntityHealth` no longer null-references when no `IStatProvider` is present.** `Start` logs an error naming the offending object and returns instead of throwing on the first `AddStat`

## [v0.3.7] - 2026-08-31

### Changed
- **Occasional skill points can drop on any wave.** `RollOccasionalWaveRewards` hardcoded `pendingOccasionalSkillPoints` to `0` on every non-milestone wave, so the only skill point the wave-reward roll ever granted was the guaranteed one on each fifth wave. Non-milestone waves now roll it at **50%**, matching the reroll roll beside it
- **`GAME.md` re-synced against the assets.** It is now a generated-style reference over `Assets/data/PlayerData/` and the `WaveManager` reward pools serialized in `Assets/New.unity`, split into Starting Attacks, Rare Pool (18), Skill Tree Attacks, Treasure Pool Attacks, Player Upgrades and Status Effects. Each entry names its `AttackData`/`ProjectileData` asset, states damage multipliers as percentages, and uses the exact `StatType` enum name for scaling. Costs and on-hit gains are now separated (they were previously merged into one signed list), several scaling stats that had drifted from the assets are corrected, and follow-up forms are filed under their real names — `Blaze Soul` is documented as the `Cosmic Blaze` replacement attack, and `Autopilot` moved out of the rare pool
- **Tuning:**
  - **Cyclone Cleave** — `numPierce` `6` → `8`, `physicalMult` `5.4` → `5.65`, and a new `spellMult` `1.25`
  - **Aphelion** — cooldown `1.6` → `2.2`s
  - **Blood Pact** — `healthCost` `14` → `11`, `healthCostPct` `11` → `7`
  - **Stellar Maelstrom** — `staminaCost` `40` → `30`, `staminaCostPct` `44` → `24`, `manaCost` `18` → `14`, `manaCostPct` `52` → `42`
  - **Warp** — `projectileCount` and `randomCount` `2` → `3`, `staminaCost` `40` → `15`, `manaCost` `60` → `50`, `manaCostPct` `20` → `15`. `Node_warp` now reads *"Increases Warp's speed, size, count, and chance to spawn a rift"* — the capstone no longer raises the attack's cost
  - **Feedback Loop** — new `trueMult` `0.08` alongside its `0.15` spell multiplier
  - **Dash Advance** — `amt` `15` → `12`
  - **Serenade** — `chance` `40` → `35`

### Fixed
- **A multi-instance damage packet applied only its first instance.** `EntityHealth.TakeDamage` walks each `DamageInstance` in the packet through `ChangeHealth`, which triggers hurt i-frames on any damaging hit; the i-frames then blocked every later instance in the *same* packet, so a projectile authored with Physical + Spell + True damage landed only its Physical portion. The trigger is now deferred: instances are processed with the hurt i-frame suppressed, and a single i-frame window is opened once the packet is finished. Nested `TakeDamage` calls (reflect and thorns Awakenings re-entering during a packet) merge their pending trigger into the outer packet rather than opening a window mid-packet
- Six authoring bugs turned up while re-syncing `GAME.md`, all corrected in the assets:
  - **`Supernova PD` pointed at a deleted status effect** (guid `e6076c0291862a842aa9b9e05e87f1e5`), so its 40% on-hit effect never applied. It now references `Weaken 5 10 4` — **-10% attack per stack**, 4 stacks, 5s
  - **`Starlit Reflexes` serialized `pctAmt: 6`** next to `amount: 10`, but `GainMana` only reads `amount`, so the 6% max-mana portion was inert. The percentage is dropped and the flat amount is raised `10` → `18`
  - **`Lifeforce PD` had `specialMult: 0.5` with `specialSclaing: None`**, so the multiplier did nothing. `specialSclaing` is now `HpConsumed`, which is what the attack's `30 +40%` max-HP cost was written for — Lifeforce scales with the health it spends
  - **`Supersonic Cooldown` was referenced by nothing.** `Supersonic PD` now applies it to the caster at 100% on cast, so the 3s marker actually lands
  - **`Exodus C` was inconsistent with its siblings.** `Exodus C AD` is typed `Additional` rather than `Technique`, matching `Exodus B AD`, and `Exodus C PD` scales off `critDamage` instead of `resPen`
  - **`Ignotion Flash` renamed to `Ignition Flash`** — the folder and its `AD`, `PD`, prefab, clip and controller assets. Every `.meta` GUID is unchanged, so the rare-pool entry and prefab references still resolve

## [v0.3.6] - 2026-08-30

### Added
- **Reference-counted stat gates** — `EntityStatManager` now tracks `CanMove`, `CanAttack`, `CanDash`, `CanGainHp`, `CanGainMana`, `CanGainStamina` and `isImmune` by depth instead of summing raw ±1 writes. Two overlapping holders (a Stun landing during a Pulled, a stun landing mid attack wind-up) can no longer release each other's hold, and a stray release cannot push a gate past its resting state
- **`EnemyMovement.Active`** — static list of enabled enemies maintained in `OnEnable`/`OnDisable`. Decoy taunting walks it instead of `GameObject.FindGameObjectsWithTag("Enemy")`, so it no longer depends on the tag being set and no longer allocates an array per cast
- **Skill-tree adjacency map and undo reachability check** — `PlayerSkillTree` builds `nodesById`/`neighbours` once per runtime-node generation and reuses them for both `CanUnlock` and the new `WouldStrandDependents`, a BFS from the unlocked starting nodes. Undoing a node that would leave other unlocked nodes disconnected is now refused with *"Other unlocked nodes depend on this one"*
- **`AnomalyInstance.Description`** — virtual description on the instance, overridden by `StatModifierInstance` with its rolled stat and value. `AnomalyButtonUI` and `WaveManager` read it instead of `AnomalyData.desc`
- **`Slow 5 3 15`** — new `StatReduction` asset (`data/StatusEffect/`): **-15% move speed per stack**, 3 stacks, 5s. Applied by the Supersonic projectile at 60% on hit
- **`EntityGearManager.RollableStats`** — explicit whitelist of the stats a `PureRandomStatAndRoll` gear roll may pick. The roll previously indexed the whole `StatType` enum, so it could roll flags and derived values (`isAlive`, `CanMove`, `EffAtk`) as gear stats
- **`StatType.Gold` is readable** — `EntityStats.GetValue` returns `gold`; it previously fell through the switch and reported 0

### Changed
- **Status-effect content moved out of the scripts tree.** The nine effect scripts are flattened into `Assets/scripts/StatusEffect/StatusEffects/` (one file per effect, no per-effect folder), and all 25 authored effect assets moved to a new `Assets/data/StatusEffect/` folder next to the rest of the data. GUIDs are unchanged, so every existing reference still resolves
- **Knockback resistance is applied once, on impact.** `KnockbackHandler.UpdateForces` multiplied each live force by `1 - kbRes%` *every frame*, compounding resistance until the force decayed to nothing. Resistance is now folded in by `ApplyKnockback`, and `UpdateForces` takes an explicit `dt` — `Time.fixedDeltaTime` from `PlayerMovement` (a `FixedUpdate` caller, which was reading `Time.deltaTime`) and `Time.deltaTime` from `EnemyMovement`
- **Awakenings are runtime copies.** `PlayerUpgradeManager` instantiates every upgrade in `activeUpgrades` on `Start` and every upgrade passed to `AddUpgrade`, and destroys the copy on removal or teardown. Per-upgrade runtime state no longer writes back into the shared project asset. `EntitySummonHandler` no longer does this copying itself
- **Upgrades are matched by name everywhere.** `HasUpgrade` and `RemoveUpgrade` go through one `FindActive` name lookup, so a skill-tree undo removes the runtime copy of the asset it granted rather than failing a reference comparison. `Start` logs an error when two upgrades on the same holder share a name, since the name is the identity
- **Enemy attack chains iterate instead of recursing.** `EnemyAttackHandler.PerformAttack` walks `nextAttack` in a loop with a visited set, so a cyclic chain terminates instead of nesting coroutines forever. The `CanMove` hold is taken and released per step (and only when `canMoveDuringAttack` is false), and `OnDisable` releases whatever is still held
- **Non-allocating overlap queries.** `Projectile` and `EntityProjectileHandler` targeting use `Physics2D.OverlapCircle` with a shared buffer and a `ContactFilter2D` instead of `OverlapCircleAll`, and both cache `Camera.main`
- **Orbit lists drain synchronously.** `ReleaseOrbits`, `AbsorbOrbits`, `RedirectOrbits` and `ExplodeOrbits` now remove projectiles from the registry as they take them, replacing a `ClearOrbitsAfterDelay(0.1f)` coroutine that wiped the whole list 0.1s later regardless of how many were taken. `AbsorbOrbits` returns the count actually absorbed instead of the count requested
- **Skill-tree connector lines are pooled.** `SkillTreeLineRenderer` reuses its `Image` objects across redraws and deactivates the leftovers instead of destroying and recreating every line whenever a node changes. It collects node UIs with `FindObjectsInactive.Include`, and walks prerequisites only — the second reverse pass drew the same undirected connections a second time
- **The skill tree builds in `Awake` and restores the previous `Time.timeScale`** when it closes, instead of forcing `1`. Closing the tree while the game was paused for another reason no longer resumes it
- **`StatReduction` writes to base stats.** `EffAtk`, `EffMaxHp`, `EffArmor`, `EffSpd`, `EffInt`, `EffHpReg`, `EffStReg`, `EffMaxMana` and `EffMaxStamina` are mapped to their base counterparts before the debuff is applied; writing to a derived stat wrote into a value that is recomputed from the base
- **`StatBuffs` tracks applied buffs in a list.** The old `Dictionary<StatBuff, StatBuff>` keyed on the authored struct, so two entries buffing the same stat by the same amount collapsed into one and only one was ever undone
- **`Detonator` fires one packet.** It sums remaining damage across every active DoT, removes them once, skips DoTs with a zero tick interval, and does nothing when the total is 0 — previously it built a packet per DoT and called `RemoveEffect<DoT>()` inside the loop, so later iterations read effects that were already gone
- **`AttackData` runtime copies clean up fully** — `OnDestroy` destroys the runtime `pd.effects` copies and `pd.additionalAttack` alongside `pd` and `nextAttack`
- **Statics clear themselves.** `ProjectileSpawner`, `TooltipUI`, `TextIndicatorSpawner`, `PlayerUpgradeManager`, `WaveManager.ActiveManager` and `IAnnouncer.Current` null their static references in `OnDestroy`, and `EnemySpawner` resets `EnemySpawning.Spawn` on subsystem registration so a domain reload cannot leave a dead delegate installed. `TooltipUI` and `TextIndicatorSpawner` destroy the duplicate and bail instead of half-initialising it
- **`Entity` requires `EntityStatManager`** rather than `IStatProvider` — `RequireComponent` cannot take an interface, so the old attribute enforced nothing. `EnemyStatManager` overrides the now-`virtual` `Awake`/`Start` instead of shadowing them
- **`EntitySplitting.splitChance` is a 0-1 `[Range]`**, matching `summonChance` and `EffectData.chance`. It was authored on a 0-100 scale but multiplied by `0.01` before the roll in the one place that used it
- **Tuning:**
  - **Autopilot** — `staminaGainOnHit` `10` → `4`, `healthGainOnHit` `15` → `6`, projectile `speed` `12` → `6`, homing `followDistance` `1.5` → `0.5`
  - **Supersonic** — `trueMult` `0.85` → `1.1`, the projectile now applies `Slow 5 3 15` at 60% on hit, and the Awakening's cooldown drops `3` → `1`s
  - **`Node_ms2` now hangs off `Node_ms1`** instead of `Node_atkarmor2`, so the movement-speed branch is a chain rather than two siblings on the same parent

### Fixed
- **Resistance penetration and armor shred were read from the victim.** `DamageCalculator.CalculateDamageTaken` pulled `resPen` and `defShred` off the entity *taking* the damage, so the player's penetration did nothing to enemies while an enemy's shred reduced its own effective armor. Both now come from the attacker, which `EntityHealth` resolves from each damage instance's owner and passes in. Armor also reads `EffArmor` (armor plus `armorPct`) instead of the raw `armor` field, and the negative-armor curve tops out at **3x** rather than 2x
- **`_isTriggeringOnDealDamage` was `static`.** One entity inside an `OnDealDamage` trigger suppressed the trigger for every other entity in the scene. It is now per-instance
- **An immune target dropped the rest of the damage packet.** `TakeDamage` returned on the first instance blocked by i-frames, so True, Heal and Consume instances behind it in the same packet never applied. It now skips that instance and continues
- **Health and stamina gains were swapped in the attack tooltip.** `PlayerAttackCooldownUI` destructured `Projectile.CalculateStatGains` as `(spg, hpg, mpg)`; it returns `(hp, stamina, mana)`
- **Enemy attack HP gates compared a fraction against a percentage.** `ChooseAttackIndex` computed `currentHp / EffMaxHp` (0-1) and tested it against `minHpPct`/`maxHpPct` (0-100), so any attack with `minHpPct > 0` was unreachable and every `maxHpPct < 100` gate passed. It also treated `phaseReq` as satisfied on enemies with no `EnemyPhase` component at all
- **Projectile and orbit targeting picked the wrong team.** `Projectile.FindClosestEnemyInDirection` hardcoded team 0 as the target and `EntityProjectileHandler.FindNearestEnemy` skipped team 0, so enemy-owned projectiles searched for other enemies. Both now target the team opposite their owner. The "skip dead targets" test in all three search sites required `isAlive <= 0` **and** `currentHp <= 0` — nearly never true — and is now an `or`
- **Cooldown reduction was clamped to a maximum of `0.9`.** `GetEffCd` clamped `1 - cdrPct%` to `[0.1, 0.9]`, so an attack on an entity with *no* cooldown reduction still came back 10% early. The upper bound is now `1`
- **Zero-multiplier damage instances were still built.** `DamagePacketBuilder` added a `Physical`, `Spell` and `True` instance for every attack regardless of its multipliers, producing stray `0` damage numbers. An instance is skipped when both its multiplier and its additional scaling are 0
- **`StatModifierInstance` wrote its rolled description into the shared `AnomalyData` asset**, permanently overwriting the authored text with the last roll
- **`AnomalyButtonUI.Setup` dereferenced a null instance** — its guard returned early only when `instance != null && instance.amd == null`, which is the one case where the fields were safe to read
- **Anomaly selection ran the wave twice.** `OnAnomalyButtonClicked` called `BeginWave()` and then `HandleWave()` on the same wave data
- **The corrupt button showed on panels that cannot be corrupted.** Visibility is now decided in one place (`PanelSetup` → `UpdateCorruptButton`), which hides the button on anomaly and milestone panels and on every fifth wave; `RewardType` is set before the panel is built so the check sees the right type
- **Occasional rerolls were credited to `ActiveManager`** rather than to the manager processing the wave
- **`additionalQuality` never reached the mixed reward pool** and was never cleared, so the bonus quality earned by skipping accumulated across waves and only applied to one pool. Both pools now pass it and reset it after use
- **`AttackReplacement` left a phantom attack.** When the replaced slot was empty on apply, expiry called `UpdateAttack(type, null)`; it now removes the attack instead, and clears `setAttack` so expiry runs once
- **`Pulled` could permanently freeze its target.** It released `CanMove` only if the target was not already immobilised when the pull landed, which lost the release whenever two sources overlapped. It now tracks its own hold and pairs with the reference-counted gates
- **`SoulRendPU.OnUnlock` appended its effect every time it ran**, stacking duplicate Soul Rend applications on the basic and skill attacks. `AddOnce` checks for the effect first, and creates the `effects` list if it is missing
- **`Reminiscence` could recurse** — the random attack it casts can re-raise the condition that triggered it. Guarded with an `isCasting` flag
- **Summon lifetime expiry skipped death bookkeeping.** `Destroy(summon, lifetime)` removed the object without unsubscribing `OnDeath`, clearing `activeSummons`, or undoing per-summon buffs; an `ExpireSummon` coroutine now runs `OnSummonDeath` first
- **Runtime `ScriptableObject`s were destroyed with `DestroyImmediate`** in `PlayerAttackHandler`, `EnemyAttackHandler` and `PlayerSkillTree`, which is an editor call and unsafe during play teardown
- **`PlayerSkillTree` aliased the definition asset's node list** instead of copying it, so runtime changes to `allNodes` reached the `SkillTreeDefinition` asset
- **Fractional regen was rounded away.** `EntityStats.Apply` rounded `staminaRegen` and `EffStReg` to an int, and enemy level scaling rounded `hpRegen`, so any authored value below 1 became 0
- **`SpawnCircle` could spawn nothing** when `projectileCount` was 0; the final count is clamped to at least 1
- **Health bars initialised to full.** `BossBarUI.Setup` and `EntityHealth.InitializeHealthBar` set the slider value to max HP rather than current HP, so a pre-damaged or scaled spawn showed a full bar until its first hit
- **`StatusEffectManager` ran teardown on application quit**, expiring effects against half-destroyed objects. On quit it now just destroys the runtime copies
- **`UnlockEffect.Remove` removed an attack slot it never granted** — it now checks `HasAttack` first, and both `Apply` and `Remove` null-guard the target
- **Boss bars spawned for enemies with no stats** in both wave managers; the bar is only created when the enemy has an `IStatProvider` to feed it
- **`TriggerUpgradeEffect(GameObject, Vector2?)` had a default argument**, making it ambiguous with the single-parameter overload at every one-argument call site. The default is removed
- **`PlayerMovement` assumed the player starts facing right** (it now reads `localScale.x`), and a dash with zero speed or distance spun an endless coroutine
- **`Decoy` was tagged `"Player"`**, so player-tag lookups elsewhere could pick it up. Taunting no longer needs the tag
- **Null-safety pass across UI and spawners** — `BossBarUI`, `PlayerUI` (via a shared `SetBar`), `PlayerResourceUI`, `RewardButton`, `TooltipUI`, `GameController`'s UI panel, `WaveManager`'s reroll/corrupt/skip buttons, `EntityGearManager.available`, `EnemySpawner`'s prefab, `TextIndicatorSpawner` (which now requires a prefab and canvas and logs when either is missing), `EntityHealth.DropGold` when the killer is gone or has no stats, `EntityHealth`'s health bar when only the text prefab is absent, `Projectile` with no `ProjectileData` (logs and self-destructs) and `ProjectileSpawner.SpawnFromPattern` with no camera or source

## [v0.3.5] - 2026-08-30 - Content Expansion Update

### Added
- **Luminaria** — new rare-pool Ultimate (`Attacks/Rare Pool/Luminaria/`, with its own prefab, controller, clip, `AttackData`, `ProjectileData` and an 11-frame `Priest_skill2` sprite sheet). 18s cooldown, single stationary projectile (`speed 0`, `useTrueAngle`), 1s lifetime, size 3, effectively unlimited pierce (`numPierce 3000`), 8 knockback force over 0.3s. Deals **270% True** damage scaling off `EffHpReg` — the first attack in the game to scale off health regen. Costs 15 stamina, 10% max health and 60 mana; returns 2 stamina and 3 mana on hit. On cast it applies **Holy Bounty** to the player (100%) and has a 40% chance to apply **Stun** for 2s. Registered in the `rarePool` of both the regular and the Unlimited reward manager in `New.unity`
- **Cosmic Superimposition** — second capstone skill node (`SkillTreeNodes/_Capstone/Node_decoy.asset` + node prefab, placed in `New.unity` at `(-67.1, -228.8)`). Requires the player to already own the **Cosmic Afterimage** (Decoy) Awakening via `NodeRequirement.requiredAwakenings`, hangs off `Node_ms2`, costs 3 skill points and 50g to refund, and its `UnlockEffect` swaps the owned Awakening for **Decoy Upgraded**
- **Decoy Upgraded** — capstone version of the Decoy Awakening (`PlayerUpgrade/Decoy Upgraded.asset`). Against the base version: lifetime `4` → `6`s, cooldown `6` → `5`s, tint alpha `0.61` → `0.78`, and it carries a `projectilePrefab` so the decoy detonates on expiry instead of quietly disappearing
- **Decoy expiry burst** — attack set under `Attacks/SkillTree/Decoy/` (prefab, controller, clip, `Decoy AD`, `Decoy PD`, `1225.png`) fired at the decoy's last position when its lifetime runs out. `Additional` type, no cooldown and no resource cost; the projectile is a stationary 1s burst, size 2.5, `numPierce 3000`, dealing **225% Spell** scaling off `EffAtk` with 5 knockback force, applying **Vulnerable** on hit (100%) and returning 3 stamina and 3 mana on hit (`basedOnDmgDealt`)
- **Two movement-speed skill nodes** — `SkillTreeNodes/ms/Node_ms1` and `Node_ms2` (+ node prefabs in `New.unity` at `(-23.1, -252.9)` and `(-30.3, -219.9)`), each granting **+2% `moveSpeedPct`** for 1 skill point / 50g refund. Both hang off `Node_atkarmor2`; `Node_ms2` is what gates the new Cosmic Superimposition capstone
- **`Holy Bounty`** — 24s single-stack buff (`StatusEffects/StatBuff/Holy Bounty.asset`) granting **+80% `addDmgPct`**, **+30% `resPen`** and **+15% `damageRes`**. Applied to the player on Luminaria cast
- **`Vulnerable 4 30`** — 4s single-stack debuff (`StatusEffects/StatBuff/Vulnerable 4 30.asset`) applying **-30% `damageRes`**. Applied by the Decoy expiry burst
- **`Decoy.projectilePrefab`** — optional prefab on the `Decoy` upgrade. When set, the decoy spawns that attack's full pattern at its own position the moment it expires; when null the decoy just disappears, so the base Awakening is unchanged

### Changed
- **Decoy lifetime is now a coroutine rather than a delayed `Destroy`.** `Decoy` is a `ScriptableObject` and cannot run coroutines itself, so the routine is hosted on `ProjectileSpawner.Instance` — the same object that has to spawn the expiry burst anyway. The routine waits out the lifetime, snapshots the decoy's position, destroys it, then hands that position to `ProjectileSpawner.SpawnFromPattern`. If there is no `ProjectileSpawner` in the scene it falls back to the old `Destroy(decoy, lifetime)` path, and it bails out early if the decoy was already destroyed
- **`DamageCalculator` resistance and armor model reworked.** Three changes, all of which shift damage numbers:
  - **Type resistance now shares one pool with `damageRes`.** `physicalRes` / `spellRes` used to be a separate multiplier applied after the `damageRes` one, which meant `resPen` could never touch it. They are now summed into a single `effRes` that `resPen` is subtracted from, so resistance penetration finally works against type resistance, and stacking `damageRes` with type resistance is additive instead of multiplicative
  - **Penetration overflow is now a damage bonus.** Because the combined pool is floored at `-100` (not at 0), penetrating past an enemy's total resistance drives `effRes` negative and multiplies damage up to a hard **2×** cap
  - **Armor may now go negative from `defShred` overflow.** `effArmor` is no longer clamped at 0. The positive side is the same diminishing curve rewritten as `100 / (effArmor + 100)`; the negative side mirrors it as `2 - 100 / (100 - effArmor)`, so over-shredding ramps physical damage toward the same **2×** ceiling instead of stopping dead at 1×
- **Autopilot retuned** — trigger changed from `OnTakeDamage` to `OnTakeHit`, so it now only answers direct hostile hits rather than every DoT tick and self-inflicted health cost. Cooldown `3` → `2`s, and the attack fires a `Circle` pattern of **3** projectiles instead of a `Single` one
- **Warp capstone cost `1` → `3` skill points**, matching the new Cosmic Superimposition capstone
- **Base Decoy tint alpha `0.75` → `0.61`**, making the untrained decoy more obviously a ghost
- **Blaze B1 `staminaGainOnHit` `2` → `3`.** The asset was also re-serialized against the current `AttackData` layout (`absorbOrbits` → `absorbOrbitPct`, `redirectCount` added, `type` moved into field order); no behaviour change from the re-serialization
- **Skill tree grew 105 → 108 nodes** (`Node_ms1`, `Node_ms2`, `Node_decoy` added to `SkillTreeDefinition.asset`)

### Fixed
- **Skill-tree connector lines were coloured as if connections were directed.** `CanUnlock` walks prerequisites both ways, so either endpoint can be the one you unlock — but `GetLineColor` only ever asked whether the *child* was unlocked and whether the *child* could be unlocked. A line whose child was unlocked and whose prereq was not showed as fully unlocked, and a line that was available in the prereq→child direction only never showed as available. It now returns `unlockedColor` only when **both** endpoints are unlocked; when exactly one is unlocked it asks `CanUnlock` about the *other* endpoint, so the available colour follows whichever end is actually reachable

## [v0.3.4] - 2026-08-30

### Added
- **Seven `PlayerUpgrade.TriggerCondition` values are now wired up.** They existed on the enum but nothing ever raised them, so any Awakening authored against one was inert. Each now has exactly one firing site:
  - **`OnTakeHit`** — `EntityHealth.TakeDamage`, alongside the existing `OnTakeDamage`. Deliberately narrower than `OnTakeDamage`: it fires only for a direct hit from a hostile entity. A new `IsEnemyHit` helper rejects the instance unless the damage type is `Physical`, `Spell` or `True` (so DoT ticks, heals and `Consume` health costs are out), the packet does **not** set `bypassIFrames` (which is what a DoT tick and every self-inflicted sustain packet sets), the instance owner is neither null nor the victim itself, and the owner's `ITeamMember.TeamID` differs from the victim's
  - **`OnKill`** — `EntityHealth.TakeDamage`, in the branch that already handles XP and gold when `ChangeHealth` reports the victim died. Fires on the killer's `PlayerUpgradeManager`, and skips the case where the packet source is the victim, so bleeding out on an attack's own health cost is not counted as a kill
  - **`OnDeath`** — `EntityHealth.StartDeathSequence`, immediately after `isAlive` drops and before the health bar, status effects and GameObject are torn down. An upgrade whose `delay` outlasts the 1s death animation is cut off with the object
  - **`OnStaminaRegen`** — `PlayerResourcePool.RegenStamina`, on each tick that actually credits at least 1 stamina. Mirrors how `OnHealthRegen` fires from `EntityHealth.RegenHp`
  - **`OnManaRegen`** — `PlayerResourcePool.ChangeMana`, on any gain that actually lands (`amount > 0` and a non-zero applied change). There is no passive mana regen loop and no mana-regen stat, so unlike health and stamina this covers every mana gain rather than a periodic tick
  - **`OnLevelUp`** — `PlayerLevel.LevelUp`. Because `GainExp` loops while the XP pool clears the requirement, one large XP pickup that crosses several thresholds fires the trigger once per level
  - **`OnSpawnProjectile`** — raised once per projectile that reaches the scene, and dispatched through the `(player, spawnCenter)` overload with the projectile's spawn position, matching `OnProjectileHit`
- **`ProjectileSpawner.ProjectileSpawned`** — static `Action<GameObject, GameObject, Vector2>` raised at the end of `SpawnProjectile` with the source, the spawned projectile and its spawn position. Static rather than instance-scoped so listeners do not have to race `Instance` during `Awake`. This is how `OnSpawnProjectile` crosses the assembly boundary: `CrystalFlux.Projectile` cannot see `PlayerUpgradeManager`, and `CrystalFlux.Core` is an external package, so the notification travels the one direction the asmdefs already allow — `CrystalFlux.Entity` subscribing to `CrystalFlux.Projectile`
- **README gained an `Awakening trigger conditions` table** documenting all 22 conditions, what raises each one, and which `TriggerUpgradeEffect` overload it dispatches to — an upgrade that overrides the wrong overload silently does nothing, which was previously undocumented

### Changed
- **`PlayerUpgradeManager` subscribes to projectile spawns** in `OnEnable`/`OnDisable` and filters the event down to projectiles the player itself owns
- **`PlayerResourcePool` and `PlayerLevel` now cache a `PlayerUpgradeManager`** in `Start`, the same pattern `EntityHealth` already used for `cpum`. Both stay null-safe on entities without one

### Fixed
- **Reentrancy guards on the two self-feeding triggers.** An `OnSpawnProjectile` upgrade that spawns a projectile, or an `OnManaRegen` upgrade that grants mana, would otherwise re-enter its own trigger without bound. Both follow the existing `_isTriggeringOnDealDamage` pattern. The guard covers the immediate call only — an upgrade with a non-zero `delay` that re-triggers its own condition still needs a cooldown, and this is called out in the README

## [v0.3.3_2] - 2026-08-30

### Added
- **Autopilot** — new treasure-pool Awakening (`PlayerUpgrade/Autopilot.asset`, a `SpawnProjectile` upgrade) that fires a homing projectile when the player takes damage. 100% chance, 3s cooldown, 0.25s delay. The projectile spirals outward at speed 12 (`spiralSpacing` 2), lives 6s, is size 2, pierces once and then destroys itself, homes onto targets within 1.5, deals 295% Physical scaling off `EffArmor`, and knocks back with 8 force. Its `AttackData` returns 15 health and 10 stamina on hit (both `basedOnDmgDealt`), so the upgrade doubles as armor-build sustain. Registered in the `treasurePool` of both the regular and the Unlimited reward manager in `New.unity`, with its own prefab, controller, clip and `Bullet 24x24 Part 9A Free` sprite sheet under `Attacks/Treasure Pool/Autopilot/`
- **Warp capstone skill node** — `SkillTreeNodes/_Capstone/Node_warp.asset` (+ node prefab, placed in `New.unity` under the skill tree canvas at `(-359.3, 293.3)`) is the first *capstone* node: it requires the player to already own **Warp** (`NodeRequirement.requiredAttacks`), hangs off `Node_mm3` as its prerequisite, costs 1 skill point / 50g to refund, and its `UnlockEffect` swaps the owned attack for the upgraded **Warp AA**. Added to `SkillTreeDefinition.asset`, bringing the tree to 105 nodes
- **Warp AA** — upgraded Warp attack set under `Attacks/SkillTree/Warp/` (own prefab, controller, clip, `AttackData`, `ProjectileData`). Against base Warp: projectile speed `0.8` → `1.4`, size `2` → `3`, Warp Rift proc chance `15%` → `25%`, and, as the trade-off the node description promises, stamina cost `15` → `40` and mana cost `50 / 15%` → `60 / 20%`. On-hit returns shift from `+3` mana to `+1` stamina and `+2 / +2%` mana
- **`destroyOnMaxPierce` on `ProjectileData`** — when set, a projectile that has spent its pierce budget destroys itself on its next trigger contact instead of lingering as an inert collider for the rest of its lifetime. `numPierce` moved out of `Basic` into a new `Piercing` header alongside it
- **`pullToSource` on `Pulled`** — forces the pull center onto the source's transform even when the effect was authored with an explicit `location`. Previously `location` always won and the source was only the fallback
- **`PlayerAttackHandler.NormalizeAttackName(string)`** — public static helper that trims and strips stacked `(Clone)` suffixes off an attack name

### Changed
- **`randomDir` now randomizes travel, not just facing** — `Projectile.HandleDirection` folded the random angle into `finalAngle`, which only set `transform.rotation`; `dir` kept the spawn/aim direction, so a "random direction" projectile flew straight at the target while pointing elsewhere. The random branch now runs first, derives `dir` from the rolled angle, applies `rotationOffset` to the rotation, and returns early
- **Corrupted rewards colour by sign** — `RewardButton.CorruptButton` picks `Color.darkGreen` when `corruptMult >= 0` and `Color.darkRed` otherwise (was unconditionally dark red, even for beneficial corruptions)
- **Serenade** — `pctAmt` `16` → `24` (additional True damage per proc; chance stays 40%)
- **Aphelion resprite** — sprite sheet swapped from `Bullet 24x24 Part 9B Free` to `Bullet 24x24 Part 5B Free` (the 9B sheet is deleted). `Aphelion Clip` retimed across its 8 frames, stop time `0.51666665` → `0.76666665`, and the prefab's sprite draw size `0.24 × 0.24` → `0.2 × 0.21`
- **Starting attacks moved to `Assets/data/PlayerData/Attacks/Base/`** — `Cyclone Cleave` and `Lacerate` relocated wholesale (assets byte-identical, GUIDs preserved), separating starting kit from the reward pools
- **Skill-tree rejection message no longer leaks internals** — `PlayerSkillTree` returns `Requirement not met` instead of `Requirement not met: {req.GetType().Name}`
- **Player prefab instance in `New.unity`** carries an `activeUpgrades` override with Autopilot in slot 0 and array size `0` — the reference is wired but the array is empty, so nothing is granted at runtime (editor test wiring)
- `graphify-out/` regenerated against the current source
- **`TODO.md` Content Updates split into `Major` / `Minor`** — gear, shop/chest, and the elemental damage / affinity / reaction line items grouped under `Major` alongside a new **Finish Gear/Item system** entry; the rest stay under `Minor`. The standalone Rune/Enchantment item is dropped (folded into the gear work). Docs also record the first capstone node against the Pre-v0.4.0 capstone checklist item

### Fixed
- **`OnTakeDamage` upgrade trigger never fired** — the `PlayerUpgrade.TriggerCondition` member existed but no system raised it, so any upgrade authored against it was inert. `EntityHealth.TakeDamage` now triggers it on the victim's own `PlayerUpgradeManager` whenever the resolved damage is positive. Autopilot is the first upgrade to use it
- **`HasAttack` missed runtime copies** — equipped attacks are `Instantiate`d, so their names carry a `(Clone)` suffix (nested clones stack it) while requirement checks compare against the source asset name; the comparison only trimmed whitespace, so a node gated on an owned attack (such as the new Warp capstone) could never match. Both sides now go through `NormalizeAttackName`, null entries in the list are skipped, and `UpdateAttack` writes the normalized name onto the runtime copy it creates

### Removed
- **`statBuffs` from `SkillNodeDef`** — the legacy `List<StatBuff>` field marked `// TODO: Remove`, superseded by `[SerializeReference] unlockEffects`
- Unused `using CrystalFlux.StatusEffectSystem;` in `EntityHealth` and `using CrystalFlux.ProjectileSystem;` in `PlayerUpgradeManager`
- Stale comment in `Projectile` describing the prefab-movement override that the code below it already documents

## [v0.3.3_1] - 2026-08-30

### Added
- **Wave progress indicator** — the wave label now reads `Wave {n} ({killed}/{total})` instead of just `Wave {n}`. `WaveManager.CleanEnemyList` returns the number of entries it removed and folds that into a new `enemiesKilled` counter, so the count advances as corpses are reaped rather than needing a separate death hook. `enemiesKilled` and `waveMaxTotalEnemies` reset in `BeginWave` (both `WaveManager` and `UnlimitedWaveManager`), and all label writes go through the shared `UpdateWaveText()`, which null-guards `waveText`
- **End-of-wave reward announcement** — new `RollAndAnnounceWaveRewards()` runs when the last enemy of a wave dies, before the 1.5s wind-down. It rolls the occasional wave rewards and the anomaly-completion rewards up front, sums them, and announces the total as a single subtitle (`+2 Rerolls, +1 Skill Point`, correctly singular/plural) via `GameController.SetSubtitleForDuration`. Nothing is announced when the wave grants neither
- **Reward panel title in Unlimited mode** — `UnlimitedWaveManager.TriggerStandardRewards` sets the `GameController` title to `Choose Wave Reward` when the standard reward panel opens

### Changed
- **Wave rewards are rolled once, then applied** — the roll and the grant were previously the same step, so announcing early would have double-rolled. Occasional rewards moved into `RollOccasionalWaveRewards(wave)` writing `pendingOccasionalRerolls` / `pendingOccasionalSkillPoints`; `UpdateOccasionalWaveRewards` now only spends those pending values (rolling lazily if `RollAndAnnounceWaveRewards` never ran) and resets them afterward. `HandleAnomalyRewards` does the same with `pendingAnomalyRerolls` / `pendingAnomalySkillPoints`, falling back to its own `Random.Range(1, 4)` roll when there is no pending value
- **`HandleAnomalyRewards` no longer announces on its own** — its `You gained 1 skill point and {c} reroll tokens!` subtitle is superseded by the combined end-of-wave announcement, so the two no longer compete for the subtitle slot. It also skips `AddSkillPoints` when the pending skill-point count is `0`
- **Spawn loop reaps dead enemies every tick** — `CleanEnemyList()` moved above the `currentEnemies.Count >= maxCurrent` check in both spawn routines (was only called inside the "at capacity" branch). Dead entries are now cleared before the capacity test, so a freed slot is refilled on the same frame instead of one frame later — and the progress counter updates continuously rather than only while the spawner is saturated
- **Boss Rush order** — `BossRush.asset` now runs Lich → Jellyfish → Cultist (was Cultist → Lich → Jellyfish), putting the reworked Cultist attack set last
- **Wave label layout** (`New.unity`) — font size 36 → 30 and width 200 → 250 so the `(killed/total)` suffix fits without wrapping
- **`TODO.md` restructured** — priority buckets (`High` / `Medium` / `Low`) replaced with milestone checklists: **Pre [v0.4.0]**, **Pre [v0.5.0]**, and **Content Updates**. Existing items redistributed; pause menu, map borders, and tilemaps pulled forward into the v0.4.0 list

### Fixed
- **Unlimited waves showed a stale enemy count** — `UnlimitedWaveManager.BeginWave` never seeded the per-wave counters, so the new progress label would have carried the previous wave's totals

### Removed
- **Unused `using CrystalFlux.UISystem;` in `GameController`**

### Credits
- **tiopalada** — [Tiny RPG - Mana Soul GUI](https://tiopalada.itch.io/tiny-rpg-mana-soul-gui) added to `CREDITS.md`
- `Packages/packages-lock.json` — `CrystalFlux-Core` git dependency hash bumped

## [v0.3.3] - 2026-08-30 — Projectile Movement Patterns & Screen-Wide Spawn Lines

Projectiles gain authored flight paths independent of their spawn pattern, and the spawner gains five screen-wide line patterns for bullet-hell style attacks.

### Added
- **`MovementType` on `ProjectileData`** — new `Default` / `Wave` / `Spiral` enum under a dedicated `Movement` header (which `speed` moved into). `Default` keeps the existing orbit / homing / boomerang behavior; the other two drive a parametric path
  - **Wave** — travels along its launch direction while oscillating sideways. `waveAmplitude` is the peak sideways offset in world units, `waveFrequency` is full sine cycles per second
  - **Spiral** — travels an Archimedean spiral outward from its spawn point. `spiralSpacing` is the world-unit gap between consecutive rings; the existing `rotateClockwise` flag picks the winding direction. The angular step is computed from arc length, so travel speed stays constant as the radius grows
- **`Projectile.HandlePatternMovement`** — pattern projectiles integrate position analytically (`patternOrigin` plus elapsed `patternTime`) and drive the rigidbody by setting `linearVelocity` to the delta over `Time.fixedDeltaTime`, so they still collide through the physics system rather than teleporting. The path is re-anchored in `Launch`, so pooled instances never inherit a previous flight
- **Homing coexists with patterns** — a pattern projectile with `followDistance > 0` suspends its path while it holds a target. When that target dies or leaves range, the path re-anchors to the current position and heading (`patternSuspended`) so it resumes forward instead of snapping back toward the spawn point
- **Five spawn patterns for screen-wide lines** — `TopDown`, `LeftRight`, `Diagonal`, `DiagonalReverse`, and `FullX` on `ProjectilePattern`, all built on a shared `SpawnOpposingLines` helper. Each fires two opposing walls of projectiles that converge on the origin (`FullX` fires four, both diagonals): `projectileCount + Random(0, randomCount)` projectiles per side, distributed across `spread` world units perpendicular to travel, jittered by `randomSpread`, offset back by `spread / 2` along their travel direction, and staggered by the usual `minDelay`–`maxDelay` wait

### Changed
- **`Projectile.HandleMovement`** — homing extracted to `TryHome()`, which returns whether a target was acquired and followed. Removes a nesting level and lets the pattern path reuse the same acquisition logic instead of duplicating it
- **Cultist Attack Overhaul** - now uses many of the new attack types - go find them out

### Fixed
- **`StatusEffectManager` no longer throws without an `IStatProvider`** — `cesm` is only assigned if a sibling component implements the interface, but `Update` dereferenced it unconditionally to read `EffectRes`. Effect resistance now falls back to `0` when absent, and `Awake` logs an error naming the GameObject instead of failing silently at the first tick
- **Status effects are cleared on destroy** — `StatusEffectManager.OnDestroy` calls `ClearAllEffects()`, so an entity destroyed mid-effect tears down its display objects and effect state instead of leaking them
- **`StatusEffectCooldownUI` null-guards its stat provider and image** — the same missing-`IStatProvider` case threw from `Update` on both the `isAlive` check and the `EffectRes` duration scale; `cooldownImage` is also null-checked before it is written
- status effect tooltips not updating after stacking
- health and stamina cost calculations on attacks being reversed

## [v0.3.2_1] - 2026-08-29

### Fixed
- **Overhealing no longer exceeds Max HP** — `EntityHealth.ChangeHealth` computed a clamped `targetChange` but then applied the raw `finalAmount` to `currentHp`, so any heal past full permanently inflated the stat (health bar read `120/100`) and stalled regeneration. The stat is now clamped to `MaxHp` when healing, matching the `PlayerResourcePool` idiom
- **`Projectile.HandleSize` no longer throws when the owner has no `IStatProvider`** — the condition `!TryGetComponent(...) && esm.GetStat(aoePct)` dereferenced a null `esm` whenever the owner lacked a stat manager, firing an NRE from `Start()` on every such projectile. It now null-guards `ownerObj`/`pd` and early-returns, keeping the base size
- **`DamagePacketBuilder` no longer throws on projectiles with no `mainAttack`** — `ProjectileData.mainAttack` is optional (e.g. pure `SpawnProjectile` upgrades), but its `.type` was dereferenced unconditionally, crashing the damage pipeline on hit. The attack-type bonus is now computed once and skipped (multiplier `1`) when `mainAttack` is null
- **Gold reroll no longer decrements token counter** — `WaveManager.OnRerollButtonClicked` spent a reroll token (`rerolls--`) even when the player paid gold (`cich.TrySpend`). Now only decrements when a free token is actually used; also null-guards `cich`/`cpsm` in reroll and corrupt flows
- **`PlayerUpgrade.chance` tooltip now matches authoring scale** — tooltip read `chance` as 0–1 (`chance*100%`) but trigger used 0–100; now both are 0–100, matching all authored upgrade assets (`Paradox: 0`, `StellarSurge: 20`, `Supersonic: 100`)
- **Cooldown NRE after attack removed** — `PlayerAttackHandler.RemoveAttack` now clears `lastAttackTimes` for the removed type, and `GetEffCd` null-guards its inputs
- **Input subscription leaks fixed** — `PlayerInputHandler` and `SkillTreeInputToggle` now unsubscribe individual callbacks in `OnDisable` and dispose `controls` in `OnDestroy`
- **Pause-safe regen loops** — `PlayerResourcePool.Update`, `PlayerMovement.FixedUpdate`, and `EntityHealth.RegenHp` now early-return when `Time.timeScale == 0f`, matching the project convention followed by other AI/player loops
- **Decoy upgrade no longer aborts on one bad enemy** — `Decoy.TriggerUpgradeEffect` changed `return` to `continue`; `PlayerLevel` now null-guards `TextIndicatorSpawner.Instance` and `ISkillPointHolder`
- **`StatReduction` now works on `Eff*` stats** — added missing `Apply` cases in `EntityStats` for `EffAtk`, `EffMaxHp`, `EffHpReg`, `EffStReg`, `EffSpd`, `EffInt`, `EffMaxStamina`, `EffMaxMana`, `EffArmor`, they instead reduce the base stat.
- **`ArmorRes` now computes physical mitigation** — was a copy-paste of `EffSpd` (move speed); now returns `EffArmor / (EffArmor + 100)` matching `DamageCalculator`
- **`Paradox.globalDoTCanCrit` wired for inspector upgrades** — added `OnRemove` hook + `Start()` seeding of `OnUnlock` for pre-assigned upgrades; symmetric revocation on removal

## [v0.3.2] - 2026-08-29 — Core Extracted to a Package, Wave Decoupled

`v0.3.1` enforced the assembly boundaries; this release moves `Core` out of the repo entirely and cuts the last system that still reached across one. `Wave` now compiles against `CrystalFlux.Core` alone, which means every system except `Entity` depends on contracts and nothing else.

### Added
- **`com.crystalflux.core` package** — `Assets/scripts/Core` is gone; Unity imports the contracts from [joezhuo2/CrystalFlux-Core](https://github.com/joezhuo2/CrystalFlux-Core) via the git URL in `Packages/manifest.json`. The package ships `.meta` files carrying the original GUIDs, so nothing re-imported as a new asset
- **`IBossBar`** — `Core` contract for `BossBarUI`, whose `Setup(string, IStatProvider)` already spoke only in `Core` types
- **`EnemySpawning`** — `Core` spawn hook. `EnemySpawner` registers itself through `[RuntimeInitializeOnLoadMethod]`, so `Wave` can spawn without naming the spawner. Spawn sites now null-check, since an unregistered hook returns `null`
- **`PlayerEvents.OnPlayerTakeDamage`** — relocated from `EntityHealth`'s own `static event`, typed over `IDamageable`. `NoDamageTrialInstance` subscribes here now
- **`AttackAsset.GetTooltipLines` / `UpgradeAsset.GetTooltipLines`** — abstract description hooks. `RewardButton` was reading ~24 concrete fields off `AttackData` and `PlayerUpgrade` (and its nested `ProjectileData`) to format tooltips; each system now describes its own data instead of exporting its stat schema

### Changed
- **`Wave` references only `Core`** (plus TextMeshPro and uGUI) — down from `Core`, `Entity`, `Projectile`, `StatusEffect`, and `SkillTree`. Its concrete references were swapped for the `Core` interfaces those classes already implemented: `PlayerAttackHandler`→`IAttackHandler`, `PlayerUpgradeManager`→`IUpgradeHolder`, `PlayerSkillTree`→`ISkillPointHolder`, `StatusEffectManager`→`IStatusEffectReceiver`, `BossBarUI`→`IBossBar`
- **`WaveReward`** — `newAttack` and `upgrade` widened from `AttackData`/`PlayerUpgrade` to their `Core` bases `AttackAsset`/`UpgradeAsset`. Widening to a base keeps existing serialized asset references intact
- **`IStatusEffectReceiver`** — gained `DisplayPrefab` / `DisplayContainer` setters, replacing direct writes to `StatusEffectManager`'s public fields
- **`Core` is a single namespace** — everything the package ships is in `CrystalFlux.Core`. The old per-system namespaces it used to contribute (`CrystalFlux.EntitySystem`, `.ProjectileSystem`, `.StatusEffectSystem`, `.UISystem`) no longer exist there

### Fixed
- **Dead `using` directives after the move** — 13 across 10 files. The old local `Core` assembly also declared `CrystalFlux.EntitySystem`, `.ProjectileSystem`, `.StatusEffectSystem`, and `.UISystem`, so assemblies referencing only `Core` were resolving those imports through `Core`'s contribution to them. Folding the package into one namespace left them pointing nowhere
- **Package `.meta` coverage** — `package.json`, `README.md`, and `CHANGELOG.md` shipped without `.meta` files, so Unity logged "has no meta file, but it's in an immutable folder" for each on import


## [v0.3.1] - 2026-08-29 — Compiler-Enforced Assembly Boundaries

The interface-driven decoupling from `v0.3.0` was convention-only — nothing stopped a system from reaching into another. This release splits the codebase into seven assemblies so those boundaries are enforced by the compiler: `Projectile`, `StatusEffect`, and `SkillTree` can now reference **only** `Core`, and an illegal dependency fails the build instead of accumulating silently.

### Added
- **Assembly definitions** — `CrystalFlux.Core`, `.Projectile`, `.StatusEffect`, `.SkillTree`, `.Entity`, `.Wave`, `.TextIndicator`. Dependency graph (verified from compiled assembly metadata):
  ```
  Core ──┬─ TextIndicator ─┐
         ├─ Projectile ────┤
         ├─ StatusEffect ──┼─→ Entity ──→ Wave
         └─ SkillTree ─────┘
  ```
- **Cross-assembly asset contracts** — `AttackAsset`, `UpgradeAsset`, and `EffectAsset`: thin abstract `ScriptableObject` bases in `Core` that `AttackData`, `PlayerUpgrade`, and `StatusEffect` derive from. Unity cannot serialize interface-typed asset fields, so a shared base is what lets `List<AttackData>` cross a boundary without breaking existing asset references — the concrete types keep their assemblies and GUIDs
- **`IAttackHandler` / `IUpgradeHolder`** — `Core` interfaces for `PlayerAttackHandler` and `PlayerUpgradeManager`, implemented explicitly so existing Entity call sites keep their concrete signatures
- **`InputState`** — `Core` holder for the shared mouse position, replacing `PlayerInputHandler.mousePos`. The assembly split exposed that `Projectile` was reading this `public static` field directly out of `Entity`
- **`DamageRoll`** — `Core` home for `RollCrits` and flat damage-packet assembly, so `StatusEffect` no longer needs `Projectile` to deal damage

### Changed
- **`Core` is now contracts only** — gained `AttackType`, `SummonCondition`, `ResourceType`, `StatType`, `StatBuff`, `InputState`, `DamageRoll`, and the new asset bases; `IDamageable` / `IKnockbackable` / `IResourcePool` / `IStatusEffectReceiver` returned to it. Namespaces were deliberately left unchanged (e.g. `AttackAsset` stays in `CrystalFlux.ProjectileSystem`), so only assembly membership moved and no `using` directives churned
- **`IStatusEffectReceiver`** — retyped over `EffectAsset`; `StatusEffectManager`'s generic constraints relaxed to match, narrowing to `StatusEffect` internally
- **`DamagePacketBuilder` / `DamageCalculator`** — flat-damage and crit-roll paths now delegate to `Core.DamageRoll`
- **`Paradox`** — the `DoT` crit check moved from `HasUpgradeOfType<Paradox>()` to the `StatType.globalDoTCanCrit` stat (previously declared but never wired), since `StatusEffect` can no longer name concrete upgrades. `EntityStats` now backs the stat and `Paradox.OnUnlock` grants it
- **`SkillTreeInputToggle`** — moved to `Entity/Player/` so it can still reach the generated `PlayerControls`

### Fixed
- **Skill tree node deserialization** — added `[MovedFrom(sourceAssembly: "Assembly-CSharp")]` to `UnlockEffect` and `NodeRequirement`. `[SerializeReference]` records a literal `{class, ns, asm}` triplet, so moving these types into `CrystalFlux.SkillTree` orphaned the reference in all 104 node assets ("Missing types referenced from component SkillNodeDef")
- **Duplicate `SummonCondition`** — the enum existed in both `Core` and `EntitySummonHandler`, producing `Operator '==' cannot be applied to 'SummonCondition' and 'SummonCondition'`
- **`EntityStatManager.AddStat`** — dropped a dead `IsUnityNull()` guard on `StatBuff`; the extension takes `object`, so the struct boxed and the check was always `false`


## [v0.3.0] - 2026-08-29 — System Refactor & QoL Update (Release Summary)

This release covers the full development arc from `v0.2.0` through `v0.2.19`. Over this period Anamnesis went through a major architectural refactor — nearly every combat, stat, resource, and UI system was decoupled from concrete components onto small interfaces — while the skill tree nearly doubled, two gamemodes were wired into a selector, enemy scaling was reworked, and new attacks shipped.

### Highlights

- **Interface-driven decoupling (`v0.2.9`–`v0.2.19`)** — the core systems were steadily refactored off direct component references onto focused interfaces:
  - **`IStatProvider` / `ICurrencyHolder`** — `EntityStatManager` is no longer referenced directly anywhere (v0.2.10)
  - **`IDamageable`** — unified take-damage / heal / consume pipeline via `DamagePacket`, adding `DamageType.Heal` / `DamageType.Consume`, `bypassIFrames`, and `sizeOverride` (v0.2.11)
  - **`IStatusEffectReceiver`** — status effect application centralized through the `StatusEffectManager` (v0.2.12)
  - **`ITooltipDisplay`** — the entire tooltip system migrated off `TooltipTrigger` onto UI components (v0.2.13)
  - **`IResourcePool` / `IKnockbackable` / `IUnlockEffect`** — plus the unified `PlayerResourcePool` replacing separate `PlayerStamina`/`PlayerMana` (v0.2.14)
  - **`ITeamMember`**, **`IAnnouncer`**, **`ISkillPointHolder`**, **`IUnlockRequirement`**, **`ISummonTrigger`**, **`IOnHitEffect`** — remaining concrete references replaced across the codebase (v0.2.15–v0.2.19)
- **Namespaces (`v0.2.17`)** — most classes now live in `CrystalFlux.Core`, `.EntitySystem`, `.ProjectileSystem`, `.StatusEffectSystem`, `.WaveSystem`, and `.UISystem`
- **Skill tree overhaul (`v0.2.2`, `v0.2.6`, `v0.2.18`)** — nodes grew from 78 → 104; strict AND-based prerequisites became a bidirectional OR connections system; per-node skill-point costs added. The headline change reworked `SkillNodeDef` onto scriptable data: `[SerializeReference] List<IUnlockEffect> unlockEffects` (`StatBuffEffect`/`PlayerUpgradeEffect`/`AttackUpgradeEffect`) and `List<IUnlockRequirement> requirements`, with a custom `TypeSelector` inspector — all 104 node assets now store their effects as data, and unlock/undo logic lives on the effects themselves
- **Unlimited Waves gamemode (`v0.2.3`–`v0.2.4`)** — new `UnlimitedWaveManager` (infinite scaling, faster spawns, periodic boss waves, mixed/milestone reward flow) plus a gamemode selector with `RegularWaveButtonController` / `UnlimitedWaveButtonController`, and tooltip support for the corrupt/reroll/skip action buttons
- **Enemy scaling rework (`v0.2.7`)** — linear → exponential per-level scaling (ATK 5%, HP/regen 10%, armor 5%, move/crit/aoe 4%, resistances 3% per level) so base stats matter; fixed the `levelOffset % 5 == 0` bug that only scaled on every 5th level
- **Stat system overhaul (`v0.2.5`–`v0.2.9`)** — get/set routed through `GetStat(StatType)` / `AddStat(StatBuff)` on `EntityStatManager`; split into player/enemy stat managers (enemies scale to level); new status-effect, resource, and per-attack CDR stats added
- **New attacks & content (`v0.2.1`)** — Ignition Flash (DoT basic that debuffs enemies) and Lifeforce (HP-scaling spell nuke); subtitle feedback when gaining skill points / reroll tokens
- **Robustness & pooling (`v0.2.11`–`v0.2.19`)** — `AttackData.IsRuntimeCopy` so runtime-upgrade copies are never confused with source assets (original ScriptableObjects are never destroyed); skill-tree undo cost/refund and requirement checks fixed; reward buttons fully reset on pooling; crash-level fixes (level system, `PoolPreSetup` on unlimited mode)

## [v0.2.19] - 2026-08-29

### Added
- **`ISummonTrigger` interface** — new interface with `TrySummon(Vector2 position)`; `EntitySummonHandler` implements it, decoupling summon-on-hit from the concrete handler type
- **`DamagePacketBuilder`** — new static class extracting the `BuildDamagePacket` methods from `DamagePacket`, which is now a pure data container
- **`IStatusEffectReceiver.RemoveStacks`** — interface now declares `RemoveStacks<T>(int)`, matching the existing `StatusEffectManager` implementation

### Changed
- **`DamagePacket` / `DamageInstance` / `DamageType`** — moved from `CrystalFlux.ProjectileSystem` to `CrystalFlux.Core` (files relocated `Assets/scripts/Projectile/` → `Assets/scripts/Core/`)
- **`IOnHitEffect`** — moved from `CrystalFlux.ProjectileSystem` to `CrystalFlux.Core`
- **`Projectile`** — no longer implements `IOnHitEffect`; hits now notify ALL `IOnHitEffect` components on the owner, so any owner component can react to projectile hits
- **`PlayerUpgradeManager`** — now implements `IOnHitEffect`, triggering `OnProjectileHit` upgrades through the shared on-hit pipeline
- **Summon-on-hit** — routed through `ISummonTrigger` instead of a direct `EntitySummonHandler` reference

### Updated
- **All 104 skill node assets** — `statBuffs` converted to `UnlockEffect` entries in `unlockEffects` (buffs / attacks / awakenings); `statBuffs` is now empty on every node (field still on `SkillNodeDef`, marked TODO: Remove)

## [v0.2.18_1] - 2026-08-28

## Updated
- scene assets to use the new files
- 1 skill node with the new system (to test, it works)

## ⚠️ [v0.2.18] - 2026-08-28

### Added
- **`IUnlockRequirement` interface** — new core interface with `Has(GameObject target)` for unified skill node unlock requirements
- **`UnlockEffects`** — new serializable `IUnlockEffect` wrappers (`StatBuffEffect`, `PlayerUpgradeEffect`, `AttackUpgradeEffect`) for `[SerializeReference]` lists on `SkillNodeDef`
- **`TypeSelectorAttribute` / `TypeSelectorDrawer`** — inspector dropdown for null `[SerializeReference]` elements on `SkillNodeDef.requirements`/`unlockEffects`, so added elements can be assigned a concrete implementation (e.g. `UnlockEffect`) and configured
- **`AttackData.IsRuntimeCopy`** — non-serialized flag set during `DeepClone`, exposing whether an instance is a runtime copy
- **`PlayerUpgrade` / `AttackData`** — now implement `IUnlockEffect` and `IUnlockRequirement`; `Apply`/`Remove`/`Has` route through `PlayerUpgradeManager`/`PlayerAttackHandler`

### Changed
- **`SkillNodeDef`** — replaced `requiredAttacks`/`requiredPlayerUpgrades` with `[SerializeReference] List<IUnlockRequirement> requirements`; replaced `attackUpgrades`/`playerUpgrades` with `[SerializeReference] List<IUnlockEffect> unlockEffects`; removed `Apply`/`Remove` and all upgrade/downgrade handling (logic now lives on the effects); `statBuffs` deprecated (TODO: remove); `cost`/`undoCost` regrouped under a Costs header with tooltips
- **`PlayerSkillTree`** — `CanUnlock` validates `node.requirements` via `Has()`; `UnlockNode`/`UndoNode` iterate `node.unlockEffects`; removed manual deep-instantiation of node attack/player upgrades in `GenerateRuntimeNodes`/`CleanupNodes`
- **`IUnlockEffect`** — wrapped in `CrystalFlux.Core` namespace

### Fixed
- **Original asset destruction** — `PlayerAttackHandler` (`UpdateAttack`/`RemoveAttack`/`OnDestroy`), `EnemyAttackHandler` (`OnDestroy`), and `AttackReplacement` (`OnDestroy`) now only destroy `AttackData` marked `IsRuntimeCopy`, so source assets are never destroyed at runtime
- **`PlayerUpgradeManager.HasUpgrade`** — name comparison now trims and ignores case, fixing requirement checks failing on name mismatches
- **`PlayerSkillTree.CanUndo`** — always returned false (fallthrough returned "No stat manager found" even on success); now returns true when the player can afford the undo cost
- **`PlayerSkillTree.UndoNode`** — effects were removed even when the gold refund failed; removal now only happens on a successful undo
- **`PlayerSkillTree.CanUnlock`** — null-guarded `node.requirements` (all existing node assets deserialize it as null, crashing every check)
- **`NodeRequirement.Has`** — requirements silently passed when the target lacked `PlayerAttackHandler`/`PlayerUpgradeManager`; now fail, with null-entry guards
- **Legacy `statBuffs`** — still applied on unlock/undo until assets are migrated to `unlockEffects` (all 60+ existing node assets store buffs in the deprecated field)

## ⚠️ [v0.2.17] - 2026-08-28

### Added
- **Namespaces** - most classes now use namespaces from one of the following: `CrystalFlux.Core`, `CrystalFlux.EntitySystem`, `CrystalFlux.ProjectileSystem`, `CrystalFlux.StatusEffectSystem`, `CrystalFlux.WaveSystem`, and `CrystalFlux.UISystem`.

## ⚠️ [v0.2.16] - 2026-08-27

### Added
- **`IAnnouncer` interface** — new core interface for unified title/subtitle announcements (`SetTitleForDuration`, `SetSubtitleForDuration`)
- **`ISkillPointHolder` interface** — new core interface for skill point management (`SkillPoints`, `AddSkillPoints`, `TrySpend`)
- **`SkillNodeDef.cost`** — per-node skill point cost (default 1) replacing fixed cost

### Changed
- **`GameController`** — now implements `IAnnouncer`; announcements route through `IAnnouncer.Current` instead of singleton
- **`PlayerSkillTree`** — implements `ISkillPointHolder`; `skillPoints` field → `SkillPoints` property; unlock cost now uses `node.cost`; added `AddSkillPoints`/`TrySpend` methods
- **`WaveManager`** — references `IAnnouncer` instead of `GameController`; uses `ISkillPointHolder` for skill point rewards; null-conditional calls for announcements
- **`PlayerLevel`** — uses `IAnnouncer.Current` for level-up announcements
- **`PlayerInputHandler`** — removed `PlayerSkillTree` reference and skill tree toggle logic (moved to UI layer)
- **`PlayerAttackCooldownUI`** — added null-safety check for `cooldownImage`

### Fixed
- **Skill tree refund** — now refunds `node.cost` skill points instead of fixed 1
- **Wave reward skill points** — now uses `AddSkillPoints(1)` via interface

## ⚠️ [v0.2.15_1] - 2026-08-27

### Removed
- code counter
- unused recovery assets

## ⚠️ [v0.2.15] - 2024-08-27

### Added
- **`ITeamMember` interface** - replaces `CompareTag` checks

## ⚠️ [v0.2.14_1] - 2024-08-27

### Fixed
- tooltip not updating when the player gets new stats (now updates whenever the player hovers over the tooltip)
- special orbit scaling not working
- level system not working and instead crashing the game

## ⚠️ [v0.2.14] - 2026-08-27
### Added
- **`PlayerResourcePool`** — new unified resource management component implementing `IResourcePool`; consolidates stamina and mana gain/spend/regen logic into a single component (replaces separate `PlayerStamina` and `PlayerMana`)
- **`IKnockbackable` interface** — new core interface with `ApplyKnockback(Vector2 direction, float force, float duration)` for unified knockback handling on players and enemies
- **`IUnlockEffect` interface** - new core interface for unified skill node unlock effects through `Apply` and `Remove` 

### Changed
- **`IResourcePool`** — `Gain` → `TryGain` returning `bool` for consistency with `TrySpend`; removed `Health` from `ResourceType` enum (health now handled via `IDamageable`)
- **`Player`** — requires `PlayerResourcePool` instead of `PlayerStamina`; resource costs/gains now route through `IResourcePool`
- **`PlayerAttackHandler`** — uses `IResourcePool.TrySpend` for stamina/mana costs; removed direct `PlayerStamina`/`PlayerMana` references
- **`PlayerMovement`** — implements `IKnockbackable` for knockback handling
- **`EnemyMovement`** — implements `IKnockbackable`; removed `Unity.Mathematics` dependency
- **`EntityProjectileHandler`** — uses `IResourcePool.TryGain` for stamina/mana gains on projectile hits
- **`Projectile`** — knockback now uses `IKnockbackable` interface; stat gains use `IResourcePool.TryGain`
- **`GainMana` upgrade** — uses `IResourcePool.TryGain(ResourceType.Mana, amount)` instead of `PlayerMana.ChangeMana`
- **`TooltipUI`** — added null-safety checks in `ShowTooltip`/`HideTooltip`
- **`SkillNodeDef`** — now implements `IUnlockEffect`
- **`PlayerSkillTree`** — now calls the `Apply` or `Remove` methods from the selected `SkillNodeDef` instead of handling upgrade/remove logic

### Removed
- **`PlayerStamina`** — entire component removed; logic migrated to `PlayerResourcePool`
- **`PlayerMana`** — entire component removed; logic migrated to `PlayerResourcePool`
- **`ResourceType.Health`** — health no longer treated as a spendable/gainable resource

## ⚠️ [v0.2.13] - 2026-08-27 
### Added
- **`ITooltipDisplay` interface** — new core interface with `ShowTooltip(string title, string subtitle, Vector2 offset)` and `HideTooltip()` for unified tooltip display
- **Tooltip methods on UI components** — `GetSkillTreeTooltip()`, `GetStatusEffectTooltip()`, `GetAttackTooltip()`, `GetDashTooltip()`, `GetStatRewardTooltip()`, `GetAttackRewardTooltip()`, `GetPlayerUpgradeTooltip()`, `GetMilestoneRewardTooltip()` returning `(title, subtitle, offset)` tuples

### Changed
- **Complete tooltip system migration** — replaced `TooltipTrigger` with `ITooltipDisplay` across all UI components:
  - `PlayerAttackCooldownUI`, `PlayerDashCooldownUI`, `PlayerUI` — attack/dash/skill tooltips via `GetAttackTooltip()`/`GetDashTooltip()`/`GetSkillTooltip()`
  - `SkillNodeUI` — skill tree node tooltips with unlock/undo info via `GetSkillTreeTooltip()`
  - `StatusEffectCooldownUI` — status effect tooltips via `GetStatusEffectTooltip()`
  - `RegularWaveButtonController`, `UnlimitedWaveButtonController` — gamemode button tooltips
  - `RewardButton` — comprehensive tooltips for all reward types (stat, attack, player upgrade, milestone)
  - `WaveManager` — action button tooltips (reroll, corrupt, skip) using `ITooltipDisplay`
- **`RewardButton` refactor** — renamed fields (`statRewardData`→`gr`, `attackRewardData`→`ar`, `playerUpgradeRewardData`→`pur`, `milestoneRewardData`→`mrd`); added dedicated tooltip methods per reward type with detailed stat/attack/upgrade info
- **`TooltipTrigger`** — simplified to implement `ITooltipDisplay`; removed old `SetupTooltipData` overloads

### Removed
- Direct `TooltipTrigger` references and `SetupTooltipData` calls throughout codebase
- `TooltipTrigger` component from `SkillNodeUI` (replaced with `ITooltipDisplay`)

## ⚠️ [v0.2.12] - 2026-08-26

### Added
- **`IStatusEffectReceiver` interface** — new core interface with `Apply(StatusEffect, GameObject, Vector2)`, `ClearAllEffects()`, `GetActiveFirstEffectOfType<T>()`, `GetActiveEffectsOfType<T>(List<T>)`, `RemoveEffectAfterDelay<T>(float)`, `RemoveEffect<T>()`
- **`StatusEffect.location`** — replaced `projectile` field with `Vector2 location` for effect application position tracking

### Changed
- **`StatusEffectManager` implements `IStatusEffectReceiver`** — unified status effect application through `Apply()`; removed `AddEffectAfterDelay`, `AddEffect`, `GetEffect`, `RemoveEffect(StatusEffect)`; `projectile` param removed from effect runtime
- **All status effect consumers migrated to `IStatusEffectReceiver`** — `EntityHealth`, `EntitySummonHandler`, `PlayerUpgrades` (Decoy, Reminiscence, SoulRendPU), `Projectile`, `Detonator`, `Pulled`
- **`EntitySummonHandler` on-death effects** — now use `Apply()` instead of `AddEffectAfterDelay`
- **`Projectile` effect application** — uses `Apply()` with location; removed delay coroutine
- **`Detonator`** — uses `GetActiveEffectsOfType<DoT>()` and `RemoveEffect<DoT>()` via interface
- **`Pulled`** — uses `location` for pull center; uses `RemoveEffect<Pulled>()` via interface
- **`SoulRendPU`** — uses `GetActiveFirstEffectOfType<SoulRend>()` and `RemoveEffectAfterDelay<SoulRend>()` via interface

### Removed
- `StatusEffect.projectile` field (replaced by `location`)
- `StatusEffectManager.AddEffectAfterDelay`, `AddEffectAfterDelayCoroutine`, `GetEffect`, `RemoveEffect(StatusEffect)` methods
- Direct `StatusEffectManager` references throughout codebase (replaced with `IStatusEffectReceiver`)

### Fixed
- not being able to heal on attack hits

## ⚠️ [v0.2.11] - 2026-08-26 

### Added
- **`IDamageable` interface** — new core interface with `TakeDamage(DamagePacket)`, `TriggerIFrames(float)`, `IsAlive` property, and `OnDeath` event
- **`DamageType.Heal` and `DamageType.Consume`** — new damage types for healing and resource consumption
- **`DamagePacket` enhancements** — added `source`, `bypassIFrames`, `sizeOverride` fields; updated `BuildDamagePacket` signatures to include bypass/size params

### Changed
- **`EntityHealth` implements `IDamageable`** — unified damage/heal/consume flow through `TakeDamage`; `Alive` → `IsAlive`; `TriggerIFrames` returns `Coroutine`; healing/consuming now use `DamagePacket`
- **All damage consumers migrated to `IDamageable`** — `EntityProjectileHandler`, `EntitySummonHandler`, `PlayerAttackHandler`, `PlayerMovement`, `PlayerUpgrades` (AdditionalDamage, StellarSurge), `Projectile`, `DoT`, `Detonator`
- **`PlayerAttackHandler` resource costs** — health/stamina/mana costs now use `DamagePacket` (Consume type) via `IDamageable.TakeDamage`
- **`TooltipTrigger`** — updated preview damage packet call with new signature
- **`StatusEffectManager`** — removed `[RequireComponent(typeof(IStatProvider))]`

### Fixed
- Dash iframes now correctly use `IDamageable.TriggerIFrames`
- Summon death events use `IDamageable.OnDeath` event
- Damage packet size override and iframe bypass properly propagated

## ⚠️ [v0.2.10] - 2026-08-26

### Changed
- **Complete Decoupling** - `EntityStatManager` is no longer referenced, and replaced by `ICurrencyHolder` and `IStatProvider`

## ⚠️ [v0.2.9] - 2026-08-26 - Stat System Refactor

### Changed
- **Complete stat system overhaul** — replaced direct field access (`esm.s.field`) with `GetStat(StatType)` and `AddStat(StatBuff)` across all entity scripts
- `EntityStatManager` now centralizes stat retrieval/modification; `EntityStats` holds base values and computed getters
- `DamageCalculator.BuildDamagePacket` and `DamagePacket` updated to use new stat API (removed `EntityStats` param, added `canCrit`, `resPen`, `defShred`)
- Status effects (`DoT`, `Detonator`, `Stun`, `Pulled`) now use `GetStat`/`AddStat` for resistances, penetration, shred, and movement/attack/dash flags
- Projectile system (`Projectile.cs`, `EntityProjectileHandler.cs`) migrated to new stat API
- `WaveManager` gold reroll logic simplified using `TrySpend`/`CurrentAmount` on `ICurrencyHolder`
- `PlayerSkillTree` node undo/refund uses `TrySpend` and `CurrentAmount`
- `EntityHealth` major refactor (172 lines) — unified damage/heal flow with new stat system
- `TooltipTrigger` and `PlayerUI` updated for new stat display

### Removed
- Direct `EntityStats` field access patterns throughout codebase (`esm.s.maxHp`, `esm.s.gold`, `esm.s.canMove`, etc.)

### Fixed
- Gold reroll button interactable state now correctly reflects `TrySpend` result
- Status effect duration/resistance calculations now use `EffectRes` and `seDurPct` via `GetStat`

## ⚠️ [v0.2.8_2] - 2026-08-26

### Updated
- folder structure in `README.md`
- folder structure in `data/entity/player`

### Removed
- unlimited wave manager tooltips that area already self explanatory

### Changed
- split `EntityStatManager` into `EntityStatManager` and `EnemyStatManager` (one used for players, the other used for scaling enemies to their level)

## [v0.2.8_1] - 2026-08-26

### Updated
- cleaned up folder structure for wave/anomaly system

## [v0.2.8] - 2026-08-26

### Removed
- deprecated status effects (old debuffs migrated to the new system)

### Updated
- folder structure to be cleaner
- slime now has the same folder structure as the other enemies

## [v0.2.7] - 2026-08-26 - Enemy Scaling Overhaul

### Changed
- **Enemy stat scaling changed from linear to exponential** - base stats now matter significantly
- Attack scales at 5% per level (was +3 flat)
- Max HP and health regen scales at 10% per level (was +12 flat)
- Armor scales at 5% per level (was +4 flat)
- All percentage stats no longer scale (they will be self-buffed)
- Move speed, crit chance, and aoe % scale at 4% per level
- Resistances scale at 3% per level
- **Fixed levelOffset % 5 == 0 bug** - all stats now scale every level instead of only on levels divisible by 5

### Updated
- increased base spawning speed for unlimited wave mode

### Fixed
- mixed pools turning into normal pools on reroll
- cosmic aftermiage not having an indicator`
- status effect indicators appear for normal enemies

## [v0.2.6_1] - 2026-08-26

### Rebalance

## [v0.2.6] - 2026-08-26 - Skill Tree Enhancement Update

### Added
- a *couple* of new skill tree nodes in each section (78 → 104 nodes), introducing many new stats
- subtitle text to inform you when you get skill points/reroll tokens 

### Rebalance
- Bat
 - stopping range (1.5 → 3)
 - attack (2 → 3)
 - Mark size (2 → 2.5)
- Crab
 - Defense Shred (0 → 35)
 - Attack (4 → 3)
 - Attack % (0 → 15)

## [v0.2.5] - 2026-08-25

### Added
- `IOrbitRegister` to replace old orbit registering method
- safety checks in wave manager

### Changed
- get/update stat values now live in entity stats, but can be called by entity stat manager

### Removed
- random visualscripting imports

### Updated
- cleaned up syntax in damage calculator
- cleaned up search in projectile

## [v0.2.4_3] - 2026-08-25

### Updated
- the navbar thing

## [v0.2.4_2] - 2026-08-25

### Updated
- the navbar thing

## [v0.2.4_1] - 2026-08-25

### Added
- something like a navbar near the top of the readme

## [v0.2.4] - 2026-08-25 - Gamemode Selector Update

### Added
- `TooltipTrigger` support for action buttons (corrupt, reroll, skip)
- **`UnlimitedWaveButtonController`** - a button (active by default, placed under `buttonContainer`) that starts unlimited waves mode
- **Unlimited Waves mode** starts via `UnlimitedWaveManager.StartNextWave()` instead of the regular sequence. 
- New `TooltipTrigger` describing the unlimited wave system (infinite scaling, faster spawns, periodic boss waves, endless rewards). Disables itself and the regular wave button once pressed. Never enables themselves again.
- **`RegularWaveButtonController`** - a button (active by default, placed under `buttonContainer`) that starts the regular wave mode.
- **Regular wave mode** starts via the base `WaveManager.StartNextWave()`. 
- New `TooltipTrigger` describing the standard sequence. Disables itself and the unlimited wave button once pressed. Never enables  themselves again.
- `WaveManager` now has access to the action button container, and the individual action buttons

### Fixed
- boss waves being a possiblity in the first wave (unlimited mode)
- `PoolPreSetup()` causing a crash, and not using unlimited wave configuration in that mode
- rerolls consuming more than 1 reroll token
- both the regular and unlimited wave managers being active at once

### Update
- player actions are now enabled in the lobby

### Removed
- `Instance` in wave manager - was unused
- skip button once corrupt button was used
- skip button on game start
- random override methods in `UnlimitedWaveManager` that was the same as the original method
- unecessary `virtual` signatures in `WaveManager`

## [v0.2.3] - 2026-08-25 - Unlimited Waves Update

### Added
- **`UnlimitedWaveManager`** - a new unlimited waves system (implemented only, NOT *yet* wired up to the scene). Inherits from `WaveManager` so all shared settings (reroll cost, wave info, action buttons, corruption, reward panel, reward pools, milestone rewards, anomalies) are reused exactly as configured on the existing `WaveManager` — no reconfiguration needed.
- **Configurable options**: max current enemies, base max total enemies (increases by 1-2 randomly every wave), base enemy level (+1 per wave), min/max spawn frequency with spawn speed increase per wave, min/max reward choices, boss bar prefab / status effect display prefab (used for all bosses), auto-generated boss bar name (`[Lv. {LEVEL}] {BOSSNAME}`), list of all spawnable enemies, list of all spawnable bosses, boss wave chance, additional boss wave chance if previous wave wasn't a boss wave, min waves between boss waves.
- **Wave flow**: every 5 waves spawns mixed rewards only, unless every 25 waves which spawns milestone rewards. No `WaveSequence` used.

### Changed
- `WaveManager` fields/methods made `protected`/`virtual` to support inheritance by `UnlimitedWaveManager` (no behavior change to the existing wired-up system).

## [v0.2.2_1] - 2026-08-25

### Added
- `GAME.md` - summarizes all attacks and player upgrades
- many files (`CLAUDE.md`, `copilot-instructions.md`, `.copilotignore`, `.claudeignore`) to (hopefully) improve ai workflows

## [v0.2.2] - 2026-08-25 - Skill Tree Expansion Update

### Added
- a **TON** of skill nodes (now 78 total nodes), including new starting nodes, connecting nodes, and node bundles for new special stats

### Changed
- **Skill Tree Prerequisites → Connections System**: Overhauled the skill tree unlock logic from a strict AND-based prerequisite system to a flexible bidirectional connections system with OR logic:
  - **Bidirectional connections**: If node A lists node B as a prerequisite, you can now unlock A when B is unlocked **OR** unlock B when A is unlocked
  - **OR logic for multiple connections**: When a node has multiple connected nodes (e.g., B and C both connect to A), only **one** connected node needs to be unlocked (B **OR** C), not all of them
  - **Reverse connections automatically work**: No need to duplicate connections in both directions; the system checks both forward (node's prerequisites list) and reverse (nodes that have this node in their prerequisites)
  - **Improved tooltip feedback**: Fail messages now show "Requires one of: [Node1, Node2, ...]" listing all connected nodes
  - **Field name unchanged**: Still uses `prerequisites` field in `SkillNodeDef` for defining connections

### Fixed
- Nodes with no connections now properly show "Node has no connections" instead of incorrectly requiring prerequisites

### Rebalance
- nerfed a lot of skill nodes

## [v0.2.1] - 2026-08-25

### Added
- `TooltipTrigger` for all reward button types
- Ignition Flash - new DoT basic attack that also debuffs enemies
- Lifeforce - hp scaling spell damage skill nuke, deals more damage based on hp consumed

### Fixed
- enemy health bars being invisible
- reward buttons showing the tooltip of the previously shown attack/player upgrade
- stealing and exp bonus stat rewards not working correctly
- max mana/stamina % increases not showing before and after values of stamina, not stamina %

## [v0.2.0] - 2026-08-24 — Progression, Economy & Milestones Update (Release Summary)

This release covers the full development arc from `v0.1.0` through `v0.1.13_1`. Over this period Anamnesis evolved from a wave-based action game with a basic skill tree into a deep roguelite with layered progression systems, a full currency economy, milestone rewards, and significantly expanded build-crafting depth.

### Highlights

- **Level & Progression System (v0.1.9)** — Enemies now drop XP; collecting XP levels up the player, granting HP, ATK, INT, SPD increases and a skill point per level. Bosses drop significantly more XP. An `EXP Bonus` stat in reward pools accelerates leveling. Level-up indicator added.
- **Gold/Currency Economy (v0.1.11)** — Enemies drop gold on death (15% variance, scaling with Stealing stat). Gold funds skill-node refunds (default 50g, configurable per node) and serves as a fallback reroll currency (200g) when rerolls are exhausted. Floating gold text indicators added.
- **Milestone Rewards (v0.1.12)** — Every 25 waves (25, 50, 75, 100…) players choose from 3 synergistic reward bundles (`MilestoneReward` struct) that combine powerful buffs with meaningful drawbacks. Uses existing reward UI with custom colors; supports rerolls.
- **Stats Extension (v0.1.13)** — Major stat system expansion:
  - **Status Effect Stats**: `sePotPct` (potency %), `seDurPct` (duration %), `seTickRatePct` (tick rate %, min 0.1s interval).
  - **Resource Stats**: `manaGainPct`, `maxManaPct` (via `EffMaxMana`), `maxStaminaPct` (via `EffMaxStamina`).
  - **Per-Attack-Type CDR**: `basicCdRedPct`, `skillCdRedPct`, `ultCdRedPct` (multiplicative with attack speed).
  - `potencyMultiplier` field on `StatusEffect` base class; all new stats integrated into `StatType`, `EntityStatManager`, reward pools, and tooltips.
- **Enemy Splitting & Global Spawner (v0.1.10)** — Enemies can now split into more enemies with configurable settings (count, HP scaling, delay, inheritance). Centralized `GlobalEnemySpawner` for consistent spawn logic.
- **Boss Rush (v0.1.12_1)** — After wave 45, level 50 bosses begin spawning in an endless gauntlet.
- **Corruption System (v0.1.7)** — Once per wave, players can corrupt the reward pool: each button has a 40% chance to become "corrupted," gaining a stat multiplier of +80% to -180%.
- **Title & Subtitle System (v0.1.6)** — Dynamic game title/subtitle with fade in/out, font/color configuration. Wave-complete and boss-killed titles integrated into `WaveManager`.
- **New Attacks & Enemies** — Exodus ultimate (3-stage, ATK/INT/ARMOR scaling, phys/spl/true damage), Supersonic treasure attack, spread barrage pattern, Cultist clone summon & large ball attacks.
- **Skill Tree Polish** — Tier 2 nodes added; node icons overridable per `SkillNodeDef`; undo cost shown in tooltips; right-click → left-click refund; border highlight fixes.
- **Technical Hardening** — Deep-cloning for enemy `AttackData`/skill nodes on spawn; pooled damage/text indicators with fixed sizing; `RewardButton`/`WaveManager` cleanup on destroy; anomaly stat-mod bug fixed (was granting +0%); typo fixes (anamoly → anomaly).
- **Rebalancing Passes** — Multiple waves: enemy HP/speed scaling curves flattened; XP formula nerfed; XP gain nerfed; gold/XP variance increased; Warp, Aphelion, Nirvana, Feedback Loop, Reminiscence, Exodus rebalanced; anomaly chance raised (5% → 15%); Lich buffed.
- **Tooltip & UI Improvements** — Resource tooltip hover zone expanded; status effect display repositioned; speed rounding fixed; skill tree undo cost display; attack/upgrade tooltips on reward buttons.

### Rebalance
- **Nirvana**
  - 620%S > 550%S
  - +30%S/orbit > +20%S/orbit
  - kbForce: 8 > 12

## [v0.1.13_1] - 2026-08-24

### Added
- credits section

## [v0.1.13] - 2026-08-24 - Stats Extension Update

### Added
- **New Status Effect Stats**:
  - `sePotPct` (Status Effect Potency %): Increases damage/severity of all status effects the player applies
  - `seDurPct` (Status Effect Duration %): Increases duration of status effects the player applies
  - `seTickRatePct` (Status Effect Tick Rate %): Makes status effects trigger OnTick() more frequently (reduces tickInterval multiplicatively, min 0.1s)
- **New Resource Stats**:
  - `manaGainPct` (Mana Gain %): Increases all mana gain by a percentage amount
  - `maxManaPct` (Max Mana %): Increases max mana by a percentage; uses new `EffMaxMana` computed property
  - `maxStaminaPct` (Max Stamina %): Increases max stamina by a percentage; uses new `EffMaxStamina` computed property
- **New Cooldown Reduction Stats** (per attack type, multiplicative with attackSpeedPct):
  - `basicCdRedPct` (Basic Attack Cooldown Reduction %)
  - `skillCdRedPct` (Skill Cooldown Reduction %)
  - `ultCdRedPct` (Ultimate Cooldown Reduction %)
- `potencyMultiplier` field to `StatusEffect` base class for effects to use
- All new stats to `StatType` enum, `EntityStatManager.GetStat/AddStat`, and `StatBuff.ToString()`
- All new stats added to either the standard/mixed modifier pools
- Torturer milestone reward (massive dot damage buff)
- fixed some formatting on changelog entries

### Updated
- `PlayerAttackHandler`, `PlayerMana`, `PlayerStamina`, `PlayerUI`, `TooltipTrigger` now use new `EffMaxMana`/`EffMaxStamina` stats
- `StatusEffectManager` now applies source's `sePotPct`, `seDurPct`, `seTickRatePct` when applying effects
- split certain larger files into smaller ones
- code counter
- graphify nodes

## [v0.1.12_1] - 2026-08-24

### Rebalance
- **Warp** 
  - count: 3-5 > 2-4
  - dura 11s > 10s
  - cd 8s > 9s
  - size 2.5 > 2
  - orbit rad 1 > 1.25
- **Warp Rift**
  - 265%s > 215%s
  - size 1.5 > 1.25
- **Aphelion**
  - cd 1.4s > 1.6s
  - dura 12s > 10.5s
  - size 2.25 > 2
  - 45%s > 35%s
- **Feedback Loop**
  - speed 14 > 16
  - 20%s > 15%s
- **Reminiscence**
  - chance 30% > 25%
  - cooldown 3s > 4s
  - delay 0.25s > 0.35s
- **Feedback Loop**
  - cooldown 0.2s > 0.3s
  - chance 100% > 70%
  - increased cultist and cultist clone health

### Added
- boss rush now starts at the end of wave 45 (lv. 50 bosses)

## [v0.1.12] - 2026-08-24 - Milestone Rewards Update

### Added
- **Milestone Rewards system**: Every 25 waves (25, 50, 75, 100...), players choose from 3 synergistic reward bundles that combine powerful buffs with meaningful drawbacks
- `MilestoneReward` struct (serializable, inspector-friendly) with base stat buffs, display color, icon, weight, and variance (±15% default)
- Milestone rewards replace regular rewards at milestone waves, using the existing reward UI with custom colors
- Reroll support for milestone rewards
- old anamolies (i removed for testing) back into the anamoly pool

### Rebalance
- increased enemy attack and armor scaling with level
- increased xp required to level up
- increased anomaly chance (5 > 15)
- buffed lich

### Fixed
- anamoly (stat mod in particular) having no value (always grants `+0%` stats)
- text indicators having their size changing relative to previous indicator size (since they are pooled) instead of scaling with default size

## [v0.1.11_1] - 2026-08-24

### Rebalance
- xp dropped nerfed, but variance increased (15% > 20%)
- xp required for levelling up increased
- gold variance increased (15% > 30%)

### Updated
- resource tooltip ui hover zone
- status effect display location

### Fixed
- speed rounding in `TooltipTrigger`
- not being able to undo unlocked nodes (revamped right click to left click)

## [v0.1.11] - 2026-08-24 - Currency Update

### Added
- Gold/Currency system: enemies now drop gold on death (15% variance, same as XP); gold used to refund skill tree nodes and buy rerolls when out of rerolls
- Stealing stat:increases gold drop rate by `{stealing}%` from all enemies; obtainable from the mixed reward pool
- Gold text indicators: floating `"+{gold}g"` text in gold color when earning gold
- Reroll with gold: spend 200 gold to reroll rewards when no rerolls remain; reroll button shows "200g" when affordable
- Skill node undo cost display: tooltip shows undo cost at bottom for unlocked nodes (default 50g, configurable per node)

### Updated
- `EntityHealth.cs` - gold drop logic on enemy death with stealing bonus
- `TextIndicator.cs` and `TextIndicatorSpawner.cs` - gold indicator support
- `WaveManager.cs` - gold reroll fallback and UI update
- `TooltipTrigger.cs` / `SkillNodeUI.cs` - undo cost in skill tree tooltips

## [v0.1.10_3] - 2026-08-24

### Added
- claude mem

### Updated
- readme
- roadmap
- graphify nodes

## [v0.1.10_2] - 2026-08-23

### Rebalance
- massively nerfed xp gain from all enemies (roughly 75% nerf on common enemies, 25% on bosses)
- enemy xp formula: `base * 1.1^(level - 1)` > `base * 1.07^(level - 1)` (nerf)
- nerfed chance to get and strength of xp bonus stat from reward pools (was too op)
- increased xp required as level increases

### Fixed
- skill node dependencies

### Update
- renamed damage indicator to text indicator
- graphify nodes
- cleaned up changelog (# => ##, ## > ###, added `` to references)

### Added
- xp wrapper option for damage indicators
- xp gain indicator when xp is gained (on enemy kill)

## [v0.1.10_1] - 2026-08-23

### Added 
- option for projectiles to follow their source objects exactly

### Updated
- graphify nodes

## [v0.1.10] - 2026-08-23

### Added
- enemy splitting behavior: enemies can now split into more enemies (with many configurable settings)
- global enemy spawner

## [v0.1.9_2] - 2026-08-23

### Changed
- increased default pool size for damage indicators

## [v0.1.9_1] - 2026-08-23 

### Added
- `ROADMAP.md` to act as a summarized versiopn of `CHANGELOG.md` that summarizes major updates

### Updated
- `README.md` now references `ROADMAP.md`

### Fixed
- skill node border highlights not updating when a node is clicked

### Removed
- entries on `TODO.md` that are already completed

## [v0.1.9] - 2026-08-23 - Level Update

### Added
- level system: enemies drop xp, collect xp to level up, levelling up increases hp, atk, and int, and speed by small amounts, and grant a skill point
- enemies now drop xp: common wave enemies do not drop a ton of xp, but bosses do, and there is also a 15% randomness from enemy base exp. 
- take the exp bonus stat from reward pools (mixed/standard) to gain even more xp to level up faster
- level up indicator
- more enemies spawn after wave 10, as they spawn in larger chunks
- tier 2 skill tree nodes

#### Updated
- graphify nodes
- readme
- todo list
- number of enemies that spawn at once is now less

#### Fixed
- player skill tree reference in `PlayerInputHandler` being assignable
- max current enemies not being fixed

## [v0.1.8_6] - 2026-08-22

### Added
- new planned features
- removed testing conditions
- graphify cache

## [v0.1.8_5] - 2026-08-22

### Fixed
- enemies now deep clone and use runtime instance of SO when they spawn
- player attack data is now cleaned up when destroyed
- skill tree nodes are now cleaned up before creation and when destroyed
- reward button pooling now resets all data
- wave manager cleans up anomalies, stops courotines, and clears reward buttons when destroyed
- fixed cultist animations (did not have transition from attack to idle) and removed test settings (fireball dealt no damage)
- slime not being able to attack

### Changed
- moved damage indicator spawner and projectile spawner to GameController

## [v0.1.8_4] - 2026-08-22

### Fixed
- enemies now cache player, meaning that unless the player they are targeting dies, they will be locked onto that player (they choose the closest player at spawn/target death)
- added `KnockbackHandler` to prevent duplicate knockback handling
- canvas caching for entity health bars
- pooled damage indicators
- fixed typo in anamoly => anomaly word

## [v0.1.8_3] - 2026-08-19

### Fixed
- animationLength not being measured from the attack start, causing enemies not moving after attacking to stay in the attack animation loop
- readme

## [v0.1.8_2] - 2026-08-18

### Fixed
- rewards being corrupted but not showing stat modifiers
- being able to reroll after corrupting
- max health increasing health regen by 1% per 1 hp over 100 (enemies would have insane health regen if they had any base health regen at all) - now completely removed

## [v0.1.8_1] - 2026-08-18

### Fixed
- enemies having too much health regen
- nerfed jellyfish and cultist

## [v0.1.8] - 2026-08-18 - Content Update v3 - Part II

### Added
- spread barrage attack type
- cultist spawn clone attack, and large ball attack

## [v0.1.7_2] - 2026-08-18

### Updated
- code counter
- performance test

#### Note
- ignore the incorrect version labels on `v1.0.7` and `v1.0.7_1`, they are emeant to be `v0.1.7` and `v0.1.7_1`, respectively :/

## [v0.1.7_1] - 2026-08-18

### Fixed
- being able to use the corruption button on rare reward and awakening stages (multiples of 5)
- player UI being shown without the game starting
- tooltip sections not showing up for anything except the skill tree

## [v0.1.7] - 2026-08-18

### Added
- Corruption button: can be used once per wave. When used, each button has a 40% chance to become "corrupt," recieving a stat boost by up to +80%, but downwards of -180%.
- Game start button and title screen.

## [v0.1.6_1] - 2026-08-15

### Fixed
- game objects meant to be disabled at the start are now done so in the editor as well
- updated title and subtitle texts in the editor to have respective texts to make them easier to find
- fixed changelog header format from bigger for less important > smaller for less important
- code redundency in `TooltipTrigger.cs`
- attacks that absorb orbits rely on a set multiplier by the player, and not the attack
- removed redundent method in `EntityProjectileHandler.cs` and renamed Count to OrbitCount for clarity
- reroll and skip buttons having set locations, updated to use a horizontal layout group

## [v0.1.6] - 2026-08-13 - The Titles Update

### Rebalance
#### Exodus
- Cooldown: 8s > 90s
- Stamina Cost: 0 > 40, 40% > 55%
- Mana Cost: 60% > 40%
- First: 550%P > 1365%P
- Second Chance: 30% > 60%
- Second: 380%S > 880%S
- Third Chance: 30% > 40%
- Third: 110%T > 560%T
- Third Scaling: EffArmor > ResPen

### Added
- Game title and subtitle, with methods to update each text seperately, and options for fading in/out, and configure font and text color
- wave manager now has wave completed and boss killed title texts
- WaveManager now has an instance

### Update
- WaveManager's EndWave method is now cleaner, having some functions split into sub methods
- methods in wave manager that did not need to be public are now private

## [v0.1.5] - 2026-08-10

### Rebalance 
**Enemies**
- hp% per level: 8 > 4
- move speed %: +10 per 5 waves (add) > +3% per wave (multiply) 
- max move speed %: 200% > 100%

### Added
- a to-do list for upcoming features
- stat modifier anamoly (+5-30% for waves 5-50, +10%-60% for waves 51-105) that grants all enemies in the wave a percentage buff to a common stat (attack, hp, move speed, armor, damage)

### Fixed
- enemies that have negative base walk speed % instantly jumping to 0% because of level scaling

## [v0.1.4] - 2026-08-10

### Added
- player gains a skill point every 5 waves
- New Ultimate: Exodus (atk/int/armor scaling, respectively), deals (phys/spl/true damage, respectively), 30% chance to trigger next stage (3 stages total) on each stage, size increases with each stage, added to Rare Pool

## [v0.1.3] - 2026-08-10

### Added
- stat reward buttons now show before and after stat values

### Fixed
- reward button name and description texts overlapping with long stat names

## [v0.1.2] - 2026-08-10

### Added
- Skill Tree node icons can now be set either by changing the icon image in the prefab (old) OR changing the icon image in the skill node def (new, overrides old)
- 4 new skill nodes
- option for status effects to remove all stacks when the timer expires (default false)

### Fixed
- Projectiles with non-zero `timeBeforeSameEnemy` not being able to trigger additional attacks multiple times as well
- Heartburn stacks decreasing one by one

### Removed
- node prefab (duplicate a pair of node GO + node def to create new nodes instead)

## [v0.1.1_1] - 2026-08-09

### Fixed
- Skill tree not closing when pressing the toggle skill tree button
- Blaze replacing skill instead of basic attack

## [v0.1.1] - 2026-08-09 

### Added
- README.md file

## [v0.1.0] - 2026-08-09 — Release Summary

This release covers the full development arc from the initial `v0.0.1` build through `v0.1.0-PR3`. Over this period Anamnesis grew from a core wave-based combat prototype into a much deeper action game with a full progression system.

**Highlights:**

- **Initial combat foundation (v0.0.1)** — Launched with the Warp, Blaze, Reminiscence, and Serenade abilities, the Attack Replacement / Blaze Soul / Heartburn status effects, the Cultist enemy with clone summoning, stun mechanics, and crit-based upgrade triggers.
- **Status effect & data robustness (v0.0.2 – v0.0.9)** — Added delayed effect application to fix initialization null-reference bugs, deep-cloning for `AttackData`, random projectile directions, and enemy retargeting fixes.
- **Reward & anomaly tuning (v0.0.9 – v0.0.9_2)** — Reworked reward pools and rebalanced anomaly frequency and counts.
- **Upgrade system expansion (v0.0.10 – v0.0.13)** — Player upgrades no longer require inheritance, added dash/attack cooldown advancement upgrades, and removed attack speed from the pool.
- **Wave & combat depth (v0.0.12 – v0.0.19)** — Extra enemy spawns every 10 waves for better clearing (was too slow), randomized damage indicators time, configurable orbit interactions, one-time additional-attack triggers, rare stat pool rework, and new additional-attack damage scaling.
- **Dash & Knockback systems (v0.0.16 – v0.0.21)** — Rebalanced the dash and added a full knockback system for players and enemies, including knockback resistance and knockback % stats.
- **New content & balance (v0.0.22 – v0.0.23_3)** — Added the Supersonic attack, cooldown indicators, Astral Nova vulnerability debuff, and numerous balance changes.
- **Skill Tree Update (v0.1.0-PR1 – PR3)** — The headline feature: a fully interactive skill tree currently with 5 nodes, prerequisites, skill-point currency, pan/zoom navigation, tooltips, and connector lines, plus pause-safe behavior and build fixes.

### Fixed
- Skill tree pan/zoom now works. Root causes: the `SkillTreePanZoom` GameObject had no raycastable `Graphic` (so the EventSystem never delivered drag/scroll events), its `contentRect` pointed at its own `RectTransform` instead of the `NodesContainer`, and the `InputSystemUIInputModule` had no scroll-wheel/right-click/middle-click actions bound. `SkillTreePanZoom` now reads the mouse directly via the Input System (`Mouse.current`) in `Update()` — Alt+Left / Alt+Right / Middle drag to pan, mouse wheel zooms (with zoom-to-cursor), and the node container is auto-resolved. Debug logs are enabled via the `debugLogs` field.

## [v0.1.0-PR3_1] - 2026-08-09

### Fixed
- Reparented the player HUD canvas into the main scene canvas hierarchy so the UI renders correctly in the built game.

## [v0.1.0-PR3] - 2026-08-09

### Fixed
- Skill tree can no longer be opened while the game is already paused (e.g. during reward/anomaly menus).
- Skill tree tooltip no longer draws behind skill nodes.
- Null-safe guard on skill tree line re-render calls.

## [v0.1.0-PR2] - 2026-08-09

### Fixed
- Player can no longer dash while the skill tree is open.
- Enemies can no longer move or attack while the skill tree is open (paused).
- Game no longer renders a blank screen in the built version.
- Wave counter misalignment in the top right.
- Resource bar alignment in the top left.

### Added
- Windows build profile (`Assets/Settings/Build Profiles/Windows.asset`).
- Updated Universal Render Pipeline settings for builds.

## [v0.1.0-PR1] - 2026-08-09 — Skill Tree Update

### Added
- Fully functional skill tree system:
  - New `SkillTreeManager`, `PlayerSkillTree`, `SkillTreeUI`, `SkillNodeUI`, `SkillTreeLineRenderer`, `SkillTreePanZoom`, `SkillTreeDefinition`, and `SkillNodeDef` components.
  - Progression with 5 skill node features, prerequisites, required attacks/upgrades, and incompatible nodes.
  - Node unlocks grant stat buffs, attack upgrades, and player upgrades.
  - Interactive UI with tooltips, locked/available/unlocked visuals, and connector lines.
  - Pan & zoom navigation over the skill tree.
  - New "skill point" currency.
- Player input now routes skill-tree open/close.

## [v0.0.23_3] - 2026-08-06

### Changed
- Nerfed enemy stat scaling.

## [v0.0.23_2] - 2026-08-06

### Changed
- Reward buff updates and rarity buffs.
- Astral Nova now applies vulnerability (-20% damage resistance, max 2 stacks).

## [v0.0.23_1] - 2026-08-06

### Changed
- Blaze moved from skill to basic attack.

## [v0.0.23] - 2026-08-06

### Added
- Supersonic cooldown indicator.
- Reminiscence cooldown image.

### Changed
- Heartburn max stacks: 15 → 10.
- Fixed text issues.

## [v0.0.22] - 2026-08-06

### Added
- New "Supersonic" player attack (Treasure Pool) with animation, projectile data, and prefab.

### Changed
- Lich balance: stopping distance 1.5 → 3; wave range 3 → 8; plant range 3 → 2; ball range 5 → 2.
- Renamed the "Exodus" attack-replacement status effect to "Blaze Soul".

## [v0.0.21] - 2026-08-05 — Knockback Update Part 2

### Added
- Knockback resistance and knockback % stats.
- New stats added to the reward pool and new stat localization.

### Changed
- Movement scripts updated to handle the new stats.

## [v0.0.20] - 2026-08-05 — Knockback Update

### Added
- Knockback implemented for all attacks that should have it, for both players and enemies.
- All enemies and players now use a dynamic Rigidbody2D so they can receive knockback.

## [v0.0.19] - 2026-08-05

### Changed
- Negative health regeneration is now allowed.

### Added
- New stat that increases damage dealt by additional attacks.

## [v0.0.18] - 2026-08-05 — Rare Stats Update

### Changed
- Rare stats moved into mixed reward pools.
- Fixed anomalies starting to generate at wave 7 (wave 5 was intended).
- Rarities from Rare and beyond are now rarer.

## [v0.0.17] - 2026-08-05

### Fixed
- Additional attacks can now only trigger the first time a projectile hits an entity.

## [v0.0.16_2] - 2026-08-05 — Dash Update Part 2

### Added
- Added a method for dash advancement.

## [v0.0.16] - 2026-08-05 — Dash Update

### Changed
- Default dash balance: multiplier 6 → 4; cooldown 2.5s → 4s; distance 1.75 → 1.5; stamina cost 25 → 35.

## [v0.0.15] - 2026-08-04

### Changed
- Minor balance tweaks (Blaze projectile, entity health/stats, Heartburn).

## [v0.0.14] - 2026-08-03

### Changed
- Number of orbit projectiles interacted with can now be configured (default: all).

## [v0.0.13] - 2026-07-31

### Added
- Dash action advance (`DashAdvance`).
- `CooldownAdvance` player upgrade (advance all/basic/skill/ultimate cooldowns).

### Changed
- Removed attack speed from the reward pool.
- Refactored "advance all cooldowns" to reuse the single-cooldown advance logic.
- Fixed Crumbling effect: reduces armor by 10% per stack (previously displayed 10% but applied 15%).

### Fixed
- "Advance all cooldowns" no longer throws `InvalidOperationException` (now snapshots attack type keys).

## [v0.0.12] - 2026-07-31

### Changed
- Heartburn max stacks: 30 → 15.
- Blaze stamina cost: 17 + 8% → 20 + 10%.
- Every 10 waves, each spawn tick now spawns 1 additional enemy.
- Damage indicator options (size, lifetime, speed, delay) now have a small amount of randomness.

## [v0.0.11] - 2026-07-30

### Changed
- Maximum attack cooldown reduction from attack speed nerfed from 90% to 70%.
- Minor wave manager method changes.

## [v0.0.10] - 2026-07-30

### Changed
- Player upgrades no longer need to inherit anything (refactor).

## [v0.0.9_2] - 2026-07-29

### Changed
- Anomaly chance nerfed to 15%; anomaly count 2–5 → 1–6.

## [v0.0.9_1] - 2026-07-29

### Added
- Reward pool updates.

### Changed
- Anomaly chance reduced from 20% to 15%.

## [v0.0.9] - 2026-07-29

### Changed
- Scene and config tweaks (Serenade config, Pulled status effect asset).

## [v0.0.8_2] - 2026-07-29

### Changed
- Exodus & Heartburn duration: 5s → 8s.
- Heartburn is now a buff (buff: false → true).

## [v0.0.7_1] - 2026-07-29

### Fixed
- Enemies can now retarget after their current target moves out of detection range.

## [v0.0.7] - 2026-07-29

### Changed
- Blaze: hits 1 → 2 (0.5s hit rate); size 2 → 2.5.

## [v0.0.6] - 2026-07-29

### Changed
- Added a small delay to damage indicator spawns to make damage bursts more satisfying.

## [v0.0.5] - 2026-07-29

### Added
- "Add effect after delay" function to the status effect manager.

### Fixed
- All status effects now apply with a 0.1s delay, fixing null reference errors when effects were added before the entity finished initializing.
- Fixes AttackReplacement instantly replacing an attack dealing no damage.

## [v0.0.4] - 2026-07-29

### Added
- `AttackData` deep clone method that clones all related ScriptableObjects and references.
- Skill tree and AttackReplacement now use the deep clone.

## [v0.0.3] - 2026-07-29

### Changed
- Aphelion balance: stamina gain 2 → 1; mana gain 1 → 2%; 60%S → 45%S.

## [v0.0.2] - 2026-07-29

### Added
- Random direction option.

### Fixed
- Status effect manager no longer tries to remove null effects.

## [v0.0.1_1] - 2026-07-29

### Changed
- Pushed Exodus and Heartburn changes.

## [v0.0.1] - 2026-07-29 — Initial Release

### Added
- Warp ability (with rift chance).
- Blaze, Reminiscence, Serenade (additional damage upgrade), Attack Replacement, Exodus, and Heartburn status effects.
- New stats: additional true damage, stamina cost %, and on-crit upgrade trigger condition.
- Feedback loop upgrade (grants mana, deals less damage).
- Cultist enemy with clone summoning, basic projectile attack, and stun-on-summon-death.
- Stun prevents enemy movement/attacks/animations.
- Projectile spawn fixes for CIRCLE pattern and orbit-self attacks.

### Changed
- Warp nerf: rift chance 20% → 15%.