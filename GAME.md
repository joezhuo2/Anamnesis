# Game Data Reference

Synced against the assets in `Assets/data/PlayerData/` and the `WaveManager` reward
pools serialized in `Assets/New.unity`. Damage multipliers are shown as percentages
(asset value x 100). Scaling stats use the exact `StatType` enum name.

Attack entries list the `AttackData` asset name; the paired `ProjectileData` asset is
the same name with `PD` instead of `AD` unless noted.

---

# Starting Attacks

Folder: `Assets/data/PlayerData/Attacks/Base`

## Lacerate
- Asset: `p_b_ad` / `p_b_pd`
- Type: Basic
- Cooldown: 1s
- Pattern: Single (1 count)
- Spawn: 0.5 dist
- Animation: 0.5s
- Gains on hit: Stamina +6, Mana +4
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 3
  - Size: 2.5
  - Damage: 180% Phys
  - Scaling: EffAtk
  - Rotation: 270 degrees
  - Knockback: 3 force for 0.15s

## Cyclone Cleave
- Asset: `p_s_ad` / `p_s_pd`
- Type: Skill
- Cooldown: 6s
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0.5s
- Costs: Stamina 15
- Gains on hit: Mana +8%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 8
  - Size: 3
  - Damage: 565% Phys, 125% Spell
  - Scaling: EffAtk
  - Knockback: 5 force for 0.15s

---

# Rare Pool

Folder: `Assets/data/PlayerData/Attacks/Rare Pool`. All 19 entries below are present in
`WaveManager.rarePool`. Entries marked with an unlock wave carry a `minWave` on their
`AttackReward` and cannot be rolled before that wave; the rest are available from wave 1.
Seven of them also sit in `corruptionSpecialPool` at a much lower unlock wave — see
[Corruption Special Pool](#corruption-special-pool).

## Aphelion
- Asset: `Aphelion AD`
- Type: Basic
- Cooldown: 2.2s
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 0.5s
- Costs: Stamina 6, Mana 8
- Gains on hit: Stamina +1, Mana +2%
- Projectile:
  - Speed: 6
  - Lifetime: 10.5s
  - Pierce: 3000
  - Size: 2
  - Damage: 35% Spell
  - Scaling: EffInt
  - Time Before Same Enemy: 0.5s
  - Orbit: radius 1 (+1 random), orbits self, CCW
  - Knockback: none

## Astral Nova
- Asset: `Astral Nova AD`
- Type: Basic
- Cooldown: 3s
- Pattern: Single (1 count)
- Spawn: 3 dist, 1.5s delay
- Animation: 0.5s
- Gains on hit: Stamina +15%, Mana +15%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 6
  - Size: 2
  - Damage: 280% Spell, 30% True
  - Scaling: EffInt
  - Effect: 100% on hit (Vulnerable, 8s, max 2 stacks, -20% damageRes per stack)
  - Knockback: none

## Blaze
- Asset: `Blaze A AD`
- Type: Basic
- Cooldown: 26s
- Pattern: Single (1 count)
- Spawn: 0.5 dist, 0.25s delay
- Animation: 1s
- Gains on hit: Stamina +6, Mana +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 4
  - Damage: 930% Phys
  - Scaling: EffAtk
  - Time Before Same Enemy: 0.5s
  - Effect: 100% self on cast (Blaze Soul, 6s, replaces the attack with Cosmic Blaze)
  - Additional: 30% chance on hit to create Blaze Spark
  - Knockback: none

## Cosmic Blaze
- Asset: `Blaze A1 AD` (the Blaze Soul replacement form)
- Type: Basic
- Cooldown: 1s
- Pattern: Single (1 count)
- Spawn: 0.5 dist
- Animation: 0.5s
- Costs: Stamina 18 +8%
- Gains on hit: Stamina +2 +1%
- Projectile:
  - Speed: 12
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2
  - Damage: 685% Phys, 30% True
  - Scaling: EffAtk
  - Effects: 100% self on cast (Blaze Soul, refreshes the 6s replacement) + 100% self on
    cast (Heartburn, 6s, max 15 stacks: +4% damagePct, +12% critDamage, +18% stCostPct,
    -16% hpRegPct per stack)
  - Additional: 40% chance on hit to create Blaze Hyperspark
  - Knockback: 12 force for 0.15s

## Blaze Spark
- Asset: `Blaze B AD`
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 0.5s
- Gains on hit: Stamina +1 +1%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 6
  - Size: 1.5
  - Damage: 185% Phys
  - Scaling: EffAtk
  - Knockback: none

## Blaze Hyperspark
- Asset: `Blaze B1 AD`
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist (fixed)
- Animation: 0.5s
- Gains on hit: Stamina +2 +2%, Mana +1
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 8
  - Size: 2
  - Damage: 215% Phys, 12% True
  - Scaling: EffAtk
  - Knockback: none

## Blood Pact
- Asset: `Blood Pact AD`
- Type: Basic
- Cooldown: 1.8s
- Pattern: Single (1 count)
- Spawn: 0.5 dist (fixed)
- Animation: 0.5s
- Costs: Health 5 +3%
- Gains on hit: Stamina +2, Health +4 +1%, Mana +1
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 10
  - Size: 2.5
  - Damage: 35% Phys, 9% True
  - Scaling: EffMaxHp
  - Effect: 40% on hit (Bleed, 3s, 0.5s tick, max 5 stacks, 8% EffMaxHp per tick as DoT)
  - Knockback: 4 force for 0.15s

## Exodus
- Asset: `Exodus A AD`
- Unlocks: wave 25
- Type: Ultimate
- Cooldown: 90s
- Pattern: Single (1 count)
- Spawn: 5 dist
- Animation: 0.75s
- Costs: Stamina 40 +55%, Mana 40%
- Gains on hit: Stamina +1, Mana +1
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 2
  - Damage: 1365% Phys
  - Scaling: EffAtk
  - Additional: 60% chance on hit to create Exodus Wave
  - Knockback: 2 force for 0.15s

## Exodus Wave
- Asset: `Exodus B AD`
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist, 0.75s delay
- Animation: 0.75s
- Gains on hit: Stamina +2, Mana +2
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 3
  - Damage: 880% Spell
  - Scaling: EffInt
  - Additional: 40% chance on hit to create Exodus Core
  - Knockback: 1 force for 0.15s

## Exodus Core
- Asset: `Exodus C AD`
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist, 0.75s delay
- Animation: 0.75s
- Gains on hit: Stamina +5, Mana +5
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 6
  - Damage: 110% True
  - Scaling: critDamage
  - Knockback: none

## Ignition Flash
- Asset: `Ignition Flash AD`
- Type: Basic
- Cooldown: 2.8s
- Pattern: Single (1 count)
- Spawn: 0.65 dist
- Animation: 0.75s
- Gains on hit: Stamina +3, Mana +1%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 6
  - Size: 2.5
  - Damage: 285% Phys
  - Scaling: EffAtk
  - Effects: 100% on hit (Burn, 6s, 1s tick, max 5 stacks, 35% EffAtk per tick as DoT) +
    45% on hit (Vulnerable, 6s, max 3 stacks, -8% damageRes per stack)
  - Knockback: 5 force for 0.15s

## Lifeforce
- Asset: `Lifeforce AD`
- Type: Skill
- Cooldown: 11s
- Pattern: Single (1 count)
- Spawn: 1 dist
- Animation: 0.75s
- Costs: Health 30 +40%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 5
  - Size: 2.25
  - Damage: 90% Spell
  - Scaling: EffMaxHp
  - Special: 0.5x multiplier scaling on HpConsumed
  - Additional: 100% chance on hit to create Lifeforce Shard (follows mouse)
  - Knockback: 6 force for 0.15s

## Lifeforce Shard
- Asset: `Lifeforce AD 1`
- Type: Basic
- Cooldown: 0s (follow-up)
- Pattern: Spread (3 count, 10 spread)
- Spawn: 0.75 dist, 0.25s delay
- Animation: 0.75s
- Gains on hit: Stamina +2, Mana +2
- Projectile:
  - Speed: 4
  - Lifetime: 6s
  - Pierce: 3000
  - Size: 1.25
  - Damage: 85% Phys
  - Scaling: EffMaxHp
  - Time Before Same Enemy: 0.5s
  - Additional: 35% chance on hit to create Lifeforce Burst
  - Knockback: 5 force for 0.15s

## Lifeforce Burst
- Asset: `Lifeforce AD 2`
- Type: Basic
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist, 1s delay
- Animation: 0.75s
- Gains on hit: Stamina +4, Health +5 +3%, Mana +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 3
  - Damage: 165% Spell
  - Scaling: EffMaxHp
  - Knockback: 8 force for 0.15s

## Luminaria
- Asset: `Luminaria AD`
- Unlocks: wave 35
- Type: Ultimate
- Cooldown: 18s
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 1s
- Costs: Stamina 15, Health 10%, Mana 60
- Gains on hit: Stamina +2, Mana +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 3
  - Damage: 270% True
  - Scaling: EffHpReg
  - Use True Angle
  - Effects: 100% self on cast (Holy Bounty, 24s: +80% addDmgPct, +30% resPen,
    +15% damageRes) + 40% on cast (Stun, 2s)
  - Knockback: 8 force for 0.3s

## Meteor Shower
- Asset: `Meteor Shower AD`
- Type: Skill
- Cooldown: 5s
- Pattern: Barrage (48 count +28 random, 3 radius)
- Spawn: 5 dist
- Animation: 0.5s
- Costs: Stamina 15, Mana 48
- Gains on hit: Stamina +2, Mana +5
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 4
  - Size: 1.25
  - Damage: 80% Spell
  - Scaling: EffInt
  - Use True Angle
  - Delay: 0.05-0.12s between projectiles
  - Knockback: none

## Nebula
- Asset: `Nebula AD`
- Type: Skill
- Cooldown: 2s
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0.5s
- Costs: Stamina 14 +8%, Mana 12
- Gains on hit: Stamina +4, Mana +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 12
  - Size: 3.5
  - Damage: 440% Phys
  - Scaling: EffAtk
  - Effect: 100% on hit (Radiation, 4s, 0.25s tick, max 8 stacks, 3% critDamage per tick
    as DoT)
  - Knockback: 4 force for 0.15s

## Nirvana
- Asset: `Nirvana A AD`
- Unlocks: wave 35
- Type: Ultimate
- Cooldown: 24s
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0.5s
- Costs: Mana 145 +10%
- Gains on hit: Mana +4 +1%
- Fires Orbits
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 3000
  - Size: 4
  - Damage: 550% Spell
  - Scaling: EffInt
  - Special: 0.2x per orbit (specialScaling Orbits)
  - Knockback: 12 force for 0.15s

## Nocturnis
- Asset: `Nocturnis T AD` / `Nocturnis T PD`
- Unlocks: wave 45 (wave 25 in `corruptionSpecialPool`)
- Type: Ultimate
- Cooldown: 14s (stamped on press, ticks down during the hold)
- Pattern: Single (1 count)
- Spawn: 0.7 dist
- Animation: 1s
- Costs: Health 35 +12%
- Gains on hit: Health +2 +1%
- Charging: hold 0.225s to charge, min 1s, max 6s, drains every 1s, charge attack `Nocturnis C AD`
- Projectile (tap):
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2
  - Damage: 60% Phys, 110% Spell, 14% True
  - Scaling: EffMaxHp
  - Use True Angle
  - Knockback: 5 force for 0.15s

## Nocturnis (Held)
- Asset: `Nocturnis C AD` / `Nocturnis C PD`
- Type: Ultimate (charge variant, spawned only by holding Nocturnis)
- Cooldown: 0s (sustained)
- Pattern: Single (1 count)
- Spawn: 0 dist
- Costs: Health 8 +4% on confirm, then again every 1s tick
- Gains on hit: Health +2 +1%
- Projectile:
  - Speed: 0.6, FollowCursor movement
  - Lifetime: 1.1s, refreshed by every charge tick
  - Pierce: 3000
  - Size: 2
  - Damage: 15% Phys, 40% Spell, 4% True
  - Scaling: EffMaxHp
  - Special: 2x multiplier scaling on HpConsumed
  - Time Before Same Enemy: 0.5s
  - Use True Angle
  - Knockback: none

## Revelation
- Asset: `Revelation AD`
- Unlocks: wave 35
- Type: Ultimate
- Cooldown: 16s
- Pattern: Single (1 count)
- Spawn: 4 dist, 0.25s delay
- Animation: 0.5s
- Costs: Stamina 70, Mana 50
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 5
  - Damage: 340% Spell
  - Scaling: EffInt
  - Effect: 100% on hit (Detonator, 0.5s, detonates DoTs for 250% as True damage)
  - Knockback: 6 force for 0.15s

## Shattered Singularity
- Asset: `Shattered Singularity A AD`
- Unlocks: wave 25
- Type: Ultimate
- Cooldown: 12s
- Pattern: Single (1 count)
- Spawn: 0.5 dist
- Animation: 0.5s
- Costs: Stamina 60, Mana 60
- Projectile:
  - Speed: 0.5
  - Lifetime: 4.5s
  - Pierce: 1
  - Size: 1.5
  - Damage: 60% Spell
  - Scaling: EffInt
  - Additional: 100% chance on hit to create Singularity Fragment
  - Knockback: none

## Singularity Fragment
- Asset: `Shattered Singularity B AD`
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Circle (6 count)
- Spawn: 1.25 dist (fixed), 0.25s delay
- Animation: 0s
- Gains on hit: Stamina +5%, Mana +5%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 8
  - Size: 2.5
  - Damage: 180% Phys, 660% Spell
  - Scaling: EffInt
  - Knockback: none

## Solar Collapse
- Asset: `Solar Collapse AD`
- Unlocks: wave 25
- Type: Ultimate
- Cooldown: 14s
- Pattern: Single (1 count)
- Spawn: 2 dist
- Animation: 0.5s
- Costs: Stamina 90%
- Gains on hit: Stamina +3%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 6s
  - Pierce: 3000
  - Size: 3
  - Damage: 340% Phys, 25% True
  - Scaling: EffAtk
  - Time Before Same Enemy: 0.5s
  - Effects: 30% on hit (Slow, 3s, max 8 stacks, -10% moveSpeed per stack) + 100% on hit
    (Pulled, 0.75s, pull speed 1.4 +2 per stack, 1.5 radius)
  - Knockback: none

## Starfury
- Asset: `Starfury AD`
- Unlocks: wave 25
- Type: Ultimate
- Cooldown: 6s
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0s
- Costs: Mana 24 +22%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 4
  - Damage: 120% Spell, 20% True
  - Scaling: moveSpeedPct
  - Time Before Same Enemy: 0.1s
  - Knockback: none

## Stellar Maelstrom
- Asset: `Stellar Maelstrom AD`
- Type: Skill
- Cooldown: 3s
- Pattern: Spread (14 count +10 random, 30 spread +/-15)
- Spawn: 0 dist
- Animation: 0.5s
- Costs: Stamina 40 +44%, Mana 18 +52%
- Gains on hit: Stamina +3, Mana +2
- Projectile:
  - Speed: 8
  - Lifetime: 1.25s
  - Pierce: 1
  - Size: 1.5
  - Damage: 180% Phys, 70% Spell
  - Scaling: EffAtk
  - Follow Distance: 2
  - Delay: 0.15-0.35s between projectiles
  - Knockback: 2 force for 0.15s

## Supernova
- Asset: `Supernova AD`
- Type: Skill
- Cooldown: 3s
- Pattern: Single (1 count)
- Spawn: 1 dist (fixed)
- Animation: 0.5s
- Costs: Stamina 20 +10%, Mana 5
- Gains on hit: Stamina +4, Health +5%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 9
  - Size: 4
  - Damage: 190% Phys, 30% True
  - Scaling: EffArmor
  - Effect: 40% on hit (Weaken, 5s, max 4 stacks, -10% attack per stack)
  - Knockback: 3 force for 0.15s

## Warp
- Asset: `Warp A AD`
- Type: Skill
- Cooldown: 9s
- Pattern: Circle (2 count +2 random)
- Spawn: 0 dist (fixed)
- Animation: 1s
- Costs: Stamina 15, Mana 50 +15%
- Gains on hit: Mana +3
- Projectile:
  - Speed: 0.8
  - Lifetime: 10s
  - Pierce: 3000
  - Size: 2
  - Damage: 60% Spell
  - Scaling: EffInt
  - Time Before Same Enemy: 1.5s
  - Orbit: radius 1.25, orbits self, CCW
  - Additional: 15% chance on hit to create Warp Rift
  - Knockback: 2 force for 0.15s

## Warp Rift
- Asset: `Warp B AD` (shared by both Warp versions)
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 1s
- Gains on hit: Mana +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1.5s
  - Pierce: 3000
  - Size: 1.25
  - Damage: 215% Spell
  - Scaling: EffInt
  - Time Before Same Enemy: 0.5s
  - Use True Angle
  - Effect: 100% on hit (Pulled, 0.6s, pull speed 5 +2 per stack, 1.5 radius)
  - Knockback: none

---

# Corruption Special Pool

Serialized in `WaveManager.corruptionSpecialPool` (identical on the regular and unlimited
managers). When the Corrupt button is pressed, every reward button that passes the
`corruptChance` roll then rolls `corruptionSpecialChance` (8%). On a hit the stat reward is
replaced outright by one of these attacks instead of receiving a value multiplier. The same
special cannot appear on two buttons in one corruption pass, and claiming one removes it from
the pool for the rest of the run. Full stats for each attack are in the sections above.

| Attack | Unlock wave here | Unlock wave in `rarePool` |
| --- | --- | --- |
| Shattered Singularity | 0 | 25 |
| Solar Collapse | 0 | 25 |
| Starfury | 0 | 25 |
| Exodus | 0 | 25 |
| Revelation | 15 | 35 |
| Nirvana | 15 | 35 |
| Luminaria | 15 | 35 |
| Nocturnis | 25 | 45 |

---

# Skill Tree Attacks

Folder: `Assets/data/PlayerData/Attacks/SkillTree`. Not in any reward pool; granted by
skill tree nodes.

## Warp (Capstone)
- Asset: `Warp AA AD`
- Type: Skill
- Cooldown: 9s
- Pattern: Circle (3 count +3 random)
- Spawn: 0 dist (fixed)
- Animation: 1s
- Costs: Stamina 30, Mana 55 +20%
- Gains on hit: Stamina +1, Mana +2 +2%
- Projectile:
  - Speed: 1.4
  - Lifetime: 10s
  - Pierce: 3000
  - Size: 3
  - Damage: 60% Spell
  - Scaling: EffInt
  - Time Before Same Enemy: 1.5s
  - Orbit: radius 1.25, orbits self, CCW
  - Additional: 25% chance on hit to create Warp Rift
  - Knockback: 2 force for 0.15s
- Unlocked by: `Node_warp` ("Warp" capstone, 3 skill points, prerequisite `Node_mm3`,
  requires the base Warp attack)

## Decoy Burst
- Asset: `Decoy AD`
- Type: Additional
- Cooldown: 0s (spawned on decoy expiry)
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 1s
- Gains on hit: Stamina +3, Mana +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2.5
  - Damage: 225% Spell
  - Scaling: EffAtk
  - Effect: 100% on hit (Vulnerable, 4s, -30% damageRes)
  - Knockback: 5 force for 0.15s
- Used by: the `Decoy Upgraded` player upgrade, granted by `Node_decoy`
  ("Cosmic Superimposition" capstone)

---

# Treasure Pool Attacks

Folder: `Assets/data/PlayerData/Attacks/Treasure Pool`. These are projectiles fired by
player upgrades rather than attacks the player selects.

## Autopilot
- Asset: `Autopilot AD`
- Type: Additional
- Cooldown: 0s (upgrade-spawned)
- Pattern: Circle (3 count)
- Spawn: 0 dist
- Animation: 1s
- Gains on hit: Stamina +2, Health +3%
- Projectile:
  - Speed: 6
  - Lifetime: 6s
  - Pierce: 1 (destroys on max pierce)
  - Size: 2
  - Damage: 335% Phys
  - Scaling: EffArmor
  - Movement: Spiral (spacing 2)
  - Homing: 0.5 follow distance
  - Knockback: 8 force for 0.15s

## Feedback Loop
- Asset: `Feedback Loop AD`
- Type: Additional
- Cooldown: 0s (upgrade-spawned)
- Pattern: Circle (6 count)
- Spawn: 0.75 dist (fixed)
- Animation: 0.5s
- Gains on hit: Mana +1%
- Projectile:
  - Speed: 16
  - Lifetime: 1.5s
  - Pierce: 4
  - Size: 2
  - Damage: 15% Spell, 4% True
  - Scaling: EffInt
  - Knockback: none

## Soul Rend
- Asset: `Shattered Vessel A AD`
- Type: Additional
- Cooldown: 0s (upgrade-spawned)
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0.5s
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 12
  - Size: 3
  - Damage: 580% Phys, 60% Spell
  - Scaling: EffAtk
  - Additional: 100% chance on hit to create Soul Fragment
  - Knockback: none

## Soul Fragment
- Asset: `Shattered Vessel B AD`
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Circle (8 count)
- Spawn: 0.1 dist (fixed), 0.1s delay
- Animation: 0.5s
- Projectile:
  - Speed: 18
  - Lifetime: 0.5s
  - Pierce: 4
  - Size: 1.5
  - Damage: 240% Phys, 5% True
  - Scaling: EffAtk
  - Knockback: none

## Supersonic
- Asset: `Supersonic AD`
- Type: Additional
- Cooldown: 0s (upgrade-spawned)
- Pattern: Circle (3 count, 45 random spread)
- Spawn: 0 dist
- Animation: 1s
- Gains on hit: Stamina +3, Mana +3
- Projectile:
  - Speed: 9
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 1.5
  - Damage: 110% True
  - Scaling: moveSpeedPct
  - Effects: 60% on hit (Slow, 5s, max 3 stacks, -15% moveSpeed per stack) + 100% self on cast (Supersonic Cooldown, 3s)
  - Knockback: 4 force for 0.15s

---

# Player Upgrades

The `GrantStatusEffect` type (`PlayerUpgrade/GrantStatusEffect`) applies an authored
`StatusEffect` to the player for `stacks` stacks under any trigger condition, and removes it
again on `OnRemove`. Used by `Solar Wind`.

Folder: `Assets/data/PlayerData/PlayerUpgrade`. All except `Decoy Upgraded` are present
in `WaveManager.treasurePool`. Entries marked with an unlock wave carry a `minWave` on
their `PlayerUpgradeReward` and cannot be rolled before that wave.

## Hypercarry
- Asset: `DashAdvance`
- Unlocks: wave 35
- Type: CooldownAdvance
- Conditions: OnStartDash
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Amount: 12
- Advance Type: All
- Description: Dashing advances all cooldowns by 12.

## Autopilot
- Asset: `Autopilot`
- Type: SpawnProjectile
- Conditions: OnTakeHit
- Chance: 100%
- Cooldown: 2s
- Delay: 0.25s
- Projectile: `Autopilot.prefab`
- Description: Taking a direct hit spawns 3 spiraling projectiles that home in on nearby
  enemies and return stamina and health on hit.

## Decoy
- Asset: `Decoy`
- Type: Decoy
- Conditions: OnStartDash
- Chance: 100%
- Cooldown: 6s
- Delay: 0s
- Lifetime: 4s
- Spawn Offset: (0, 0, 0)
- Tint: White (61% alpha)
- Cooldown Effect: Cosmic Afterimage (6s)
- Projectile: None (base version does not detonate)
- Description: Dashing spawns a decoy that taunts enemies within their detection range
  for 4 seconds.

## Decoy Upgraded (Capstone)
- Asset: `Decoy Upgraded`
- Type: Decoy
- Conditions: OnStartDash
- Chance: 100%
- Cooldown: 5s
- Delay: 0s
- Lifetime: 6s
- Spawn Offset: (0, 0, 0)
- Tint: White (78% alpha)
- Cooldown Effect: Cosmic Afterimage (6s)
- Projectile: `Decoy.prefab` (Decoy Burst)
- Description: Dashing spawns a decoy that taunts for 6 seconds, then detonates at its
  own position for 225% Spell damage and applies Vulnerable.
- Unlocked by: `Node_decoy` ("Cosmic Superimposition", 3 skill points, prerequisite
  `Node_ms2`, requires the base Decoy upgrade, which it consumes on unlock and returns
  on refund)

## Feedback Loop
- Asset: `FeedbackLoop`
- Type: SpawnProjectile
- Conditions: OnProjectileHit
- Chance: 70%
- Cooldown: 0.3s
- Delay: 0s
- Projectile: `Feedback Loop.prefab`
- Description: 70% chance on projectile hit to spawn a ring of 6 Feedback Loop
  projectiles, at most once every 0.3s.

## Hex Cast
- Asset: `HexCast`
- Type: HexCast
- Conditions: none
- Chance: 0%
- Cooldown: 0s
- Delay: 0s
- Description: Marker upgrade with no trigger logic of its own; allows Health to replace
  Stamina for attack costs.

## Paradox
- Asset: `Paradox`
- Unlocks: wave 35
- Type: Paradox
- Conditions: none
- Chance: 0%
- Cooldown: 0s
- Delay: 0s
- Description: On unlock, grants globalDoTCanCrit; removed on unequip. Allows global DoTs
  to crit.

## Reminiscence
- Asset: `Reminiscence`
- Unlocks: wave 35
- Type: Reminiscence
- Conditions: OnCrit
- Chance: 25%
- Cooldown: 4s
- Delay: 0.35s
- Cooldown Effect: Reminiscence Cooldown (4s)
- Description: 25% chance on a critical hit to immediately perform an extra attack of a
  randomly chosen equipped attack type.

## Serenade
- Asset: `Serenade`
- Unlocks: wave 35
- Type: AdditionalDamage
- Conditions: OnDealDamage
- Chance: 35%
- Cooldown: 0s
- Delay: 0s
- Percent Amount: 24%
- Damage Type: True
- Description: 35% chance to deal 24% of the damage dealt again as True damage.

## Solar Wind (Capstone)
- Asset: `SolarWind`
- Type: GrantStatusEffect
- Conditions: OnHealthRegen
- Chance: 30%
- Cooldown: 3s
- Delay: 0s
- Effect: `Solar Wind` (6s, 6 stacks), 1 stack per trigger
- Description: 30% chance on each health regen tick to gain a stack of Solar Wind, at
  most once every 3s.
- Unlocked by: `Node_solarwind` ("Solar Wind" capstone, 3 skill points, prerequisite
  `Node_hprp5`, requires the Stellar Surge Awakening). Unlocking it consumes Stellar
  Surge — the Awakening is removed as Solar Wind is granted, so the health regen tick
  rolls for Solar Wind instead of the heal. Refunding the node returns Stellar Surge.
- Not in `treasurePool` — capstone-only.

## Soul Rend
- Asset: `SoulRendPU`
- Unlocks: wave 35
- Type: SoulRendPU
- Conditions: OnUltAttack
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Projectile: `Shattered Vessel A.prefab`
- Soul Rend Effect: `Soul Rend` status effect
- Description: On unlock, attaches the Soul Rend stacking buff to the equipped Basic and
  Skill attacks. Using an Ultimate at 50 or more stacks fires the Shattered Vessel
  projectile and then clears the stacks after 0.3s.

Soul Rend buff (1.5s duration, max 100 stacks):
- +0.3% atkPct per stack
- +2 defShred per 5 stacks
- +1% resPen per 10 stacks
- +4% physicalDmgPct per 20 stacks
- +5% critDamage per 25 stacks
- +100% UltDmgPct per 50 stacks

## Starlit Reflexes
- Asset: `Starlit Reflexes`
- Type: GainMana
- Conditions: OnCounterDodge
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Flat Amount: 18
- Description: Gain 18 flat mana when dashing into a projectile.

## Stellar Surge
- Asset: `StellarSurge`
- Type: StellarSurge
- Conditions: OnHealthRegen
- Chance: 20%
- Cooldown: 0s
- Delay: 0s
- HP Percent: 6%
- Description: 20% chance on each health regen tick to additionally heal for 6% of
  EffMaxHp.
- Note: the asset also serializes `bypassMaxPct`, which `TriggerUpgradeEffect` never reads.

## Supersonic
- Asset: `Supersonic`
- Type: SpawnProjectile
- Conditions: OnEndDash
- Chance: 100%
- Cooldown: 1s
- Delay: 0s
- Projectile: `Supersonic.prefab`
- Description: Ending a dash spawns 3 Supersonic projectiles, at most once per second.

---

# Status Effects

Folder: `Assets/data/StatusEffect`.

| Asset | Class | Name | Duration | Tick | Max stacks | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| `Blaze Soul` | AttackReplacement | Blaze Soul | 6s | - | 1 | Replaces the attack with `Blaze A1 AD` (Cosmic Blaze) |
| `Bleed 5 1 3 30 EffAtk` | DoT | Bleed | 3s | 0.5s | 5 | 8% EffMaxHp per tick |
| `Burn 6 1 5 15` | DoT | Burn | 6s | 1s | 5 | 35% EffAtk per tick |
| `Cosmic Afterimage` | Info | Cosmic Afterimage Cooldown | 6s | - | 1 | Cooldown marker |
| `Crumbling 6 10 4` | StatReduction | Crumbling | 6s | - | 4 | -10% armor per stack |
| `DotDetonator 0.5 2` | Detonator | (unnamed) | 0.5s | - | 1 | Detonates DoTs for 250% as True |
| `Freeze 2` | Stun | Frozen | 2s | - | 1 | Cannot move or attack |
| `Heartburn` | StatBuffs | Heartburn | 6s | - | 15 | +4% damagePct, +12% critDamage, +18% stCostPct, -16% hpRegPct per stack |
| `Holy Bounty` | StatBuffs | Holy Bounty | 24s | - | 1 | +80% addDmgPct, +30% resPen, +15% damageRes |
| `Overheat` | StatBuffs | Overheat | 7s | - | 5 | -8% atkPct, -12% stRegPct per stack |
| `Poison 2 0.5 1 20 Atk` | DoT | Poison | 2s | 0.5s | 1 | 20% EffAtk per tick |
| `Pulled 0.6 1 1.5 5 0.1` | Pulled | Possessed | 0.6s | 0.016s | 1 | Pull speed 5 (+2/stack), 1.5 radius |
| `Pulled 0.75 1 1.4 2 1.5` | Pulled | Possessed | 0.75s | 0.016s | 1 | Pull speed 1.4 (+2/stack), 1.5 radius |
| `Radiation 4 0.25 8 2 CritDmg` | DoT | Radiation | 4s | 0.25s | 8 | 3% critDamage per tick |
| `Reminiscence Cooldown` | Info | Reminiscence Cooldown | 4s | - | 1 | Cooldown marker |
| `Slow 3 8 10` | StatReduction | Slow | 3s | - | 8 | -10% moveSpeed per stack |
| `Slow 5 3 15` | StatReduction | Slow | 5s | - | 3 | -15% moveSpeed per stack |
| `Slow 6 15 5` | StatReduction | Slow | 6s | - | 15 | -5% moveSpeed per stack |
| `Slow 8 2 30` | StatReduction | Slow | 8s | - | 2 | -30% moveSpeed per stack |
| `Solar Wind` | StatBuffs | Solar Wind | 6s | - | 6 | +3 hpRegen, +8% hpRegPct, +6% moveSpeedPct per stack; all stacks drop on expiry |
| `Soul Rend` | SoulRend | Soul Rend | 1.5s | - | 100 | See the Soul Rend upgrade above |
| `Stun 2` | Stun | Stun | 2s | - | 1 | Cannot move or attack |
| `Stun 6` | Stun | Stun | 6s | - | 1 | Cannot move or attack |
| `Supersonic Cooldown` | Info | Supersonic Cooldown | 3s | - | 1 | Cooldown marker |
| `Vulnerable 6 30` | StatBuffs | Vulnerable | 6s | - | 1 | -30% damageRes |
| `Vulnerable 6 3 8` | StatBuffs | Vulnerable | 6s | - | 3 | -8% damageRes per stack |
| `Vulnerable 6 6 5` | StatReduction | Afflicted | 6s | - | 6 | -5% maxHp per stack |
| `Vulnerable 8 2 20` | StatBuffs | Vulnerable | 8s | - | 2 | -20% damageRes per stack |
| `Weaken 5 10 4` | StatReduction | Weaken | 5s | - | 4 | -10% attack per stack |

Used by enemies rather than the player: `Crumbling 6 10 4` (Crab), `Poison 2 0.5 1 20 Atk`
(Slime), `Slow 8 2 30` (Lich), `Stun 6` (Cultist), `Vulnerable 6 6 5` (Bat Mark),
`Stun 2` (also used by BallSpam), `Slow 6 15 5` and `Freeze 2` (Slime (Frost)'s Blizzard),
`Overheat` (Slime (Magma)'s Eruption).

`Slow 5 3 15` is authored but no longer referenced by any projectile — Blizzard moved to
`Slow 6 15 5` in v0.3.9.
