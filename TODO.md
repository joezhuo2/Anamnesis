# Planned Features 

## Open Items
- No affordability check on press. A player with no resources still plays the attack animation, and only
  ~0.225s later does nothing spawn. A real pre-check is awkward: `HandleStatChanges` both triggers cost
  upgrades and spends in one pass, and the public `GetCosts` skips `HandleHexCast`, so a rough check would
  wrongly block HexCast builds. Needs the method split properly if the dead animation matters.
- Orbit interactions and the `SummonCondition.OnCast` roll still fire on press, before the tap/hold split is
  known — so a hold consumes your orbits even though it skips the tap.
- The on-screen attack button (`PlayerAttackHandler.CreateButtonUI`) fires `onClick` only and has no release
  event, so a chargeable attack triggered from the UI holds until `maxChargeTime`. Needs `IPointerUpHandler`.

## Pre [v0.5.0] Checklist — Feel & Foundations
*Make the current game feel finished before stacking more systems on it.*

**Systems**
- [ ] Pause menu with resume/restart/home/quit, settings/controls menu
- [ ] Audio (SFX + music buses, ready for the volume sliders)
- [ ] Data saving (settings, unlocks, groundwork for run history)

**Content**
- [ ] re-add phase based buffs using the new system (Cultist and Jellyfish still pending)

**QoL & Polish**
- [ ] Screen shake and hit-stop feedback on attacks
- [ ] Title/header for reward menu
- [ ] map borders
- [ ] Auto-pause when window loses focus (single-player)
- [ ] Scrollable Tooltips
- [ ] target dummy
- [ ] "What's new" changelog popup on update

## Pre [v0.6.0] Checklist — Combat Depth
*Attacks chain, enemies fight back with more than stats.*

**Systems**
- [ ] attack combo chains
- [ ] input buffering (queue next attack early)

**Content**
- [ ] Elite/Champion enemy/boss variants with unique modifiers (extra hp, faster, new ai, split)
- [ ] Chests or loot drops from elites/bosses with guaranteed rare rewards
- [ ] Kill Streak (combo counter, `PlayerUpgrade` condition)
- [ ] backstap `specialMult`
- [ ] Wave 60 boss
- [ ] more enemy move telegraphs

**QoL & Polish**
- [ ] Full attack stats display
- [ ] Status effect sort options (duration, num of stacks, etc.) - configurable in settings
- [ ] enemy status effect overlay on common enemies

## Pre [v0.7.0] Checklist — Run Variety
*Every wave stops looking the same; the settings/stats menus catch up.*

**Systems**
- [ ] Skill Points (? name) update: agi/def/str/dex/int/vit
- [ ] Permenant version of Anamolies (active until run ends) or one thats active for X waves
- [ ] Difficulty selector
- [ ] certain upgrades have requirements before being added to the pool

**Content**
- [ ] wave events - random events that can randomly occur during waves
- [ ] contracts - similar to anomaly, but no fail risk, easier objective, but still some bonus rewards
- [ ] environmental collectible items (mana, xp, hp, gold)

**QoL & Polish**
- [ ] settings menu overhaul: volume control, enemy health bar toggle, damage number toggle/size change
- [ ] stats menu
- [ ] Confirmation dialog before corrupting a reward (can be toggled in settings)
- [ ] Accessibility options (colorblind mode, reduced screen shake, larger text)

## Pre [v0.8.0] Checklist — Player Power & Maps
*New ways to deal damage, and somewhere interesting to deal it.*

**Systems**
- [ ] Player summons
- [ ] Player new "signature" that charges via a new special resource instead of a cooldown
- [ ] deployables (eg. totems/auras)
- [ ] loadout slots

**Content**
- [ ] multiple map layouts
- [ ] Environmental hazards on maps (spikes, lava, traps)
- [ ] portals
- [ ] Anamolies update (change spawning behavior)
- [ ] starting builds / starting kits

**QoL & Polish**
- [ ] Screen-edge indicators for off-screen enemies, boss cursor
- [ ] Keyboard/controller navigation for reward & skill tree menus (no mouse required)

## Pre [v0.9.0] Checklist — Gear & Items
*The item layer itself: equip, consume, buy.*

**Systems**
- [ ] Finish Gear/Item system (slots, rarity tiers, stat rolls, equip/unequip flow)
- [ ] Consumables (potions, bombs, temporary buffs) with hotkeys
- [ ] Shop/merchant between waves to spend currency on items or stat boosts

**Content**
- [ ] Corrupt button (part 2) - can turn normal rewards into new type of special rewards (very rare)
- [ ] Elite "aura" variants that buff nearby enemies (e.g. attack speed, damage reduction) — encourages target prioritization
- [ ] "Memory" collectibles scattered in waves that unlock lore snippets and permanent bonuses

**QoL & Polish**
- [ ] Suggest certain stats based on player's current loadout
- [ ] Post-Death Summary
- [ ] Build export/share — copy current loadout as text for sharing
- [ ] damage breakdown (by attack, every x waves)
- [ ] FPS counter & performance stats toggle

## Pre [v0.10.0] Checklist — Elemental Core
*Damage gets a type, and defenses get a matching axis.*

**Systems**
- [ ] Elemental Damage/Defense system
- [ ] Elemental affinities/weaknesses
- [ ] attack mastery (use more to level up)

**Content**
- [ ] Alternate movement options
- [ ] choose next wave style
- [ ] restrictions on run start - choose from a pool for bonus rewards

**QoL & Polish**
- [ ] Status Effect vfx
- [ ] Minimap
- [ ] Build Guide menu
- [ ] skill tree node search (by name, stat, etc.)
- [ ] skill tree refund all button (show total gold cost, confirm)

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
- [ ] Customizable HUD layout
- [ ] Leaderboards (local/online) for boss rush/endless/highest dps
- [ ] Screenshot mode that hides the HUD
