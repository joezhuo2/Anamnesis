## Roadmap

Summarized major feature updates, newest first. Upcoming work lives in the milestone checklists in `TODO.md`.

### [v0.5.0]

### [v0.4.0] - Object Pooling and Content Update
- [v0.4.1] **Skill tree expansion & balance pass**: 12 new nodes take the tree to 137 — an Attack III tier (+3 Attack, +2% `atkPct`) on both tails of the attack branch and a six-node movement-speed chain off `atk3b`; Solar Wind buffed to 8s, Cyclone Cleave reworked to physical + true damage, Blaze spaced; enemy HP regen scaling cut to a third, +2 armor per level
- [v0.4.0] **Object pooling**: one shared static `PrefabPool` (prefab-keyed, capped, with an opt-in `IPoolable` reset hook) now backs health bars, status effect icons, cast bars, damage numbers, reward/anomaly buttons and projectiles; `Projectile.Start` became `Setup`. Surfaced eight latent reuse bugs, including per-frame health-bar rebuilds and an unbounded damage-number pool. Enemies remain unpooled
- [v0.3.13_1] **Balance pass**: enemy base attack lowered across the roster, `corruptionSpecialChance` 8 to 4, unlimited-wave enemy cap 10 to 6, Blaze windup removed, Cyclone Cleave and Solar Wind buffed
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
