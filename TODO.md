# Planned Features 

[?] Questionable - indicates questionable behavior (may not be true), only occured one time, or may be unlucky

## Open Items
- Enemy pooling is deliberately not done. Enemies are still `Instantiate`d per spawn (plus per split death)
  and `Destroy`ed on death, along with the whole per-spawn `AttackData`/`EntityStats` ScriptableObject clone
  chain. Three things block a straight swap to `PrefabPool`: cleanup is `Destroy`-bound across eight
  components (`EntityStatManager`, `EntityHealth`, `EnemyAttackHandler`, `EnemyMovement`,
  `StatusEffectManager`, `EnemyPhase`, `EntitySummonHandler`, `EntityProjectileHandler`) with no `OnDisable`
  counterparts; `EnemyStatManager.ScaleBaseStats` is non-idempotent, so level scaling compounds on a reused
  stat clone; and `WaveManager.CleanEnemyList` counts kills purely by Unity fake-null
  (`RemoveAll(e => e == null)`), which a deactivated enemy never satisfies, so the wave-completion gate would
  never close. Also latched with no reset: `EntityHealth.barRetired`, the animator `isDead` bool,
  `EnemyPhase.phase`, `EnemyMovement.cScale`, and `EnemyAttackHandler.cooldowns`.
- The on-screen attack button (`PlayerAttackHandler.CreateButtonUI`) fires `onClick` only and has no release
  event, so a chargeable attack triggered from the UI holds until `maxChargeTime`. Needs `IPointerUpHandler`.
- `SkillTreePanZoom` still polls `Mouse.current` / `Keyboard.current` directly and hard-codes Alt plus the mouse buttons, so skill tree pan and zoom cannot be rebound. Those controls are mouse-driven anyway
- `GameRestart` reloads the scene rather than tearing a run down, so anything held in a static that is not reset on scene unload survives the restart. `Projectile` and `MenuPause` are handled above; other statics have not been audited
- `DeathScreenUI` pushes `MenuPause` and never pops it. Restart is the only exit today and `GameRestart.ToHomeScreen` resets the depth, but any future dismiss or return-to-menu path has to pop or reset it itself

## Pre [v0.5.0] Checklist — Feel & Foundations
*Make the current game feel finished before stacking more systems on it.*

**QoL & Polish**
- [ ] finish tilemap
  - [ ] map borders (tilemap colliders)
- [ ] Background Overlays - reward menu, home screen, settings menu, scroll menu, death menu
- [ ] add code of conduct, license, [*] security policy, issue template, [*] pull req template

## Pre [v0.6.0] Checklist — Combat Depth
*Attacks chain, enemies fight back with more than stats.*

**Systems**
- [ ] attack combo chains
- [ ] Audio (SFX + music buses + menu volume control)

**Content**
- [ ] Elite/Champion enemy/boss variants with unique modifiers (extra stats, new ai, splitting)
- [ ] more enemy move telegraphs
- [ ] contact damage
- [ ] ram dash (dash upgrade, dashing into enemies deal damage based on `x` and sends you back)
- [ ] environmental collectible items (mana, xp, hp, gold)

**QoL & Polish**
- [ ] Full stats display menu (in settings panel)
- [ ] Status effect sort options (duration, num of stacks, etc.) - configurable in settings
- [ ] enemy status effect overlay on common enemies
- [ ] sort attack cooldown ui by basic - skill - ult instead of whatever was obtained first
- [ ] Pause menu quit button
- [ ] wave track - show upcoming bosses/special rewards/milestones
- [ ] map debris/decor
- [ ] Screen shake and hit-stop feedback on attacks
- [ ] Status Effect vfx

## Pre [v0.7.0] Checklist — Run Variety
*Every wave stops looking the same; the settings/stats menus catch up.*

**Systems**
- [ ] Skill Points (? name) update: agi/def/str/dex/int/vit
- [ ] Permenant version of Anamolies (active until run ends) or one thats active for X waves
- [ ] Techniques - utility/QoL featured (blink tp, buff, crowd control)

**Content**
- [ ] wave events - random events that can randomly occur during waves
- [ ] contracts - similar to anomaly, but no fail risk, easier objective, but still some bonus rewards
- [ ] Nightmare/Death difficulty (new enemy ai (eg. spliting))

## Pre [v0.8.0] Checklist — Player Power & Maps
*New ways to deal damage, and somewhere interesting to deal it.*

**Systems**
- [ ] Player summons
- [ ] Player new "signature" that charges via a new special resource instead of a cooldown
- [ ] deployables (eg. totems/auras)

**Content**
- [ ] multiple map sections
- [ ] Environmental hazards on maps (spikes, lava, traps)
- [ ] portals
- [ ] starting builds / starting kits

**QoL & Polish**
- [ ] Screen-edge indicators for off-screen enemies, boss cursor

## Pre [v0.9.0] Checklist — Gear & Items
*The item layer itself: equip, consume, buy.*

**Systems**
- [ ] Finish Gear/Item system (slots, rarity tiers, stat rolls, equip/unequip flow)
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

## Pre [v0.10.0] Checklist — Elemental Core
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
- [ ] skill tree node search bar (by name, stat, etc.)

## Pre [v0.11.0] Checklist — Gear & Elemental Expansion
*Both systems stop being standalone: crafted, combined, and replayed.*

**Systems**
- [ ] Crafting/enchanting system for gear
- [ ] Set bonuses for equipping matching gear pieces
- [ ] Elemental reactions
- [ ] Passive stat synergies between different build types (e.g. armor scaling with hp) (player upgrades)
- [ ] Combo/synergy bonuses for stacking related rewards
- [ ] second skill tree (Prestiage/Ascension/Mastery)

**Content**
- [ ] Daily/weekly challenge modifiers with seeded runs
- [ ] run archive

**QoL & Polish**
- [ ] Achievement system with unlock notifications
- [ ] Leaderboards (local/online) for boss rush/endless/highest dps

## Planned - Unknown
- [ ] target dummy OR dps counter
- [ ] Auto-pause when window loses focus (single-player)
- [ ] Keyboard/controller navigation for reward & skill tree menus (no mouse required)
- [ ] Scrollable Tooltips
- [ ] "What's new" changelog popup on update
- [ ] reward history
- [ ] Customizable HUD layout
- [ ] Screenshot mode that hides the HUD
- [ ] queue skill point spending
- [ ] skill node undo grace window
- [ ] data saving - full game runs
- [ ] Confirmation dialog before corrupting a reward (can be toggled in settings)
- [ ] Accessibility options (colorblind mode, reduced screen shake, larger text)
- [ ] attack cooldown over indicator
- [ ] loadout slots

- [ ] neutral entities
- [ ] in-world spawners
- [ ] rift system (portal opens to add new enemies)
- [ ] beacon objective (defend/destroy)
- [ ] cosmetics (player skins/dash effects/attack effects)
- [ ] nameplates/titles
- [ ] background/ambience (debris/wind)
- [ ] Kill Streak (combo counter, `PlayerUpgrade` condition)

### Planned Abilities 
- **Exploit** - *something* applies *something else* to the target, increasing status effect damage taken by `{x}%` for each status effect are on the target
- **Kinetic Theory** - knocking enemies into other enemies causes them to take contact damage scaling off of kbPct (after contact damage update)
- **Midas Touch** - *something* consumes gold to increase all damage dealt by `{y}%`
- **Phoenix Flare** - allows one rebirth every `{x}` waves, and creates a massive explosion on trigger

### Available Colors 
- **red**
- **red-pink**
- purple-blue
- green-yellow
- brown

### Planned Capstone Nodes
- Hex Cast (+buff -cost)
- Starlit Reflexes (+buff -mana gain)
- Supersonic (+count +dmg +size)
- Shattered Singularity (+spd -cd +size)

- Astral Nova (-spawnDelay -cd +range)
- Stellar maelstrom (-cost +count +homing)
- Solar Collapse (+size -cd +dmg)
- Meteor Shower (+size +dmg)
- Starfury (+size +debuff -cost)
- Autopilot (+count +pierce +homing +dmg)
- Feedback Loop (+chance -cd +dmg)

### Stats without skill tree nodes
- spell dmg pct
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
- dash cd red pdt
- dash dist pct
- dash stamina cost red pct
- exp bonus
- stealing

- crit chance
- crit damage
- atk spd pct
- aoe pct
- def shred
- res pen
- healing pct
- damage res 
- move spd pct