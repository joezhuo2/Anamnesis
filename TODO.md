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

**Content**
- [ ] wave events - random events that can randomly occur during waves

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
- [ ] "Memory" collectibles scattered in waves that unlock lore snippets and permanent bonuses (new collectible type)
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

# Planned

### High priority To-Do
- **Gravity Well** - *something* create an aoe attack that pulls enemies (and debuffs them?)
- beacon objective (defend/destroy)
- [ ] dps counter

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

### Will do sometime
- [ ] "What's new" changelog popup on update
- [ ] reward history
- [ ] Customizable HUD layout
- [ ] data saving - full game runs
- [ ] Confirmation dialog before corrupting a reward (can be toggled in settings)
- [ ] Kill Streak (combo counter, `PlayerUpgrade` condition)

### Will Consider
- [ ] Keyboard/controller navigation for reward & skill tree menus (no mouse required)
- [ ] Scrollable Tooltips
- [ ] Screenshot mode that hides the HUD
- [ ] Accessibility options (colorblind mode, reduced screen shake, larger text)
- [ ] build/loadout slots
- [ ] neutral entities
- [ ] cosmetics (player skins/dash effects/attack effects)
- [ ] nameplates/titles
- [ ] background/ambience (debris/wind)

### Planned Abilities 
- **Exploit** - *something* applies *something else* to the target, increasing status effect damage taken by `{x}%` for each unique status effect are on the target
- **Something** - counter dodging creates a shockwave
- **Gravemark** (Basic) — marks the target instead of damaging it; the next *different* attack slot that hits a marked enemy detonates every mark. Opens a slot-rotation playstyle.
- **Riptide** (Ultimate) — rush that drags every enemy it passes through along with you (applies `Pulled` on contact), then drops them in a heap on `OnRushEnd`. Sets up AoE ultimates. (new rush dashing through enemies bool)
- **Overclock** (Ultimate) — no damage. For 8s, every cast advances all other cooldowns by 50%, but each cast costs stamina. When the timer ends you get `Overheat`.
- **Shatterpoint** — `OnCrit` against a stunned or frozen enemy: consumes the CC and deals 200% crit damage as true damage. 
- **Kinetic Theory** - knocking enemies into other enemies causes them to take contact damage scaling off of kbPct (after contact damage update)
- **Event Horizon** (Ultimate) — a slow `Spiral` projectile that `Pulled`s nearby enemies and grows over its lifetime.
- **Something** - something that grants thorns effect
- **Something** - something that grants life steal effect
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
- **red-pink**
- purple-blue
- green-yellow
- brown

### Planned Capstone Nodes
- Hex Cast (+buff -cost)

- Shattered Singularity - very slow speed (hard to hit), high costs
- Stellar maelstrom - very high costs, low homing strength
- Solar Collapse - weak pull strength, small aoe, does not move
- Exodus - long cooldown, multi scaling

- Meteor Shower - long cast time, small hitbox, large aoe, high mana cost
- Starfury - low aoe, primary speed scaling
- Autopilot - huge knockback (less grouping), low pierce
- Feedback Loop - low damage, not guaranteed

### Stats without skill tree nodes
- add spl dmg pct
- add dmg pct
- basic dmg pct
- skill dmg pct
- ult dmg pct
- basic cd red pct
- skill cd red pct
- ult cd red pct
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

  *New feature:* stack counter on `PlayerUpgrade`.
- [ ] **Contagion** — `OnKill`: status effects on the dying enemy spread to the 3 nearest enemies at 50% of their remaining duration. Generalizes `DoTSpread` to every effect.
- [ ] **Vampiric Resonance** — `OnOverkill`: excess damage heals you, and healing past max HP converts to overhealth. Uses overkill and overhealth.

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
- [ ] **Codex**: an encyclopedia of enemies, bosses, attacks, Awakenings and status effects, with undiscovered entries shown as silhouettes. Completion itself becomes a goal and doubles as in-game documentation.
- [ ] **Unlimited mutations**: every 25 waves in Unlimited, choose one of three permanent enemy mutations in exchange for a stacking score/reward multiplier. Keeps endless runs changing instead of just scaling numbers.

## New Anomalies

- [ ] **Grounded** (fail) - dashing is disabled for the wave, and the anomaly fails the moment a dash is attempted. Same shape as No Hit, so it is cheap to build and a real test for dash-reliant builds. *Uses:* `PlayerMovement.TryStartDash`, the `NoDamageTrialInstance` pattern.

- [ ] **Rampage** (no fail) - the boss starts the fight in its final phase (`EnemyPhase`) with reduced max HP. A short, brutal fight that skips the warm-up.
- [ ] **Unstoppable** (no fail) - the boss is immune to knockback, stun and other hard CC for the wave, but takes `+x%` more damage. Turns CC-lock builds into raw damage races.
- [ ] **Entourage** (no fail) - common enemies from the wave pool keep trickling in during the boss fight, and every escort alive grants the boss `x%` damage reduction. Forces target priority between boss and adds. (configurable max)

- [ ] **Precision** (fail) - land at least `x%` of your attacks (hits / casts). Fails at wave end if under the threshold. Rewards aim over spam.
- [ ] **Overcharged** (no fail) - all attack cooldowns are reduced by `y%`, but every cast costs `x%` more resources. Feeds `OnCast` awakenings hard.

### Boss only
- [ ] **Twin Crowns** (no fail) - a second, weaker copy of the boss spawns alongside it (both at reduced HP). Killing one enrages the other (+atk spd). Bigger reward.

### Non-boss only
- [ ] **Last Stand** (no fail) - the final `x` enemies of the wave become empowered (size, damage, speed) once the rest are dead. Makes the end of a wave a mini-duel.
- [ ] **Pacifist Start** (fail) - you cannot deal damage for the first `x` seconds of the wave; dealing damage early fails it. Kite and survive, then clean up. *Uses:* the `NoDamageTrialInstance` pattern, inverted to outgoing damage.
- [ ] **Bloodtide** (no fail) - every enemy kill heals all other living enemies by `x%` max HP. Rewards burst and focus over spreading damage.
- [ ] **Stampede** (no fail) - enemies spawn all at once at the start of the wave instead of trickling in (ignores `maxCurrentEnemies`), but with reduced HP.

### Any wave
- [ ] **Shrinking Arena** (no fail) - a closing ring hurts the player outside it and resets at wave end. Forces close-range fighting. *Uses:* the `BlackoutVision` circle approach for the ring.
- [ ] **Echoes** (no fail) - every enemy attack fires a delayed second copy after 0.5s. Doubles the bullet hell without adding enemies; works on bosses and commons alike.

### Bugs/Cleanup
- [ ] **Same `effName` merges different assets** - `StatusEffectManager.IsSameEffect` matches by name, so Slow (5 assets), Stun (4), Possessed (4), Vulnerable (3) and Burn (2) share one instance. A stronger variant just refreshes or stacks the weaker one's parameters.
- [ ] **No end-of-run state** - after the last Regular wave, `StartNextWave` just returns (`WaveManager.cs:361`), with no victory screen. Unlimited shows "Wave x/128" even though it is endless.
- [ ] **EventSystem lives under the Player prefab** - if the player is destroyed (after `deathAnimTime` 1s) before the death screen pauses (`showDelay` 1s, real time), the death-screen buttons lose input. `GameController`'s fallback adds a `StandaloneInputModule` (`GameController.cs:32`), which does not work with `activeInputHandler: 1`.
- [ ] Stale Player overrides in `New.unity` (~line 65210): `skillPoints` (field no longer exists) and `activeUpgrades.Array.data[0..1]` (array size 0; data[0] points at a deleted asset `f108d857...`).

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
