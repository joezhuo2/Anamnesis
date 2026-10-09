# Planned Features 
## High priority To-Do
- [6] move paradox to skill tree
- [4] **Veil of Stars** (Ultimate) — creates an aura with `configurable `tile radius around you (with overlay) that grants a `status effect`. Enemy projectiles that enter have a `configurable` chance to it spawn a `projectile`.
- [4] **Anvil Stance** standing still for 1s grants a buff. Moving drops it after a 0.5s grace.
- [3] **Gravity Well** - teleporing, dashing, and rushing create an aoe attack that pulls enemies
- [3] **Repulsion** - counter dodging creates a shockwave
- [2] finish everything in between first and second ring in skill tree
- [1] skill tree third circular ring (no connections yet)

## Medium Priority To-Do
- new boss rush gamemode, and starting build
- beacon objective (defend/destroy)

## Pre [v1.0.0] Checklist — First Light
*Everything that has to be true before a stranger can play it.*

**Bigger Things**
- [ ] Audio (SFX + music buses + menu volume control)
- [ ] more enemy projectile telegraphs (jellyfish ball, lich drill, cultist tp, golem bullet hell)

**Smaller Things**
- [ ] Confirm every `CREDITS.md` asset license permits redistribution inside a compiled build, not just use
- [ ] In-game credits/attribution screen — some of those licenses want attribution in the build itself
- [ ] Resolution/window options (currently a fixed 1920x1080 with `resizableWindow: 0`)
- [ ] Clean-machine pass — fresh build, no `settings.json`, no Unity installed
- [ ] Store page — description, screenshots, capsule art, controls
- [ ] Window icon + splash

## Features By Category

### Content

- [8] ram dash (dash upgrade, dashing into enemies deal damage based on `x` and sends you back, has more iframes)

- [7] Skill Points (? name) update: atk/dex/int/agi/vit/?/def/?

- [6] wave events - random events that can randomly occur during waves

- [5] Elite/Champion enemy/boss variants with unique modifiers (extra stats, new ai, splitting)
- [5] Player new "signature" that charges via a new special resource instead of a cooldown
- [5] Elite "aura" variants that buff nearby enemies (e.g. attack speed, damage reduction) — encourages target prioritization
- [5] "Memory" collectibles scattered in waves that unlock lore snippets and permanent bonuses (new collectible type)
- [5] Techniques - utility/QoL featured (blink tp, buff, crowd control)

- [4] Player summons

- [3] Combo/synergy bonuses for stacking related rewards
- [3] deployables (eg. totems/auras)

- [2] second skill tree (Prestiage/Ascension/Mastery)
- [2] portals
- [2] multiple map sections
- [2] Environmental hazards on maps (spikes, lava, traps)
- [2] Daily/weekly challenge modifiers with seeded runs

- [1] Finish Gear/Item system (slots, rarity tiers, stat rolls, equip/unequip)
- [1] Consumables (potions, bombs, temporary buffs) with hotkeys
- [1] Shop/merchant between waves to spend currency on items or stat boosts
- [1] Chests or loot drops from elites/bosses with guaranteed rare rewards
- [1] Elemental Damage/Defense system
- [1] Elemental affinities/weaknesses
- [1] attack mastery (use more to level up)
- [1] Crafting/enchanting system for gear
- [1] Set bonuses for equipping matching gear pieces
- [1] Elemental reactions

### Graphics
- [2] Status Effect vfx

- [1] map debris/decor
- [1] Background Overlays - reward menu, home screen, settings menu, scroll menu, death menu

### QoL
- [6] enemy status effect overlay on common enemies
- [6] Screen-edge indicators for off-screen enemies, boss cursor

- [5] full run saving

- [4] reward history
- [4] Post-Death Summary (run grade)

- [3] Suggest certain stats based on player's current loadout
- [3] Achievement system with unlock notifications
- [3] Full stats display menu (in settings panel)

- [2] Leaderboards (local/online) for boss rush/endless/highest dps
- [2] Customizable HUD layout

- [1] FPS counter & performance stats debug toggle
- [1] damage breakdown (by attack, every x waves)
- [1] run archive
- [1] "What's new" changelog popup on update
- [1] data saving - full game runs
- [1] Build export/share — copy current loadout as text for sharing
- [1] Confirmation dialog before corrupting a reward (can be toggled in settings)
- [1] Status effect sort options (duration, num of stacks, etc.) - configurable in settings

# Planned

#### Expert Attacks
- slime => poison splash
- frost slime => splitting ice shards
- magma slime => explosion on spear hit
**- reaper => long distance pull to self**
- golem => ?

#### Master Attacks
- slime =>
- magma slime => self buff (+spd +atk)
- reaper => 
- jellyfish =>
- bat =>

### Will Consider
- [ ] Keyboard/controller navigation for reward & skill tree menus (no mouse required)
- [ ] Scrollable Tooltips
- [ ] Screenshot mode that hides the HUD
- [ ] Accessibility options (colorblind mode, reduced screen shake, larger text)
- [ ] build/loadout slots
- [ ] neutral entities
- [ ] cosmetics (player skins/dash effects/attack effects)
- [ ] nameplates/titles
- [ ] Minimap
- [ ] Build Guide menu
- [ ] background/ambience (debris/wind)
- [ ] choose next wave style
- [ ] contact damage
- [] attack combo chains

### Planned Abilities 
- **Exploit** - *something* applies *something else* to the target, increasing status effect damage taken by `{x}%` for each unique status effect are on the target
- **Gravemark** (Basic) — marks the target instead of damaging it; the next *different* attack slot that hits a marked enemy detonates every mark. Opens a slot-rotation playstyle.
- **Riptide** (Ultimate) — rush that drags every enemy it passes through along with you (applies `Pulled` on contact), then drops them in a heap on `OnRushEnd`. Sets up AoE ultimates. (new rush dashing through enemies bool)
- **Overclock** (Ultimate) — no damage. For 8s, every cast advances all other cooldowns by 50%, but each cast costs stamina. When the timer ends you get `Overheat`.
- **Kinetic Theory** - knocking enemies into other enemies causes them to take contact damage scaling off of kbPct (after contact damage update)
- **Event Horizon** (Ultimate) — a slow `Spiral` projectile that `Pulled`s nearby enemies and grows over its lifetime.
- **Something** - something that grants thorns effect
- **Scatter Mine** (Basic) — random-direction (`randomDir`) mines that sit still and arm after 0.5s. Opens a trap/kiting playstyle. new `armDelay` (no collision until armed).
- **Perfect Parry** — `OnCounterDodge`: reflects the incoming hit as a projectile toward its source and refunds the dash cooldown. Deepens dash play.

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

## Misc

### Available Colors
- purple-blue
- green-yellow
- brown

### Planned Capstone Nodes
- Hex Cast (+buff -cost)

- Shattered Singularity - very slow speed (hard to hit), high costs
- Solar Collapse - very weak pull strength, small aoe, does not move
- Exodus - long cooldown, multi scaling

- Starfury - low aoe, primary speed scaling
- Feedback Loop - low damage, not guaranteed

## Performance Improvements

### Medium

- [ ] Physics layers: everything sits on `Default` and the 2D collision matrix is all-ones
  (`ProjectSettings/Physics2DSettings.asset:56`). Projectile triggers pair with other projectiles, pickups and
  walls, so dense barrages get O(n²) broadphase pairs, plus `OnTriggerEnter2D`/`OnTriggerStay2D` callbacks
  every physics step (`Projectile.OnTriggerEnter2D`/`OnTriggerStay2D`: `hit.Contains` + interface `TryGetComponent` per pair per step).
  Fix: add Player/Enemy/Projectile/Environment/Pickup layers, disable Projectile↔Projectile and
  Projectile↔Pickup. The overlap queries in `Projectile`/`EntityProjectileHandler` already skip triggers (v0.6.3);
  an entity LayerMask would also drop walls from them.

## Brainstorm: New Playstyle Sets

### 2. Legion — player summoner

The player fights through a squad of minions and buffs, sacrifices or commands them. Reuses the enemy-side `EntitySummonHandler`.

*Core feature:* player-owned summons with an `EntityStats` block that scales off the player's stats, a minion cap, target-following AI that reuses `EnemyMovement` with the target set to the nearest enemy, and new `OnMinionSpawn` / `OnMinionDeath` trigger conditions.

- [ ] **Conscript** (Basic) — a weak hit whose kills raise the slain enemy as a minion for 10s (up to the cap).
- [ ] **Rally** (Skill) — every minion rushes to the cursor, using rush impact for its damage. Minions arriving together share their knockback.
- [ ] **Grand Muster** (Ultimate) — fills the minion cap with elite minions that copy the player's equipped basic attack for 15s.
- [ ] **Blood Tithe** (Awakening) — `OnMinionDeath`: heal 3% max HP and advance skill cooldown by 0.5s.
- [ ] **Shared Vessel** (Awakening) — passive. Minions inherit 30% of the player's crit chance and status effects on hit, and 20% of the damage the player takes is redirected to the nearest minion.
- [ ] **Martyr** (Awakening) — `OnTakeHit` while a minion exists: sacrifice the oldest minion to negate the hit, and it explodes for its remaining HP.

### Duality — stance switching (After techniques)

The player swaps between two stances, Sol and Luna. Every attack has a different form in each stance, and switching mid-combo pays off.

*Core feature:* a `Stance` state on the player with a swap binding, an alternate `ProjectileData` per stance on `AttackData` (swapped in the way `AttackReplacement` does), and a new `OnStanceSwap` trigger condition.

- [ ] **Twin Moons** (Basic) — Sol: a single heavy physical slash. Luna: 3 spell needles in a spread that apply Vulnerable.
- [ ] **Eclipse Step** (Skill) — swaps stance instantly and teleports behind the nearest enemy. The first attack in the new stance crits.
- [ ] **Equinox** (Ultimate) — for 8s, both stances are active at once, so every attack fires both forms.
- [ ] **Balance** (Awakening) — passive. Damage in Sol raises Luna's damage for 3s, and damage in Luna raises Sol's, so the bonuses alternate when you swap.
- [ ] **Twilight Burst** (Awakening) — `OnStanceSwap`: releases a ring that deals damage and cleanses one debuff. 1.5s cooldown.
- [ ] **Zenith** (Awakening) — `OnStanceSwap` after at least 5s in one stance: the next attack deals +150% damage.

---

## Brainstorm: QoL, Gameplay & Replayability
- [ ] **Boss relics**: each boss drops a choice between two unique, boss-themed Awakenings that can only come from that boss (e.g. a Golem armor-to-rush-impact relic, a Reaper execute relic). Makes each boss kill memorable instead of just another reward panel.
- [ ] **Owned upgrades list**: a pause menu tab listing every Awakening, capstone and synergy owned this run with full tooltips, so it is possible to check what the build actually does mid-run.
- [ ] **Damage number filters**: settings to hide DoT ticks, merge rapid hits on one target into a running total, or show only crits/big hits.
- [ ] **Awakening fusions**: specific Awakening pairs fuse into a stronger combined version when both are owned (e.g. Supersonic + Chaos Theory). Hidden recipes the player discovers, a concrete form of the v1.6 synergy bonuses.

### QoL
- [ ] **Banish**: a limited per-run charge that removes a reward from its pool for the rest of the run. Allowed in Ironman since it is not a take-back.
- [ ] **First-run hints**: one-time contextual tips (first dash, first corruption, first anomaly, first skill point) that can be reset in settings. Fits v1.0 "a stranger can play it".

### Gameplay

*Theme: memory & recollection*
- [ ] **Constellation tracing**: stars light up across the map mid-wave; touch them in the drawn order before they fade to complete a constellation, which grants a named buff for the rest of the wave (a different one for each constellation, each with a lore line). Pulls the player around the map instead of kiting in circles.

*Wave events & side objectives (candidates for the v1.2 wave events pool)*
- [ ] **Resonance pillars**: three pillars that each have to be hit by a different attack slot (Basic, Skill, Ultimate) within a short window. Lighting all three releases a shockwave and drops a reward. Rewards slot rotation over spamming one attack.
- [ ] **Meteorfall**: telegraphed meteors crash onto the map, damaging whatever is under them (enemies included). Each crater leaves a crystal that you can break open for **a new type of reward**
- [ ] **Lost wisp escort**: a friendly wisp appears and drifts slowly toward a beacon while enemies switch to target it. Get it there alive for a reward, with a bigger reward if it arrives above 50% HP. The mirror image of the planned defend/destroy beacon.

*New ways to earn rewards*
- [ ] **Blind draw**: a reward panel variant with face-down cards and better rarity odds. You commit to a card before seeing it.

### Replayability
- [ ] **Recollection (meta progression)**: every run pays a persistent currency based on waves cleared, bosses killed and difficulty. Spent on permanent unlocks: new attacks entering the reward pools, an extra starting reroll, extra pre-run picks, new starting kits. Pairs with the v1.4 Memory collectibles.
- [ ] **Codex**: an encyclopedia of enemies, bosses, attacks, Awakenings and status effects, with undiscovered entries shown as silhouettes. Completion itself becomes a goal and doubles as in-game documentation, with a save/load state.
- [ ] **Unlimited mutations**: every 25 waves in Unlimited, choose one of three permanent enemy mutations in exchange for a stacking score/reward multiplier. Keeps endless runs changing instead of just scaling numbers.

## New Anomalies

- [ ] **Grounded** (fail) - dashing is disabled for the wave, and the anomaly fails the moment a dash is attempted. Same shape as No Hit, so it is cheap to build and a real test for dash-reliant builds. *Uses:* `PlayerMovement.TryStartDash`, the `NoDamageTrialInstance` pattern.

- [ ] **Entourage** (no fail) - common enemies from the wave pool keep trickling in during the boss fight, and every escort alive grants the boss `x%` damage reduction. Forces target priority between boss and adds. (configurable max)

### Non-boss only
- [ ] **Last Stand** (no fail) - the final `x` enemies of the wave become empowered (size, damage, speed) once the rest are dead. Makes the end of a wave a mini-duel.
- [ ] **Pacifist Start** (fail) - you cannot deal damage for the first `x` seconds of the wave; dealing damage early fails it. Kite and survive, then clean up. *Uses:* the `NoDamageTrialInstance` pattern, inverted to outgoing damage.
- [ ] **Bloodtide** (no fail) - every enemy kill heals all other living enemies by `x%` max HP. Rewards burst and focus over spreading damage.

### Any wave
- [ ] **Shrinking Arena** (no fail) - a closing ring hurts the player outside it and resets at wave end. Forces close-range fighting. *Uses:* the `BlackoutVision` circle approach for the ring.
- [ ] **Echoes** (no fail) - every enemy attack fires a delayed second copy after 0.5s. Doubles the bullet hell without adding enemies; works on bosses and commons alike.

## Brainstorm: Expert/Master Modes & Nightmare

### Expert mode
- [ ] **Elite enemies** — Elite/Champion variants (v1.1) only spawn from Expert up. Gives Expert its own enemy layer instead of borrowing Nightmare's attack tier. *Uses:* a `minMode` on the wave spawn entry.

### Master mode
- [ ] **Boss relics** — Master bosses drop the boss-themed relic choice. Simple/Expert bosses keep the normal reward panel. (also change some existing attacks/awakenings to be boss specific)
- [ ] **Attack evolutions** — at attack mastery (v1.5) or at wave 50, an attack can evolve into a Master-only variant

### Mode QoL
- [ ] **Mode + difficulty records** — the death screen and the future run archive store best wave per (mode, difficulty, ironman) combo.
- [ ] **Master + Nightmare + Ironman badge** — a named title/frame for clearing the hardest combo. Ties into nameplates/cosmetics.

### Nightmare difficulty

- [ ] **Wave affixes** — from wave 10, each block of 10 waves rolls one affix shown on the wave HUD: *Bolstering* (enemy deaths buff nearby enemies), *Sanguine* (deaths leave a pool that heals enemies), *Splitting* (common enemies split once), *Volcanic* (ground eruptions under the player). Every 30 waves add a second affix. One hosting system for several of the ideas above. *New fields:* `DifficultyData.affixStartWave`, `affixInterval`.
- [ ] **Shorter telegraphs** — enemy attack telegraphs run 25% faster. *New field:* `DifficultyData.telegraphTimeMult`.
- [ ] **Reward timer** — reward panels auto-pick a random card after 30s. Real-time pressure without making the waves harder.
- [ ] **Level breakpoints** — every 25 waves, all enemies permanently gain a "star": +1 projectile, +10% size or +1 phase, chosen at random and shown on the wave track.
- Elite enemies can have an aura (attack speed, damage reduction or healing) and it stacks.
- Killed enemies drop a short-lived hazard on the ground.
- Enemies sometimes dodge-dash sideways out of your aim.
- Wave timer: after 60s, extra enemies spawn every 8s until the wave is cleared.

### Nightmare payoff
*Harder needs to feel worth it, not just be a number.*
- [ ] **Nightmare-only rewards** — a small Awakening pool (new `PlayerUpgrade.minDifficulty`, set to 3) that only appears in Nightmare, e.g. "+1% damage per wave survived without healing".
