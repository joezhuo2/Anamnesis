# Planned Features 

## Pre [v1.0.0] Checklist — First Light
*Everything that has to be true before a stranger can play it.*

**Bigger Things**
- [ ] Audio (SFX + music buses + menu volume control)
- [ ] more enemy projectile telegraphs (jellyfish ball)
- [ ] Store page — description, screenshots, capsule art, controls
- [ ] Window icon + splash

**Smaller Things**
- [ ] Confirm every `CREDITS.md` asset license permits redistribution inside a compiled build, not just use
- [ ] In-game credits/attribution screen — some of those licenses want attribution in the build itself
- [ ] Resolution/window options (currently a fixed 1920x1080 with `resizableWindow: 0`)
- [ ] Clean-machine pass — fresh build, no `settings.json`, no Unity installed

## Pre [v1.1.0] Checklist — Starlight Remnants
*Enemies fight back with more than stats.*

**Content**
- [ ] Elite/Champion enemy/boss variants with unique modifiers (extra stats, new ai, splitting)
- [ ] contact damage
  - [ ] ram dash (dash upgrade, dashing into enemies deal damage based on `x` and sends you back, has more iframes)

**QoL & Polish**
- [ ] Full stats display menu (in settings panel)
- [ ] Status effect sort options (duration, num of stacks, etc.) - configurable in settings
- [ ] enemy status effect overlay on common enemies
- [ ] wave track - show upcoming bosses/special rewards/milestones
- [ ] Status Effect vfx
- [ ] sort attack cooldown ui by basic - skill - ult instead of whatever was obtained first
- [ ] map debris/decor
- [ ] Background Overlays - reward menu, home screen, settings menu, scroll menu, death menu
- [ ] queue skill point spending - different highlight color, can be undone
- [ ] skill node undo grace window (no undo cost until closing the tree)

## Pre [v1.2.0] Checklist — Threads of Fate
*Every wave stops looking the same; the settings/stats menus catch up.*

**Content**
- [ ] Passive stat synergies between different build types (e.g. "Increases attack by 18% of max health") (new type of perm reward)

**Systems**
- [ ] Skill Points (? name) update: agi/def/str/dex/int/vit
- [ ] Permenant version of Anamolies (active until run ends) or one thats active for `X` waves, can also occur randomly at the start of every wave
- [ ] Techniques - utility/QoL featured (blink tp, buff, crowd control)
- [ ] attack combo chains
- [ ] in-world spawner boxes - spawns a small group of enemies for some rewardd (that can be picked up)

**Content**
- [ ] wave events - random events that can randomly occur during waves
- [ ] contracts - similar to anomaly, but no fail risk, easier objective, less bonus rewards
- [ ] Nightmare/Death difficulty (new enemy ai (eg. spliting), new attacks (attackData `minDifficulty` field))

## Pre [v1.3.0] Checklist — Domains of the Unbound
*New ways to deal damage, and somewhere interesting to deal it.*

**Systems**
- [ ] Player summons
- [ ] Player new "signature" that charges via a new special resource instead of a cooldown
- [ ] deployables (eg. totems/auras)
- [ ] Achievement system with unlock notifications
- [ ] Leaderboards (local/online) for boss rush/endless/highest dps

**Content**
- [ ] multiple map sections
- [ ] Environmental hazards on maps (spikes, lava, traps)
- [ ] portals
- [ ] starting builds / starting kits
- [ ] skill tree node search bar (by name, stat, etc.)

**QoL & Polish**
- [ ] Screen-edge indicators for off-screen enemies, boss cursor

## Pre [v1.4.0] Checklist — Starforged Echoes
*The item layer itself: equip, consume, buy.*

**Systems**
- [ ] Finish Gear/Item system (slots, rarity tiers, stat rolls, equip/unequip)
- [ ] Consumables (potions, bombs, temporary buffs) with hotkeys
- [ ] Shop/merchant between waves to spend currency on items or stat boosts

**Content**
- [ ] Elite "aura" variants that buff nearby enemies (e.g. attack speed, damage reduction) — encourages target prioritization
- [ ] "Memory" collectibles scattered in waves that unlock lore snippets and permanent bonuses
- [ ] Chests or loot drops from elites/bosses with guaranteed rare rewards

**QoL & Polish**
- [ ] Suggest certain stats based on player's current loadout
- [ ] Post-Death Summary (run grade)
- [ ] Build export/share — copy current loadout as text for sharing
- [ ] damage breakdown (by attack, every x waves)
- [ ] FPS counter & performance stats debug toggle

## Pre [v1.5.0] Checklist — Prismatic Recollection
*Damage gets a type, and defenses get a matching axis.*

**Systems**
- [ ] Elemental Damage/Defense system
- [ ] Elemental affinities/weaknesses
- [ ] attack mastery (use more to level up)

**Content**
- [ ] choose next wave style
- [ ] restrictions on run start - choose from a pool for bonus rewards

**QoL & Polish**
- [ ] Minimap
- [ ] Build Guide menu

## Pre [v1.6.0] Checklist — Starlight Ascension
*Both systems stop being standalone: crafted, combined, and replayed.*

**Systems**
- [ ] Crafting/enchanting system for gear
- [ ] Set bonuses for equipping matching gear pieces
- [ ] Elemental reactions
- [ ] Combo/synergy bonuses for stacking related rewards
- [ ] second skill tree (Prestiage/Ascension/Mastery)

**Content**
- [ ] Daily/weekly challenge modifiers with seeded runs
- [ ] run archive

## Planned

### Will do sometime
- [ ] "What's new" changelog popup on update
- [ ] reward history
- [ ] Customizable HUD layout
- [ ] data saving - full game runs
- [ ] Confirmation dialog before corrupting a reward (can be toggled in settings)
- [ ] beacon objective (defend/destroy)
- [ ] Kill Streak (combo counter, `PlayerUpgrade` condition)

### Will Consider
- [ ] target dummy OR dps counter
- [ ] Keyboard/controller navigation for reward & skill tree menus (no mouse required)
- [ ] Scrollable Tooltips
- [ ] Screenshot mode that hides the HUD
- [ ] Accessibility options (colorblind mode, reduced screen shake, larger text)
- [ ] attack cooldown over indicator
- [ ] build/loadout slots
- [ ] neutral entities
- [ ] cosmetics (player skins/dash effects/attack effects)
- [ ] nameplates/titles
- [ ] background/ambience (debris/wind)

### Planned Abilities 
- **Exploit** - *something* applies *something else* to the target, increasing status effect damage taken by `{x}%` for each status effect are on the target

- **Kinetic Theory** - knocking enemies into other enemies causes them to take contact damage scaling off of kbPct (after contact damage update)
- **Midas Touch** - *something* consumes `{x}` gold on every attack to increase its damage dealt by `{y}%`
- **Phoenix Flare** - allows one rebirth every `{x}` waves, and creates a massive explosion on trigger

## Open Items
- Enemy pooling is deliberately not done. Enemies are still `Instantiate`d per spawn (plus per split death)
  and `Destroy`ed on death, along with the per-spawn `EntityStats` clone (the `AttackData` clone chain is
  gone as of v0.4.10 — attacks are shared assets now). Three things block a straight swap to `PrefabPool`: cleanup is `Destroy`-bound across eight
  components (`EntityStatManager`, `EntityHealth`, `EnemyAttackHandler`, `EnemyMovement`,
  `StatusEffectManager`, `EnemyPhase`, `EntitySummonHandler`, `EntityProjectileHandler`) with no `OnDisable`
  counterparts; `EnemyStatManager.ScaleBaseStats` is non-idempotent, so level scaling compounds on a reused
  stat clone; and `WaveManager.CleanEnemyList` counts kills purely by Unity fake-null
  (`RemoveAll(e => e == null)`), which a deactivated enemy never satisfies, so the wave-completion gate would
  never close. Also latched with no reset: `EntityHealth.barRetired`, the animator `isDead` bool,
  `EnemyPhase.phase`, `EnemyMovement.cScale`, and `EnemyAttackHandler.cooldowns`.
- `SkillTreePanZoom` still polls `Mouse.current` / `Keyboard.current` directly and hard-codes Alt plus the mouse buttons, so skill tree pan and zoom cannot be rebound. Those controls are mouse-driven anyway
- `GameRestart` reloads the scene rather than tearing a run down, so anything held in a static that is not reset on scene unload survives the restart. `Projectile` and `MenuPause` are handled above; other statics have not been audited

### Available Colors 
- **red**
- **red-pink**
- purple-blue
- green-yellow
- brown

### Planned Capstone Nodes
- Hex Cast (+buff -cost)
- Starlit Reflexes (+buff -mana gain)

- Shattered Singularity
- Stellar maelstrom
- Solar Collapse
- Exodus

- Meteor Shower - long cast time, small hitbox, large aoe, high mana cost
- Starfury
- Autopilot
- Feedback Loop

### Stats without skill tree nodes
- add spl dmg pct
- add dmg pct
- basic dmg pct
- skill dmg pct
- ult dmg pct
- kb pct
- se pot pct
- basic cd red pct
- skill cd red pct
- ult cd red pct
- max stamina
- max mana
- dash spd mult
- dash stamina cost red pct
- exp bonus
- stealing

### next non-basic skill tree nodes
- crit chance
- crit damage
- atk spd pct
- aoe pct
- def shred
- res pen
- healing pct
- damage res 
- move spd pct

## Performance Improvements

### Medium

- [ ] Enemy pooling — documented as a deliberate deferral in Open Items; revisit when the three blockers
  (Destroy-bound cleanup with no `OnDisable` counterparts, non-idempotent `ScaleBaseStats`, fake-null kill
  counting) are resolved. Churn source: `EnemySpawner.cs:14-24`, `EntitySplitting.cs:21`,
  `EntitySummonHandler.cs:50`, per-spawn `EntityStats` clone (`EntityStatManager.cs:27`).

### Low

- [ ] `EnemyMovement.cs:50-63,71-109` — per frame per enemy: `Vector2.Distance` (sqrt) for de-aggro, plus
  `normalized` (second sqrt) on movement; `Start` (line 46) runs `FindGameObjectWithTag("Player")` per
  spawn. Fix: compare `distSqr` against squared range; axis pick via `Mathf.Abs`; cache player reference.
- [ ] `PlayerMovement.cs:72` — `animator.SetFloat(SpeedHash, ...)` every FixedUpdate even when unchanged
  (dirty animator param 50×/sec). Fix: change-guard like `EnemyMovement.SetAnimator` already does.
- [ ] `UnlimitedWaveManager.cs:233` — `List<GameObject> available = new()` per spawn (~1-3/sec in waves).
  Fix: reuse a field buffer or reservoir-sample without a list.
- [ ] Per-frame UI polling: `PlayerAttackCooldownUI.cs:88-120` re-runs `CanCast` (≈10 stat reads, 4 buttons
  ≈ 40/frame) and `StatusEffectCooldownUI.cs:45-68` polls GetStat + recomputes fill per effect icon.
  Fix: throttle to ~10 Hz or drive from cooldown-change events.
- [ ] `TextIndicator.cs:54-68` — `WorldToScreenPoint` per indicator per frame; 100+ concurrent numbers = 100
  screen-space transforms/frame. Fix: update screen pos only when world pos moved > ~1px.
- [ ] `WaveManager.cs:228-241` — `$"Time Remaining: {tt.timeRemaining:F1}s"` string + TMP setter per frame
  during Time Trial. Fix: rebuild when `FloorToInt(t * 10)` changes or description reference changes.
