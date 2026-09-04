<div align="center">

# Anamnesis

**A 2D wave-based action roguelite built in Unity 6**

*Anamnesis* — the recollection of memories. Survive endless waves of enemies, collect rewards, and rebuild your power through a branching skill tree. Choose between **Regular** waves or an **Unlimited** endless mode at the start of each run.

</div>

| [📖 About](./README.md) | [📜 Changelog](./CHANGELOG.md) | [🗺️ Roadmap](./ROADMAP.md) | [📝 Upcoming](./TODO.md) | [👏 Credits](./CREDITS.md) | [⚔️ Game Index](./GAME.md)
| :---: | :---: | :---: | :---: | :---: | :---: |

---

## About

Anamnesis is a top-down, wave-based action game currently in active development. Each wave throws escalating hordes of enemies at you while you weave together basic attacks, skills, and ultimates. Between waves you pick from randomized rewards — stat buffs, rare attacks, and powerful player upgrades — and spend skill points on a persistent skill tree.

At the start of each run you pick a **gamemode**: **Regular** waves follow the standard escalating sequence, while **Unlimited** waves scale infinitely with faster spawns, periodic boss waves, and endless rewards.

The game is built entirely with **ScriptableObject-driven data** (attacks, status effects, upgrades, skill tree nodes), so most content is data-authored and tuned in the Unity inspector. Under the hood, the combat, stat, resource, and UI systems are **decoupled onto small interfaces** (`IDamageable`, `IResourcePool`, `IStatusEffectReceiver`, `ITooltipDisplay`, and more) so systems compose without hard dependencies on concrete components.

## Core Loop

1. **Pick a difficulty and a gamemode** — cycle between **Easy**, **Normal** and **Hard** on the home screen (the tooltip lists exactly what each one shifts), then choose **Regular** waves (standard escalating sequence) or **Unlimited** waves (infinite scaling, faster spawns, periodic boss waves, endless rewards). Easy opens a free pre-run pick from the rare and treasure pools before wave 1. **Ironman Mode** can be toggled on alongside the difficulty for a run with no rerolls, no corruption and no skill node refunds.
2. **Survive the waves** — enemies spawn in escalating sequences with occasional boss encounters. Enemies can even **split** into more enemies on death, and extra spawns occur every 10 waves. Enemies scale exponentially.
3. **Choose a reward** — pick from buffs, special attacks, or treasure-pool Awakenings, using limited rerolls effectively. Some rare attacks and Awakenings are gated behind a minimum wave, so late-game options only start appearing once the run is deep enough. Some are even brave enough to "corrupt" the rewards, risking it all for a greater reward. Reroll with **gold** (200g) when out of rerolls.
4. **Face anomalies** — random world modifiers (e.g. *Time Trial*, *No Damage*, *Stat Modifier*, and more) that add risk for greater reward.
5. **Spend skill points** — unlock nodes on the skill tree to permanently empower the run. Gain skill points from levelling up, occasionally on wave clears and every 5 waves. Refund nodes using gold (default 50g) — including the starting node, unless the run is Ironman.
6. **Level Up & Earn Gold** — collect experience from enemies to level up, gaining stat boosts and skill points along the way. Enemies drop XP (common enemies drop less, bosses drop more with 15% variance). **Enemies also drop gold (15% variance), increased by the Stealing stat.**
7. **Repeat** — waves get harder, and you get stronger.

## Features

- **Difficulty selector** — pick **Easy**, **Normal** or **Hard** on the home screen with the left/right arrows before starting a run; the choice persists to `settings.json` and locks in when a gamemode button is pressed. Each difficulty is a `DifficultyData` ScriptableObject of additive offsets — enemy level (flat and per wave), total and concurrent enemy counts, reward choices and quality, milestone choices, bonus reroll/skill-point chance, anomaly chance/counts/rewards, corruption odds and boost, reroll gold cost, starting rerolls and skill points, and pre-run free picks. The tooltip is generated from the asset, listing only the non-zero offsets, and Normal is an all-zero asset so it is exactly the untuned baseline. Easy hands out 3 pre-run free picks, +5 rerolls and +3 skill points; Hard adds +3 enemy levels (+0.25 per wave), 5 more total and 2 more concurrent enemies, a +50g reroll cost and worse corrupt odds.
- **Ironman Mode** — a home-screen toggle, sitting alongside the difficulty selector, that removes every take-back from the run: rerolls are forced to 0 (starting rerolls, anomaly rerolls and occasional wave rerolls all included) and the reroll button and its count are hidden, the corrupt button never appears, and skill node refunds are refused with *Disabled in Ironman Mode*. The tooltip reads the live state (`Ironman Mode [ON]` / `[OFF]`), the choice persists to `settings.json` as `ironmanMode`, and it locks in and hides the moment a gamemode button is pressed.
- **Gamemode selector** — choose between **Regular** and **Unlimited** waves at the start of each run via dedicated buttons (with tooltips). Player actions are enabled in the lobby.
- **Wave system** — scriptable wave sequences, escalating spawns, extra enemy spawns every 10 waves, single-enemy boss waves, boss bars, and reward/anomaly button panels that update dynamically. A live **wave progress indicator** (`Wave 7 (12/30)`) tracks kills against the wave total, and clearing a wave announces the rerolls and skill points it granted in a single subtitle.
- **Unlimited waves** — an endless mode that scales infinitely: enemy level and max total enemies rise each wave, spawns speed up, boss waves appear periodically, and rewards never stop. Reuses the shared `WaveManager` settings (reroll cost, rewards, corruption, milestones, anomalies) with no reconfiguration; only the corruption odds are tuned separately.
- **Enemy splitting** — enemies can split into more enemies on death with configurable split count, health scaling, and behavior settings.
- **Enemy phases** — bosses (and any configured enemy) transition through phases as their HP drops below thresholds (e.g. 70% / 40%), granting phase stat buffs and unlocking stronger phase-gated attacks. The Lich speeds up at 40% HP (+40% move speed, +20% attack speed).
- **Global enemy spawner** — centralized spawning system for consistent enemy management.
- **Anomaly system** — randomized run modifiers with configurable frequency, counts, and reward bonuses (e.g., *Time Trial*, *No Damage*, *Stat Modifier*).
- **Data-driven attacks** — `AttackData` ScriptableObjects with projectile patterns (circle, spread, barrage, spread barrage, and the screen-wide converging lines: top-down, left-right, diagonal, diagonal reverse, full X), resource costs (stamina / mana / health), on-hit resource gains, summoning, boomerang travel patterns, **wave**, **spiral** and **follow-cursor** flight paths, orbit interactions (fire, absorb, redirect, explode), **follow-source** option for projectiles, `cleanseDebuffs` to strip N debuffs off the caster the moment the attack is performed, and on-hit **additional attacks** that chain into multi-stage combos. Every projectile in such a chain carries a `chainRoot` back to the attack that started it, so upgrades can react to the chain ending.
- **Cast time** — attacks can carry an interruptible windup (`castTime`) before they resolve, with `canMoveWhileCasting` deciding whether the caster is rooted for it. A pooled cast bar and remaining-time countdown follow the caster while it channels. The cooldown is spent when the cast starts, resource costs only when it lands, so an interrupt costs nothing but the cooldown. Death, stuns and enemy projectile hits all interrupt; the `interruptResist` stat tiers that off (1 ignores projectile hits, 2 also ignores stuns), and `castTimeRedPct` shortens casts.
- **Charged attacks** — attacks flagged `canCharge` can be held down to sustain them. Holding past `chargeThreshold` (0.225s) skips the tap entirely and commits to the charge, spawning the separate `chargeAttack` and paying its cost again every `chargeTickInterval`; releasing sooner resolves as a plain tap at the base attack's cost. Each drain tick refreshes the sustained projectiles' lifetime and re-snapshots their damage, so buffs earned mid-hold carry forward. `minChargeTime` queues an early release, `maxChargeTime` forces one, and `cooldownOnAttackStart` decides whether the cooldown ticks down during the hold. Charges reuse the cast bar and every cast interrupt rule. Enemies skip the threshold, commit instantly, and roll to keep holding on each tick.
- **Status effects** — stackable DoTs, stuns, stat buffs/reductions, attack replacement, and more, with cooldown UI.
- **Awakenings** — trigger-condition-based `PlayerUpgrade` ScriptableObjects with chance/cooldown/delay, driven by 22 trigger conditions (on attack, on crit, on hit, on dash, on kill, on level up, on projectile spawn, …). Awakenings can also be **passive**, wiring themselves up in `OnUnlock` and tearing themselves down in `OnRemove` instead of listening for a trigger. See [Awakening trigger conditions](#awakening-trigger-conditions).
- **Skill tree** — interactive pan/zoom tree with a **bidirectional connections system** (OR logic), incompatible nodes, tooltips, connector lines, and a skill-point currency. Nodes connect via the `prerequisites` field; unlocking works both ways (A→B means unlock A if B unlocked OR unlock B if A unlocked) and only **one** connected node needs to be unlocked. Left-click unlocked nodes to refund using **gold** (default 50g, configurable per node) — the starting node included, which clears the starting choice so another can be picked. Refunds are disabled entirely in Ironman Mode. **Capstone nodes** gate behind owning a specific attack or Awakening and upgrade it in place — a required Awakening is removed as the upgraded one is granted, and comes back if the node is refunded. 137 nodes across the health, regen, intelligence, area-of-effect, damage, movement and capstone branches.
- **Corruption system** — once per wave (never in Ironman Mode), corrupt rewards for a chance at massive stat boosts or severe penalties. In Regular waves a corrupt rolls at 40% with a 40% chance of landing positive and a boost capped at +80%; Unlimited waves push that to 45% / 30% / +135%, trading a worse coin flip for a bigger payout. Each corrupted button also has a 4% chance to be replaced outright by a **corruption special** — a rare attack pulled from a dedicated pool at a far lower unlock wave than the rare pool asks for, marked violet and labelled *Corrupted*.
- **Wave-gated content** — enemies, rare attacks, and Awakenings each carry a `minWave` and only enter their pools once the run reaches it, so early waves draw from a smaller, gentler set.
- **Milestone rewards** — every 25 waves (25, 50, 75, 100...), choose from 3 synergistic reward bundles that combine powerful buffs with meaningful drawbacks (e.g., *Glass Cannon*: +40% Damage / -40% Max Health). Each stat has ±15% variance for replayability.
- **Title system** — game title/subtitle with fade in/out, plus wave-complete and boss-killed title displays. The between-wave subtitle (and the 1.5s pause it holds) can be switched off per wave manager via the *wave completion message* setting.
- **Settings menu** — press `Escape` to open a pause-and-configure panel. It runs two pages: the outer pause page, and a controls page holding toggles for enemy health bars, XP drops, gold drops, damage numbers and the wave completion message, plus a rebindable list of every keyboard binding. `Escape` backs out one page at a time. Everything applies live and persists to `settings.json` in the platform's persistent data path. Rebinding runs the Input System's interactive rebind, restricted to keyboard controls, cancelable with `Escape`, and rejects a key that is already bound to something else.
- **Blocked-attack feedback** — each attack's cooldown indicator carries a border that turns red the moment the attack cannot be cast — dead, `CanAttack` off, still on cooldown, or unaffordable — and returns to its normal color when it can. Pressing a blocked attack flashes that border on unscaled time, so the refusal reads even while paused. Affordability is checked through `PlayerAttackHandler.CanAfford`, which prices the attack the same way spending does (`GetCosts` plus `HandleHexCast`) without taking anything.
- **Hover feedback** — reward, anomaly, cooldown-indicator and skill-node buttons scale up under the pointer via `HoverScale`, easing on unscaled time so it keeps animating while the game is paused.
- **Resources** — health, stamina, and mana with dash, knockback, and cooldown systems.
- **Overhealth** — a pool of health held *above* `EffMaxHp`. It is spent before `currentHp` when damage lands, is never clamped by max health, and is cleared on death. Absorbed damage still runs the hurt animation, i-frames, the damage number and the `OnTakeHit` triggers, so a fully-absorbed hit hands out no free invulnerability. Backed by `StatType.overhealth`, so buffs, status effects and gear can move the pool directly; `EntityHealth` owns the gain/decay rules.
- **Knockback** — full knockback for players and enemies, with knockback resistance and increased knockback stats.
- **Damage indicators** — floating damage numbers with small randomness. **XP gain indicators** and **XP wrapper option** for custom XP display. **Gold gain indicators** (+{gold}g in gold color).
- **Levelling system** — enemies drop XP, players collect XP to level up and gain stat buffs (+5 HP, +2 ATK, +2 INT, +0.008 SPD) and a skill point per level. **Level-up indicator** on progression.
- **Gold system** — enemies drop gold on death (15% variance, same as XP). **Stealing stat** increases gold drops by {stealing}%. Spend gold to reroll rewards (200g when out of rerolls) or refund skill nodes.

## Controls

| Action | Binding |
| --- | --- |
| Move | `WASD` / Arrow keys |
| Basic attack | `Left Click` / `Space` |
| Charge an attack | Hold its binding (chargeable attacks only) |
| Skill | `E` / `1` |
| Ultimate | `R` / `2` |
| Dash | `Q` / `Right Click` |
| Toggle skill tree | `K` |
| Settings menu (pause) | `Escape` |
| Skill tree pan | Drag (Alt+Left / Alt+Right / Middle) |
| Skill tree zoom | Mouse wheel (zoom-to-cursor) |

Every keyboard binding above except the skill tree pan/zoom can be rebound in the settings menu; `Escape` itself is fixed.

## Content

**Player attacks** — 
- **Basic Attacks**: Blaze, Lacerate, Aphelion, Astral Nova, Blood Pact, Ignition Flash
- **Skill**: Warp, Cyclone Cleave, Meteor Shower, Nebula, Stellar Maelstrom, Supernova, Lifeforce
- **Ultimate**: Nirvana, Revelation, Shattered Singularity, Solar Collapse, Starfury, Exodus, Luminaria, Nocturnis
- **Awakenings**: Reminiscence, Serenade, Feedback Loop, Soul Rend, Supersonic, Hex Cast, Stellar Surge, Starlit Reflexes, Paradox, Cosmic Afterimage, Hypercarry, Autopilot, Exsanguinate, Terminal Cascade, Cresendo, Tempo, and the capstone-only Solar Wind

**Enemies** — 
- **Regular Enemies**: Bat, Crab, Slime, Slime (Frost), and Slime (Magma)
- **Bosses**: Cultist (clone summoning), Jellyfish, Lich, Golem (phase-gated moveset)

Each enemies have their own stats, attack sets, movement patterns, behavior, and inflict unique status effects. Some even have unique behaviour such as summons, phase escalation, and more to come! In Unlimited waves the roster unlocks as the run goes: Slime from wave 0, Crab from 5, Slime (Magma) from 10, Bat from 15, and Slime (Frost) from 20.

**Status effects** — DoTs, Stun, Stat Buffs, Stat Reductions (Slow, Weaken, etc.), Attack Enhancements,and more.

**Player upgrade effects** — the `PlayerUpgrade` subclasses the Awakening assets above are authored against: Add Chain, Additional Damage, Cooldown Advance, Decoy, Gain Mana, Grant Status Effect, Hex Cast, Overhealth, Paradox, Reminiscence, Soul Rend, Spawn Projectile, Stellar Surge.

### Awakening trigger conditions

Every `PlayerUpgrade` asset lists one or more `TriggerCondition` values, plus a chance, cooldown and
delay. `PlayerUpgradeManager` rolls the chance and checks the cooldown once per condition match, then
calls one of the `TriggerUpgradeEffect` overloads — so an upgrade only responds to a condition if it
overrides the overload that condition dispatches to.

| Condition | Fires when | Overload |
| --- | --- | --- |
| `OnAttack` | Any attack is performed | `(player)` |
| `OnBasicAttack` | A Basic attack is performed | `(player)` |
| `OnSkillAttack` | A Skill attack is performed | `(player)` |
| `OnUltAttack` | An Ultimate attack is performed | `(player)` |
| `OnCalculateAttackCost` | An attack's resource costs are about to be paid | `(player)` |
| `OnSpawnProjectile` | A projectile the player owns is spawned, once per projectile | `(player, spawnCenter)` |
| `OnProjectileHit` | A projectile the player owns hits a target | `(player, hitPosition)` |
| `OnDealDamage` | A damage instance the player owns lands | `(player, target, damageDealt)` |
| `OnTargetRecievedHit` | A target takes damage from the player | `(player)` |
| `OnCrit` | A critical damage instance the player owns lands | `(player)` |
| `OnOverkill` | The player's killing blow is at least 3× the target's remaining HP | `(player)` |
| `OnKill` | An entity dies to damage the player dealt | `(player)` |
| `OnTakeDamage` | The player takes any damage — direct hits, DoT ticks and health costs alike | `(player)` |
| `OnTakeHit` | The player is hit directly by an enemy. Excludes DoT ticks, health costs (`Consume`), heals, and any packet that bypasses i-frames | `(player)` |
| `OnCounterDodge` | The player is hit while immune and dashing | `(player)` |
| `OnStartDash` | A dash begins | `(player)` |
| `OnEndDash` | A dash ends | `(player)` |
| `OnHealthRegen` | Passive health regen ticks for at least 1 HP | `(player)` |
| `OnStaminaRegen` | Passive stamina regen ticks for at least 1 stamina | `(player)` |
| `OnManaRegen` | The player actually gains mana. There is no passive mana regen tick, so this covers every mana gain | `(player)` |
| `OnLevelUp` | The player gains a level. A single XP pickup that crosses several thresholds fires once per level | `(player)` |
| `OnDeath` | The player dies, before the death sequence tears the object down. An upgrade whose `delay` outlasts the death animation is cut off | `(player)` |

Two conditions are reentrancy-guarded so an upgrade cannot feed itself in a loop: `OnSpawnProjectile`
(an upgrade that spawns projectiles) and `OnManaRegen` (an upgrade that grants mana). The guard only
covers the immediate call, so an upgrade with a non-zero `delay` that re-triggers its own condition
still needs a cooldown to stay bounded.

Note that a non-zero `delay` on a `(player, spawnCenter)` condition drops the position: the delayed
path calls the plain `(player)` overload. Position-sensitive upgrades on `OnSpawnProjectile` and
`OnProjectileHit` should leave `delay` at 0.

An Awakening does not have to use a trigger at all. `OnUnlock(player)` runs when the upgrade is
granted and `OnRemove(player)` when it is taken away (a capstone node swapping it out, or a refund),
so a purely passive upgrade installs its effect in the first and reverses it in the second, leaving
`conditions` empty. Exsanguinate (health-regen conversion on `EntityHealth`) and Terminal Cascade
(the projectile chain-retrigger chance) both work this way.

## Tech Stack

- **Engine:** Unity `6000.4.6f1` (Unity 6)
- **Rendering:** Universal Render Pipeline (2D)
- **Input:** New Input System (`PlayerControls.inputactions`)
- **UI:** uGUI + TextMeshPro

## Project Structure

```
Assets/
├── New.unity                  # Main game scene (WaveManager, SkillTree, Player UI)
├── data/                      # ScriptableObject data (attacks, entities, waves, skill tree)
│   ├── _example/              # Template attack folder (AD/PD/controller/prefab) to copy when authoring
│   ├── entity/                # Enemy/Player base stats, attacks, animation data, assets, and prefabs
│   │   └── enemy/             # Split into Bosses/ and Enemies/
│   ├── images/                # Images assets
│   ├── PlayerData/            # Player attacks, upgrades, skill tree data, controls
│   ├── prefabs/               # UI element prefabs
│   ├── StatusEffect/          # Authored status effect assets (DoTs, stuns, buffs, debuffs)
│   └── Wave/                  # Wave sequences (waves/) and difficulty assets (difficulties/)
└── scripts/
    ├── Entity/                # [asmdef] Player, Enemy, stats, health, levelling, summoning, XP
    │   ├── Enemy/             # Enemy AI, movement, attack handlers, spawner, stats
    │   └── Player/            # Player movement, attack, resources, UI, upgrades, level
    ├── Items/                 # Items/Gear system (Assembly-CSharp)
    ├── Misc/                  # Game Controller (implements IAnnouncer), settings menu UI (Assembly-CSharp)
    ├── Pooling/               # [asmdef] PrefabPool + IPoolable — shared prefab-keyed object pool
    ├── Projectile/            # [asmdef] Projectiles/Attack data and the damage calculator
    ├── Settings/              # [asmdef] GameSettings — persisted gameplay toggles and keybind overrides
    ├── StatusEffect/          # [asmdef] Status effect system & implementations (DoTs, Stun, Pulled, buffs) — data lives in data/StatusEffect/
    ├── SkillTree/             # [asmdef] Skill tree manager (implements ISkillPointHolder), UI, pan/zoom, bidirectional connections
    ├── TextIndicator/         # [asmdef] Floating damage numbers, XP/Gold indicators
    └── Wave/                  # [asmdef] WaveManager, UnlimitedWaveManager, rewards, anomalies, difficulty and Ironman selectors
```

`Core` is no longer in this repo. It lives in
[joezhuo2/CrystalFlux-Core](https://github.com/joezhuo2/CrystalFlux-Core) and Unity
imports it automatically from the git URL in `Packages/manifest.json`:

```json
"com.crystalflux.core": "https://github.com/joezhuo2/CrystalFlux-Core.git"
```

It holds contracts only — interfaces (`IStatProvider`, `IDamageable`, `IResourcePool`,
`IStatusEffectReceiver`, `IAttackHandler`, `IUpgradeHolder`, `ISkillPointHolder`,
`IBossBar`, `ITooltipDisplay`, `IUnlockEffect`, `IAnnouncer`, …), asset bases
(`AttackAsset`, `UpgradeAsset`, `EffectAsset`), shared value types (`DamagePacket`,
`StatType`, `StatBuff`, `InputState`, `DamageRoll`), and the `EnemySpawning` /
`PlayerEvents` hooks. Everything in it sits in the `CrystalFlux.Core` namespace.

### Assembly Boundaries

Each `[asmdef]` folder compiles to its own assembly, so cross-system dependencies are enforced by the compiler rather than by convention:

```
                  ┌─ Projectile
                  ├─ StatusEffect
Core (package) ───┼─ SkillTree
                  ├─ Wave
                  └─ Entity ──→ Projectile, StatusEffect, SkillTree, TextIndicator

                  ┌─ Projectile
                  ├─ StatusEffect
Pooling ──────────┼─ Wave
                  ├─ TextIndicator
                  └─ Entity

TextIndicator ── (references Pooling and TextMeshPro only)

Settings ─────────┬─ Entity
                  └─ Wave
```

- **`Core` references nothing.** It holds only contracts — interfaces, abstract `ScriptableObject` bases, and shared value types — and ships as its own package.
- **`Pooling` references nothing either.** It holds `PrefabPool` and the `IPoolable` reset hook, needs only `UnityEngine`, and is referenced by every assembly that spawns something. It is a separate assembly rather than part of `Core` because `Core` ships as an external package, and because `TextIndicator` referenced neither.
- **`Settings` references nothing either.** It holds `GameSettings` and its lifecycle bootstrap, and is referenced by the two assemblies that read a setting — `Entity` (health bars, XP and gold drops) and `Wave` (the completion message). The menu UI itself lives in `Misc`.
- **`Projectile`, `StatusEffect`, `SkillTree`, and `Wave` never reference each other** — only `Core`, plus `Pooling` where they spawn something (`SkillTree` does not). Adding a cross-reference between them is a compile error, which is the point.
- **`Entity`** is the only assembly that composes the leaf systems — everything else references infrastructure (`Core`, `Pooling`) and nothing else.
- **`TextIndicator`** references only `Pooling` and TextMeshPro — floating numbers need the object pool and nothing else.
- `Items` and `Misc` remain in `Assembly-CSharp`, which auto-references every assembly above.

Types shared across a boundary live in `Core` as an abstract base (`AttackAsset`, `UpgradeAsset`, `EffectAsset`) rather than an interface, because Unity cannot serialize interface-typed asset fields. Concrete data (`AttackData`, `PlayerUpgrade`, `StatusEffect`) stays in its own assembly.

`Wave` orchestrates the run but never names a concrete system type. Where it used to
reach for `PlayerAttackHandler`, `PlayerUpgradeManager`, `PlayerSkillTree`,
`StatusEffectManager`, or `BossBarUI`, it now talks to the `Core` interface each of
those already implements. Two static entry points it depended on became `Core` hooks
the owning system registers or raises — `EnemySpawning` (registered by `EnemySpawner`
at load) and `PlayerEvents.OnPlayerTakeDamage` (raised by `EntityHealth`).

Reward tooltips work the same way: rather than `RewardButton` reading two dozen fields
off `AttackData` and `PlayerUpgrade`, `AttackAsset` and `UpgradeAsset` declare an
abstract `GetTooltipLines`, and each system describes its own data.

> **Moving a `[SerializeReference]` type between assemblies breaks existing assets.** Unity stores a literal `{class, ns, asm}` triplet, so add `[MovedFrom(sourceAssembly: "...")]` when relocating one — see `UnlockEffect` and `NodeRequirement`.

## Getting Started

1. Open the project in **Unity 6000.4.6f1** (or newer).
2. Open the main scene: `Assets/New.unity`.
3. Press **Play** or **File > Build and Run (`Ctrl + B`)**

> **Note:** The build settings must include `Assets/New.unity` — `Assets/data/Scenes/SampleScene.unity` is an empty placeholder scene.

## Roadmap

See [ROADMAP.md](ROADMAP.md) for summary of major feature updates.

## Planned Features

See [TODO.md](TODO.md) for upcoming content and rebalances.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for the full release history.

## License

All rights reserved. This project is a personal work-in-progress and is not licensed for redistribution.
