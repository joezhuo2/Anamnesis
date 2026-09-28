# Planned Features 

## Pre [v1.0.0] Checklist — First Light
*Everything that has to be true before a stranger can play it.*

**Bigger Things**
- [ ] Audio (SFX + music buses + menu volume control)
- [ ] more enemy projectile telegraphs (jellyfish ball)

**Smaller Things**
- [ ] Confirm every `CREDITS.md` asset license permits redistribution inside a compiled build, not just use
- [ ] In-game credits/attribution screen — some of those licenses want attribution in the build itself
- [ ] Resolution/window options (currently a fixed 1920x1080 with `resizableWindow: 0`)
- [ ] Clean-machine pass — fresh build, no `settings.json`, no Unity installed
- [ ] Store page — description, screenshots, capsule art, controls
- [ ] Window icon + splash

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
- [ ] map debris/decor
- [ ] Background Overlays - reward menu, home screen, settings menu, scroll menu, death menu

## Pre [v1.2.0] Checklist — Threads of Fate
*Every wave stops looking the same; the settings/stats menus catch up.*

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
- [ ] full run saving

**Content**
- [ ] multiple map sections
- [ ] Environmental hazards on maps (spikes, lava, traps)
- [ ] portals
- [ ] starting builds / starting kits
- [ ] skill tree node search bar (by name, stat, etc.) - highlight nodes that are filtered (change border color), and changes other nodes to have gray/dark gray borders

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
- [ ] queue skill point spending - different highlight color, can be undone
- [ ] skill node undo grace window (no undo cost until closing the tree)

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
- **Something** - counter dodging creates a shockwave
- **Gravity Well** - *something* create an extremely massive aoe attack that pulls enemies and debuffs them
- **Gravemark** (Basic) — marks the target instead of damaging it; the next *different* attack slot that hits a marked enemy detonates every mark. Opens a slot-rotation playstyle.
- **Riptide** (Ultimate) — rush that drags every enemy it passes through along with you (applies `Pulled` on contact), then drops them in a heap on `OnRushEnd`. Sets up AoE ultimates. (new rush dashing through enemies bool)
- **Overclock** (Ultimate) — no damage. For 8s, every cast advances all other cooldowns by 50%, but each cast costs stamina. When the timer ends you get `Overheat`.
- **Shatterpoint** — `OnCrit` against a stunned or frozen enemy: consumes the CC and deals 200% crit damage as true damage. 
- **Resonance** — using the same attack slot 3 times in a row empowers the 4th cast
- **Kinetic Theory** - knocking enemies into other enemies causes them to take contact damage scaling off of kbPct (after contact damage update)
- **Midas Touch** — passive. Every 250 gold held grants +1% `damagePct`
- **Phoenix Flare** - allows one rebirth every `{x}` waves, and creates a massive explosion on trigger
- **Event Horizon** (Ultimate) — a slow `Spiral` projectile that `Pulled`s nearby enemies and grows over its lifetime.
- **Something** - something that grants thorns effect
- **Something** - something that grants life steal effect
- **Mitosis Shot** (Basic) — each projectile splits into 3 smaller copies on hit
- **Scatter Mine** (Basic) — random-direction (`randomDir`) mines that sit still and arm after 0.5s. Opens a trap/kiting playstyle. new `armDelay` (no collision until armed).
- **Glacial Lance** (Skill) — applies `Freeze`; hitting a frozen enemy shatters it for bonus true damage and splashes `Slow` onto nearby enemies.

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
- basic cd red pct
- skill cd red pct
- ult cd red pct
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

- [ ] Physics layers: everything sits on `Default` and the 2D collision matrix is all-ones
  (`ProjectSettings/Physics2DSettings.asset:56`). Projectile triggers pair with other projectiles, pickups and
  walls, so dense barrages get O(n²) broadphase pairs, plus `OnTriggerEnter2D`/`OnTriggerStay2D` callbacks
  every physics step (`Projectile.OnTriggerEnter2D`/`OnTriggerStay2D`: `hit.Contains` + interface `TryGetComponent` per pair per step).
  Fix: add Player/Enemy/Projectile/Environment/Pickup layers, disable Projectile↔Projectile and
  Projectile↔Pickup. The overlap queries in `Projectile`/`EntityProjectileHandler` already skip triggers (v0.6.3);
  an entity LayerMask would also drop walls from them.

---

## Brainstorm: New Attacks & Awakenings

### Attacks
- [ ] **Chakram** (Basic) — boomerang (`maxBoomerangDist`) that re-hits everything on the way back and speeds up per enemy pierced. Scales with `ProjSpd`, which almost nothing uses today.
- [ ] **Siphon Chain** (Basic) — chain lightning (`additionalAttack` retarget) that applies `Lifesteal` to the player per hop. Pairs with sustain builds.
  *Uses:* the unused `Lifesteal` effect.

### Awakenings (PlayerUpgrade)

- [ ] **Perfect Parry** — `OnCounterDodge`: reflects the incoming hit as a projectile toward its source and refunds the dash cooldown. Deepens dash play.
  *New feature:* stack counter on `PlayerUpgrade`.
- [ ] **Contagion** — `OnKill`: status effects on the dying enemy spread to the 3 nearest enemies at 50% of their remaining duration. Generalizes `DoTSpread` to every effect.
- [ ] **Vampiric Resonance** — `OnOverkill`: excess damage heals you, and healing past max HP converts to overhealth. Uses overkill and overhealth.
- [ ] **Gravity Well** — `OnRushEnd`: spawns a small `Pulled` field at the rush endpoint. Links Riptide, Kinetic Slam and Subspace Blitz.

---

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

### 3. Furnace — heat resource

A fourth resource, Heat, fills as you attack and bleeds off over time. High heat makes attacks stronger but burns you; venting heat turns it into burst damage.

*Core feature:* a `heat` / `maxHeat` stat in `PlayerResourcePool` plus a HUD bar, attack cost entries that *add* heat, burning the player above 80% heat, and a new `SpecialScalingAttribute.Heat`.

- [ ] **Stoke** (Basic) — fast jab that adds 8 heat. Its damage scales with current heat.
- [ ] **Vent** (Skill) — dumps all heat as a cone of fire. Damage scales with the heat spent, and it applies Burn stacks equal to heat/20.
- [ ] **Meltdown** (Ultimate) — locks heat at max for 10s with no self-burn. When it ends, releases a full-screen explosion and you become `Overheat`ed.
- [ ] **Heat Sink** (Awakening) — `OnTakeDamage`: converts 50% of the damage taken into heat instead of HP loss while below 80% heat.
- [ ] **Thermal Runaway** (Awakening) — passive. Every 10 heat held grants +2% `attackSpeedPct`; the self-burn tick rate also scales with heat.
- [ ] **Cooling Dash** (Awakening) — `OnStartDash`: vents 25 heat as a ring of `Slow`.

### 4. Duality — stance switching

The player swaps between two stances, Sol and Luna. Every attack has a different form in each stance, and switching mid-combo pays off.

*Core feature:* a `Stance` state on the player with a swap binding, an alternate `ProjectileData` per stance on `AttackData` (swapped in the way `AttackReplacement` does), and a new `OnStanceSwap` trigger condition.

- [ ] **Twin Moons** (Basic) — Sol: a single heavy physical slash. Luna: 3 spell needles in a spread that apply Vulnerable.
- [ ] **Eclipse Step** (Skill) — swaps stance instantly and teleports behind the nearest enemy. The first attack in the new stance crits.
- [ ] **Equinox** (Ultimate) — for 8s, both stances are active at once, so every attack fires both forms.
- [ ] **Balance** (Awakening) — passive. Damage in Sol raises Luna's damage for 3s, and damage in Luna raises Sol's, so the bonuses alternate when you swap.
- [ ] **Twilight Burst** (Awakening) — `OnStanceSwap`: releases a ring that deals damage and cleanses one debuff. 1.5s cooldown.
- [ ] **Zenith** (Awakening) — `OnStanceSwap` after at least 5s in one stance: the next attack deals +150% damage.

### 6. Tether — linked enemies

The player links enemies together with chains. Damage, status effects and knockback travel along the links, so one target stands in for a whole group.

*Core feature:* a `Tethered` status effect that stores a link to another entity and draws a line between them (pooled `LineRenderer`). Damage to either end copies a percentage to the other, with a reentrancy guard. Adds a new `OnTetherBreak` trigger condition.

- [ ] **Chainlink** (Basic) — a projectile that tethers the first two enemies it pierces. Links last 5s and share 30% of damage.
- [ ] **Drag Net** (Skill) — pulls every tethered enemy toward the center of its link, using `Pulled`. Enemies that collide take rush impact damage.
- [ ] **Constellation** (Ultimate) — tethers every enemy on screen to its 2 nearest neighbours for 8s. Links share 60% of damage and every status effect applied.
- [ ] **Load Bearing** (Awakening) — `OnTetherBreak`: the chain snaps and deals the damage it carried to both ends.
- [ ] **Conductive Chains** (Awakening) — passive. DoT ticks travel along tethers, but can't travel back along the link they came from.
- [ ] **Anchor** (Awakening) — passive. Tethering to a boss or elite makes that end immovable, so knockback on the other end is doubled.

---

### Planned
- [ ] **Boss relics**: each boss drops a choice between two unique, boss-themed Awakenings that can only come from that boss (e.g. a Golem armor-to-rush-impact relic, a Reaper execute relic). Makes each boss kill memorable instead of just another reward panel.

## Brainstorm: QoL, Gameplay & Replayability

### QoL
- [ ] **Banish**: a limited per-run charge that removes a reward from its pool for the rest of the run. Allowed in Ironman since it is not a take-back.
- [ ] **Reward compare**: hovering a reward shows `current -> new` for every stat it touches, and attack replacements show cooldown/damage/scaling side by side with the attack they replace.
- [ ] **Capstone path badge**: rewards that satisfy a capstone or keystone requirement in the skill tree get a small badge so players can spot build-defining picks without memorizing the tree.
- [ ] **Quick retry**: a death screen button (and hotkey) that restarts straight into the same gamemode, difficulty and Ironman setting, skipping the home screen.
- [ ] **Owned upgrades list**: a pause menu tab listing every Awakening, capstone and synergy owned this run with full tooltips, so it is possible to check what the build actually does mid-run.
- [ ] **Damage number filters**: settings to hide DoT ticks, merge rapid hits on one target into a running total, or show only crits/big hits.
- [ ] **Cooldown ready cue**: a short flash (and sound once audio lands) on the HUD button when an Ultimate or stacked attack comes off cooldown.
- [ ] **First-run hints**: one-time contextual tips (first dash, first corruption, first anomaly, first skill point) that can be reset in settings. Fits v1.0 "a stranger can play it".

### Gameplay
- [ ] **Tempo bonus**: each wave has a par time; clearing under par pays bonus gold/XP scaled by how far under. Rewards aggressive builds and gives a reason to push.
- [ ] **Bounty target**: one enemy per wave is marked and flees or buffs itself over time; killing it before it escapes drops a guaranteed collectible or reroll.
- [ ] **Hunter**: if a wave drags on too long, an elite spawns that relentlessly chases the player and drops a rare reward if killed. Anti-stall pressure with an upside.
- [ ] **Awakening fusions**: specific Awakening pairs fuse into a stronger combined version when both are owned (e.g. Supersonic + Chaos Theory). Hidden recipes the player discovers, a concrete form of the v1.6 synergy bonuses.

*Theme: memory & recollection*
- [ ] **Echo of a past self**: a rare mini-boss that uses your *previous* run's attacks and Awakenings (read from the saved loadout). Defeating it lets you take one of its rewards into this run. Every run leaves something behind for the next one.
- [ ] **Deja vu**: a wave replays an earlier wave's exact spawns while a faint ghost trail shows how you moved last time. Beat your earlier clear time for a bonus reward, and it quietly shows how much stronger the build has become.
- [ ] **Constellation tracing**: stars light up across the map mid-wave; touch them in the drawn order before they fade to complete a constellation, which grants a named buff for the rest of the wave (a different one for each constellation, each with a lore line). Pulls the player around the map instead of kiting in circles.

*Wave events & side objectives (candidates for the v1.2 wave events pool)*
- [ ] **Whispers**: short timed micro-objectives that pop up during a wave, like "kill 10 burning enemies in 20s", "take no damage for 15s" or "crit the elite 3 times". Each completed whisper pays a small reward, and a streak of them pays a bigger one.
- [ ] **Resonance pillars**: three pillars that each have to be hit by a different attack slot (Basic, Skill, Ultimate) within a short window. Lighting all three releases a shockwave and drops a reward. Rewards slot rotation over spamming one attack.
- [ ] **Meteorfall**: telegraphed meteors crash onto the map, damaging whatever is under them (enemies included). Each crater leaves a crystal that you can break open for gold, or leave to charge into a temporary buff zone. Lets you bait enemies into impacts.
- [ ] **Lost wisp escort**: a friendly wisp appears and drifts slowly toward a beacon while enemies switch to target it. Get it there alive for a reward, with a bigger reward if it arrives above 50% HP. The mirror image of the planned defend/destroy beacon.
- [ ] **Rift trial**: a rift tears open for 10s; step in to be pulled into a short pocket arena with an elite pack while the main wave is frozen. Clear it within the time limit to come back with a guaranteed rare reward; fail and you come back with a debuff.
- [ ] **Eclipse**: the screen darkens to a light radius around the player, and enemies outside it only show as glowing eyes. Lanterns scattered around the map widen the radius when lit, and enemies killed in the dark drop double gold.

*New ways to earn rewards*
- [ ] **Blind draw**: a reward panel variant with face-down cards and better rarity odds. You commit to a card before seeing it.
- [ ] **Transmute altar**: sacrifice an owned reward to reroll it into a random reward one rarity tier higher from the same pool. A risky upgrade path that is separate from corruption.
- [ ] **Overkill cache**: overkill damage fills a hidden meter; when it is full a crystal cache drops at the last enemy's position. Makes overkill-heavy nuke builds pay out beyond the kill itself.

### Replayability
- [ ] **Recollection (meta progression)**: every run pays a persistent currency based on waves cleared, bosses killed and difficulty. Spent on permanent unlocks: new attacks entering the reward pools, an extra starting reroll, extra pre-run picks, new starting kits. Pairs with the v1.4 Memory collectibles.
- [ ] **Codex**: an encyclopedia of enemies, bosses, attacks, Awakenings and status effects, with undiscovered entries shown as silhouettes. Completion itself becomes a goal and doubles as in-game documentation.
- [ ] **Unlimited mutations**: every 25 waves in Unlimited, choose one of three permanent enemy mutations in exchange for a stacking score/reward multiplier. Keeps endless runs changing instead of just scaling numbers.
