## Roadmap

Summarized major feature updates, newest first. Upcoming work lives in the milestone checklists in `TODO.md`.

### [v0.5.0]
- [v0.4.6] **Hypernova**: a Supernova capstone that upgrades the attack in place — bigger, higher pierce, and it now pulls enemies in, self-buffs with Celestial Protection and can stun. Ships with 9 supporting nodes (137 to 147) across a new added-physical-damage chain and two Defense III branches that tie the armor and regen trees together. `Escape` now closes the skill tree instead of opening the settings menu. Rebalances: Blaze family, Cyclone Cleave, Sacred Surge, Exsanguinate, enemy attack scaling, unlimited-wave enemy counts
- [v0.4.5] **Golem**: the fourth boss — a slow, heavily armoured bruiser whose moveset unlocks with its `EnemyPhase` stages: Slam and Cross from the start, Charge below 80% HP, and a self-orbiting stone barrage below 40%. Its Charge is `selfApply`, stunning the Golem for 3s and sometimes buffing its attack by 40%. Ships with the `ws_4` wave sequence (waves 46-60, Golem at 60) and a slot in the Unlimited boss rotation. Enemy assets were also split into `Bosses/` and `Enemies/`, and `WaveManager`'s rarity table was resynced with `UnlimitedWaveManager`'s
- [v0.4.4] **Ironman Mode**: an opt-in home-screen toggle that removes every take-back from a run — rerolls are forced to 0 and their button hidden, corruption is unavailable, and skill node refunds are refused. It persists to `settings.json` (`ironmanMode`) and locks in when a gamemode button is pressed, the same way the difficulty selector does. Outside Ironman, starting skill nodes became refundable and re-pickable. Also fixed: orbit interactions and the `OnCast` summon roll no longer fire on press before the tap/hold split is known
- [v0.4.3] **Difficulty selector**: a home-screen cycler that picks Easy / Normal / Hard before the run starts, persisted to `settings.json` and locked in when a gamemode button is pressed. Each difficulty is a `DifficultyData` asset holding additive offsets for enemy scaling, reward count and quality, occasional wave rewards, anomalies, corruption and economy, read at the point of use so Normal is byte-for-byte the old behaviour. Easy also grants pre-run free picks — a reward panel of rare/treasure choices before wave 1, reusing the existing reward flow via the new `RewardType.PreRun`
- [v0.4.2_2] **Sacred Surge & input buffering**: a new armor-scaling rare-pool Skill that cleanses debuffs and can be held to sustain its field; attacks pressed during a cast or charge are now queued and fired on release (`maxQueuedAttacks`, `queueExpiry`); floating numbers abbreviate to k/M and moved to a `TextType` enum; `enableExtraSpawns` lets unlimited waves spawn one enemy per tick. Rebalances: Supernova, unlimited-wave rarity weights
- [v0.4.2] **Overhealth & attack chains**: overhealth — health held above `EffMaxHp`, spent before health and decaying over time — plus the `overhealth` and `healingPct` stats (Core v0.9.0). Four new treasure-pool upgrades: Exsanguinate, Terminal Cascade, Cresendo and Tempo.
- [v0.4.1] **Settings menu**: an `Escape`-opened pause panel with live gameplay toggles and full keyboard rebinding, persisted to `settings.json` by the new `CrystalFlux.Settings` assembly.
- [v0.4.0] **Object pooling Expansion**: one shared static `PrefabPool` (prefab-keyed, capped, with an opt-in `IPoolable` reset hook) now backs health bars, status effect icons, cast bars, damage numbers, reward/anomaly buttons and projectiles; `Projectile.Start` became `Setup`. Surfaced eight latent reuse bugs, including per-frame health-bar rebuilds and an unbounded damage-number pool. Enemies remain unpooled

### [v0.4.0] - Content Updates
- [v0.3.13] **Charged attacks**: hold-to-charge attacks via the `AttackData` charging block (`canCharge`, `chargeThreshold`, `minChargeTime`/`maxChargeTime`, `chargeTickInterval`, `chargeAttack`), sustained by ticked projectile lifetimes and the new `IChargeRegister`; plus `MovementType.FollowCursor`, the `corruptionSpecialPool` corruption outcome, and the chargeable Ultimate Nocturnis. Rebalances: Blood Pact, Lifeforce Burst, level-up rewards, rare pool unlock waves.
- [v0.3.12] **Cast time**: interruptible attack windups with a pooled, entity-following cast bar; `castTime` and `canMoveWhileCasting` on `AttackData`, plus the `castTimeRedPct` and `interruptResist` stats (Core v0.8.0)
- [v0.3.11] **Intelligence & AOE branches**: 11 new skill tree nodes (114 to 125). Capstone nodes now consume the Awakening they require. Rebalances: Blaze, Blood Pact, Cultist.
- [v0.3.10] **Solar Wind**: `SolarWind` Awakening 6 ew skill tree nodes (108 to 114) Reblanace: Cosmic Superimposition
- [v0.3.8] **Enemy variants & wave gating**: two new slime variants — Magma and Frost. Rewards and enemies both gained a `minWave`.
- [v0.3.5] **Content Expansion Update**: Luminaria, Cosmic Superimposition Capstone, plus two movement-speed nodes (105 to 108 nodes).
- [v0.3.3_2] **Capstone skill nodes**: first capstone node (Warp) — a node gated on owning an attack that swaps it for an upgraded variant; plus the Autopilot Awakening and a working `OnTakeDamage` upgrade trigger
- [v0.3.3] **Projectile movement patterns**: `MovementType` (`Wave` / `Spiral`) on `ProjectileData` giving projectiles authored flight paths that coexist with homing, plus five screen-wide converging-line spawn patterns (`TopDown`, `LeftRight`, `Diagonal`, `DiagonalReverse`, `FullX`)
- [v0.3.2] **Core as a package**: `Assets/scripts/Core` extracted to [joezhuo2/CrystalFlux-Core](https://github.com/joezhuo2/CrystalFlux-Core), imported by Unity from its git URL; `Wave` decoupled onto `Core` alone via `IBossBar`, `EnemySpawning`, `PlayerEvents`, and the `GetTooltipLines` hooks

### [v0.3.0] - System Refactor & QoL Update
- [v0.2.19] **Damage & On-Hit Pipeline**: new `DamagePacketBuilder` extracts damage-packet building; new `ISummonTrigger`; `IOnHitEffect` moved to Core; `PlayerUpgradeManager` now reacts to projectile hits through the shared on-hit pipeline
- [v0.2.18] **Skill Tree Scriptable-Data Overhaul**: `SkillNodeDef` reworked onto `[SerializeReference] unlockEffects` (`StatBuffEffect`/`PlayerUpgradeEffect`/`AttackUpgradeEffect`) and `requirements` (`IUnlockRequirement`) with a custom `TypeSelector` inspector; all 104 node assets migrated, with runtime-copy protection so source assets are never destroyed
- [v0.2.17] **Namespaces**: most classes organized under `CrystalFlux.Core`, `.EntitySystem`, `.ProjectileSystem`, `.StatusEffectSystem`, `.WaveSystem`, `.UISystem`
- [v0.2.12]–[v0.2.16] **Interface-driven systems**: `IStatusEffectReceiver`, `ITooltipDisplay` (tooltips off `TooltipTrigger`), `IResourcePool`/`IKnockbackable`/`IUnlockEffect` (unified `PlayerResourcePool` replacing `PlayerStamina`/`PlayerMana`), `ITeamMember`, `IAnnouncer`, `ISkillPointHolder`
- [v0.2.10]–[v0.2.11] **`IDamageable` & full decoupling**: unified take-damage / heal / consume pipeline via `DamagePacket`; direct `EntityStatManager` references removed in favor of `IStatProvider` / `ICurrencyHolder`
- [v0.2.9] **Stat System Overhaul**: `GetStat(StatType)` / `AddStat(StatBuff)` API; player/enemy stat manager split; new stats
- [v0.2.7] **Exponential enemy scaling**: linear → exponential per-level stat scaling so base stats matter
- [v0.2.2, v0.2.6] **Skill tree expansion**: 78 → 104 nodes; bidirectional connections system with OR logic (prerequisites work both ways, only one connected node needed); per-node skill-point costs
- [v0.2.4] **Gamemode Selector Update**: `UnlimitedWaveButtonController` + `RegularWaveButtonController` (under `buttonContainer`) to pick between unlimited and regular wave modes, each with a `TooltipTrigger`
- [v0.2.3] **Unlimited Waves System**: new `UnlimitedWaveManager` with infinite scaling, boss waves, and shared settings inherited from `WaveManager`
- [v0.2.1] **Tooltip Expansion Update**: added tooltips to player upgrades and attack rewards; new attacks Ignition Flash & Lifeforce
- [v0.2.0] **Progression, Economy & Milestones Update**: level system, gold economy, milestone rewards, stats extension

### [v0.2.0] - Content Expansion Update (Released)
- [v0.1.12] **The Milestone Update**: milestone rewards with buffs and drawbacks
- [v0.1.11] **The Currency Update**: gold system, stealing stat, gold rerolls, skill node refunding
- [v0.1.10_2] **Levels Update Part 2**: xp rebalances and gain indicators
- [v0.1.9] **The Levels Update**: exp, levels, skill points, tier 2 skill nodes
- [v0.1.6] **The Titles Update**: game titles and subtitles, wave completion texts
- [v0.1.0] **The Skill Tree Update**: skill tree, points, nodes, pan/zoom

### [v0.1.0] - Skill Tree Update (Released)
- [v0.0.20] **The Knockback Update**: knockback system, resistance, knockback % stats
- [v0.0.16] **The Dash Update**: new dash stats (multiplier, cooldown, distance, stamina)
- [v0.0.1] **Core Systems**: Warp, Blaze, Reminiscence, Serenade, Cultist enemy, status effects
