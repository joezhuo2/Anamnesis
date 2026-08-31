## Roadmap

Summarized major feature updates, newest first. Upcoming work lives under **Pre [v0.4.0]**.

### Pre [v0.4.0]
- [v0.3.8] **Enemy variants & wave gating**: two new slime variants — Slime (Frost) with the drifting `Blizzard` field that applies Slow and the new `Freeze 2`, and Slime (Magma) with the `Eruption` burst that applies the new `Overheat` stack. Rewards and enemies both gained a `minWave`: seven rare-pool attacks unlock at wave 20, five Awakenings at wave 35, and the unlimited-mode enemy roster now unlocks Slime, Crab, Slime (Magma), Bat and Slime (Frost) at waves 0/5/10/15/20. Boss waves are clamped to a single enemy, spawn bursts can no longer overshoot the wave budget, anomaly rerolls stop charging for an empty menu, and enemy health bars resolve a dedicated `HealthBarCanvas` instead of the first canvas in the scene
- [v0.3.7] **Data reference & damage-packet fix**: `GAME.md` rebuilt as a full reference over the attack, upgrade and status-effect assets, and the six authoring bugs it turned up all fixed — Supernova's missing Weaken reference, `Starlit Reflexes`' unread `pctAmt`, Lifeforce's inert `specialMult`, the unreferenced `Supersonic Cooldown`, `Exodus C`'s type and scaling stat, and the misspelled `Ignotion Flash` assets. `EntityHealth` defers the hurt i-frame trigger until a damage packet finishes, so a multi-type projectile applies every instance instead of only the first; wave rewards can roll a skill point on non-milestone waves; plus a tuning pass over Cyclone Cleave, Aphelion, Blood Pact, Stellar Maelstrom, Warp, Feedback Loop, Dash Advance and Serenade
- [v0.3.5] **Content Expansion Update**: Luminaria with the Holy Bounty buff; the Cosmic Superimposition capstone that upgrades Cosmic Afterimage into a decoy that detonates on expiry, plus two movement-speed nodes (105 to 108 nodes); and a reworked `DamageCalculator` where `resPen` finally applies to type resistance, penetration and armor shred overflow into a bonus damage multiplier. Also a stability pass over the entity, projectile, skill-tree and wave systems: reference-counted `CanMove`/`CanAttack` gates, runtime-copied Awakenings, attacker-sourced `resPen`/`defShred`, pooled skill-tree lines, non-allocating overlap queries, and the status effect assets moved out of the scripts tree into `Assets/data/StatusEffect/`
- [v0.3.4] **Awakening trigger coverage**: the seven declared-but-never-raised `TriggerCondition` values are wired up — `OnTakeHit` (direct enemy hits only, excluding DoT ticks and health costs), `OnKill`, `OnDeath`, `OnStaminaRegen`, `OnManaRegen`, `OnLevelUp`, and `OnSpawnProjectile` via a new static `ProjectileSpawner.ProjectileSpawned` event that carries the notification across the assembly boundary. README documents all 22 conditions and the overload each dispatches to
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
