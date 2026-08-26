# Changelog

All *notable* changes to Anamnesis are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project *roughly* follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
- cosmic aftermiage not having an indicator
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