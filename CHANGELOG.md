# Changelog

All notable changes to Anamnesis are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [v0.1.6] - 2026-08-13 - The Titles Update

### Rebalance
# Exodus
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
# Enemy
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

## Removed
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