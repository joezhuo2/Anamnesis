# Game Data Reference

Synced against the assets in `Assets/data/PlayerData/`, `Assets/data/Collectibles/` and
the `WaveManager` reward pools serialized in `Assets/New.unity`. Damage multipliers are shown as percentages
(asset value x 100). Scaling stats use the exact `StatType` enum name.

Attack entries list the `AttackData` asset name; the paired `ProjectileData` asset is
the same name with `PD` instead of `AD` unless noted.

Projectile `Size` multiplies the projectile prefab's own scale: the final scale is
`prefab scale x Size x (1 + aoePct%)`. `aoePct` is capped at 200% when read, so the multiplier never exceeds x3.

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
  - Size: 1.5
  - Damage: 180% Phys
  - Scaling: EffAtk
  - Rotation: 270 degrees
  - Knockback: 3 force for 0.15s

## Cyclone Cleave
- Asset: `p_s_ad` / `p_s_pd`
- Type: Skill
- Cooldown: 5s
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
  - Damage: 545% Phys, 85% True
  - Scaling: EffAtk
  - Knockback: 5 force for 0.15s

## Coherent Strike
- Asset: `Coherent Strike AD` / `Coherent Strike PD`
- Unlocks: run start, as the Master mode `startingUlt` (see [Modes](#modes)). Not in any reward pool
- Type: Ultimate
- Cooldown: 20s (starts on cast)
- Pattern: Single (1 count)
- Spawn: 1 dist
- Costs: Stamina 35 +25%, Mana 20 +15%
- Gains on hit (based on damage dealt): Stamina +2, Mana +1
- Hit stop 0.06s (0.5s cooldown), screen shake 0.08
- Projectile:
  - Speed: 12
  - Lifetime: 1s
  - Pierce: 18
  - Size: 3
  - Damage: 350% Phys, 260% Spell
  - Scaling: EffAtk, plus 50% EffInt
  - Rotation: 45 degrees
  - Knockback: 5 force for 0.15s

---

# Rare Pool

Folder: `Assets/data/PlayerData/Attacks/Rare Pool`. All 22 entries below are present in
`WaveManager.rarePool`. Entries marked with an unlock wave carry a `minWave` on their
`AttackReward` and cannot be rolled before that wave; the rest are available from wave 1.
Seven of them also sit in `corruptionSpecialPool` at a much lower unlock wave — see
[Corruption Special Pool](#corruption-special-pool).

## Aeternus
- Asset: `Aeternus AD`
- Unlocks: wave 25
- Type: Ultimate
- Cooldown: 28s (starts on cast)
- Pattern: Single (1 count)
- Cast: 1s, can move while casting
- Costs: Stamina 80, Mana 80
- Gains on hit (based on damage dealt): Stamina +1 +0.5%, Mana +1 +0.5%
- Cleanses: 1 debuff
- Projectile:
  - Speed: 1.1
  - Lifetime: 56s
  - Pierce: 3000
  - Size: 2
  - Damage: 10% True
  - Scaling: critDamage, plus 30% EffAtk, 30% EffInt, 30% EffArmor, 30% EffMaxHp
  - Time Before Same Enemy: 0.25s
  - Knockback: 0.15s

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
  - Damage: 55% Spell
  - Scaling: EffInt
  - Time Before Same Enemy: 0.5s
  - Orbit: radius 1 (+1 random), orbits self, CCW
  - Knockback: none

## Astral Nova
- Asset: `Astral Nova AD`
- Type: Basic
- Cooldown: 3s
- Pattern: Single (1 count)
- Spawn: 5 dist, 1.5s delay
- Animation: 0.5s
- Gains on hit: Stamina +15%, Mana +15%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 6
  - Size: 2
  - Damage: 180% Spell, 30% True
  - Scaling: EffInt
  - Effect: 100% on hit (Vulnerable, 8s, max 2 stacks, -20% damageRes per stack)
  - Knockback: none

## Blaze
- Asset: `Blaze A AD`
- Type: Basic
- Cooldown: 26s
- Pattern: Single (1 count)
- Spawn: 0.5 dist
- Animation: 1s
- Gains on hit: Stamina +6, Mana +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2.5
  - Damage: 930% Phys
  - Scaling: EffAtk
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
- Cast: 1s, rooted while casting
- Costs: Stamina 18 +8%
- Gains on hit: Stamina +3
- Projectile:
  - Speed: 12
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2
  - Damage: 600% Phys
  - Scaling: EffAtk
  - Effects: 100% self on cast (Blaze Soul, refreshes the 6s replacement) + 100% self on
    cast (Heartburn, 6s, max 15 stacks: +4% damagePct, +12% critDamage, +18% stCostPct,
    -16% hpRegPct per stack)
  - Additional: 60% chance on hit to create Blaze Hyperspark
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
  - Damage: 255% Phys
  - Scaling: EffAtk
  - Knockback: none

## Blaze Hyperspark
- Asset: `Blaze B1 AD`
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist (fixed)
- Animation: 0.5s
- Gains on hit: Stamina +3, Mana +2
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 8
  - Size: 2
  - Damage: 310% Phys, 18% True
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
- Gains on hit: Stamina +2, Health +5 +2%, Mana +1
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 10
  - Size: 2.5
  - Damage: 35% Phys, 9% True
  - Scaling: EffMaxHp
  - Effect: 70% on hit (Bleed, 3s, 0.5s tick, max 5 stacks, 8% EffMaxHp per tick as DoT)
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
- Cooldown: 2.2s
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
- Spawn: 2 dist
- Animation: 0.75s
- Costs: Health 10 +15%
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
- Type: Additional
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
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist, 1s delay
- Animation: 0.75s
- Gains on hit: Stamina +4, Health +5 +3%, Mana +2
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 3
  - Damage: 180% Spell, 15% True
  - Scaling: EffMaxHp
  - Knockback: 6 force for 0.15s

## Luminaria
- Asset: `Luminaria AD`
- Unlocks: wave 35
- Type: Ultimate
- Cooldown: 18s
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 1s
- Costs: Health 35 +25%, Mana 30
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
- Pattern: Barrage (48 count +32 random, 3 radius)
- Spawn: 5 dist
- Animation: 0.5s
- Costs: Stamina 15, Mana 48
- Gains on hit: Stamina +2, Mana +5
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 4
  - Size: 1
  - Damage: 100% Spell
  - Scaling: EffInt
  - Use True Angle
  - Delay: 0.06-0.14s between projectiles
  - Knockback: none

## Nebula
- Asset: `Nebula AD`
- Type: Skill
- Cooldown: 2.5s
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
  - Damage: 320% Phys
  - Scaling: EffAtk
  - Effect: 100% on hit (Radiation, 5s, 0.25s tick, max 10 stacks, 5% critDamage per tick
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
- Costs: Mana 85
- Gains on hit: Mana +4 +1%
- Fires Orbits
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 3000
  - Size: 4
  - Damage: 650% Spell
  - Scaling: EffInt
  - Special: 0.15x per orbit (specialScaling Orbits)
  - Knockback: 12 force for 0.15s

## Nocturnis
- Asset: `Nocturnis T AD` / `Nocturnis T PD`
- Unlocks: wave 25 (two `rarePool` entries, wave 25 and wave 45; wave 25 in `corruptionSpecialPool`)
- Type: Ultimate
- Cooldown: 14s (stamped on press, ticks down during the hold)
- Pattern: Single (1 count)
- Spawn: 0.7 dist
- Animation: 1s
- Costs: Health 35 +12%
- Gains on hit: Health +4 +3%
- Charging: hold 0.225s to charge, min 1s, max 6s, drains every 1s, charge attack `Nocturnis C AD`
- Projectile (tap):
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2
  - Damage: 85% Phys, 130% Spell, 22% True
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
  - Speed: 0.8, FollowCursor movement
  - Lifetime: 1.1s, refreshed by every charge tick
  - Pierce: 3000
  - Size: 2
  - Damage: 15% Phys, 40% Spell, 4% True
  - Scaling: EffMaxHp
  - Special: 2x multiplier scaling on HpConsumed
  - Time Before Same Enemy: 0.33s
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

## Sacred Surge
- Asset: `Sacred Surge AD`
- Type: Skill
- Cooldown: 16s (stamped on press)
- Pattern: Single (1 count)
- Spawn: 4 dist
- Animation: 1s
- Cast: 1s, can move while casting
- Costs: Stamina 8 +4%
- Gains on hit: Stamina +2, Health +1 +2%, Mana +1
- Cleanses the caster's debuffs on cast
- Charging: hold 0.225s to charge, min 1s, max 8s, ticks every 0.9s, no separate charge attack — the tick refreshes the projectile instead
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s, refreshed by every charge tick
  - Pierce: 3000
  - Size: 2
  - Damage: 290% Phys, 140% Spell
  - Scaling: EffArmor
  - Time Before Same Enemy: 0.8s
  - Use True Angle
  - Knockback: 4 force for 0.15s

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
  - Damage: 90% Spell, 15% True
  - Scaling: moveSpeedPct + 60% EffInt
  - Time Before Same Enemy: 0.1s
  - Knockback: none

## Stellar Maelstrom
- Asset: `Stellar Maelstrom AD`
- Type: Skill
- Cooldown: 3s
- Pattern: Spread (14 count +10 random, 30 spread +/-15)
- Spawn: 0 dist
- Animation: 0.5s
- Costs: Stamina 30 +24%, Mana 14 +42%
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

## Subspace Blitz
- Asset: `Subspace Blitz AD`
- Type: Skill
- Cooldown: 1.3s
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 0.5s
- Costs: Stamina 10, Mana 7
- Gains on hit: Stamina +2, Mana +1
- Rush: toward the cursor for 0.25s at 6x move speed. Attacks pressed mid-rush are queued
  until it ends, and the rush stops on collision.
- Rush impact: an enemy the rush collides with takes 65% of the projectile's damage and
  8 force knockback for 0.15s, resolved before the rush ends. Both scale with `rushImpactPct`.
- Projectile:
  - Speed: 9
  - Lifetime: 0.5s
  - Pierce: 6
  - Size: 2
  - Damage: 235% Phys, 18% True
  - Scaling: EffAtk
  - Rotation Offset: -45
  - Effect: 45% on hit (Freeze, 2s, cannot move, attack, dash or regen health)
  - Knockback: 1 force for 0.15s

## Supernova
- Asset: `Supernova AD`
- Type: Basic
- Cooldown: 3s
- Pattern: Single (1 count)
- Spawn: 1 dist (fixed)
- Animation: 0.5s
- Costs: none
- Gains on hit: Stamina +3, Health +3%, Mana +2
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 9
  - Size: 4
  - Damage: 160% Phys, 20% True
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
- Costs: Stamina 15, Mana 40 +15%
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
  - Size: 1.5
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
`corruptChance` roll then rolls `corruptionSpecialChance` (4%). On a hit the stat reward is
replaced outright by one of these attacks instead of receiving a value multiplier. The same
special cannot appear on two buttons in one corruption pass, and claiming one removes it from
the pool for the rest of the run. A [locked](#reward-lock) reward is skipped by corruption. Full
stats for each attack are in the sections above.

Corruption specials only roll when the run's mode has `allowCorruptionSpecials` on, and the
Corrupt button only appears when it has `allowCorruption` on (see [Modes](#modes)). Ultimates
in this pool are skipped when the mode locks Ultimates, as are attacks whose `minMode` is above
the mode's tier.

---

# Modes

Each run has a mode, picked on the home screen by `ModeSelector` (one button, each click cycles
Simple → Expert → Master and swaps the button sprite). The choice persists to `settings.json`
(`modeIndex`) and locks in when a gamemode button is pressed. Modes are `ModeData` assets and
are independent of difficulty.

| Field | Effect |
|---|---|
| `tier` | 0 Simple, 1 Expert, 2 Master. Gates `AttackData.minMode` (enemy attacks, Rare Pool and corruption special rewards), `PlayerUpgrade.minMode` (Treasure Pool Awakenings), `SkillNodeDef.minMode` (nodes below the tier can't be unlocked or queued: "Requires Expert mode"), `AnomalyData.minMode` (anomaly offers) and `CollectibleData.minMode` (pickups and spawner box drops) |
| `allowCorruption` | Shows the Corrupt button |
| `allowCorruptionSpecials` | Lets corruption roll [corruption specials](#corruption-special-pool) |
| `unlockUltimates` | Off: the player's Ultimate is removed, no Ultimate can be granted by rewards or skill nodes, and the Ultimate button stays on the cooldown bar greyed out like a Sealed slot ("Locked in this mode") for the whole run |
| `startingUlt` | Equipped at run start when `unlockUltimates` is on |
| `lockAnomalySkip` | Hides and blocks Skip on the anomaly panel and the contract panel (see [Anomaly Reroll & Skip](#anomaly-reroll--skip)) |

Without a mode asset the run behaves as before: corruption, specials and Ultimates all on, at tier 0.

Shipped assets (`Assets/data/Mode`), in selector order:

| Mode | Tier | Corruption | Specials | Ultimates | Starting Ultimate |
|---|---|---|---|---|---|
| Simple | 0 | — | — | locked | — |
| Expert | 1 | yes | — | yes | — |
| Master | 2 | yes | yes | yes | [Coherent Strike](#coherent-strike) |

Only Master has `lockAnomalySkip` on.

The selector tooltip shows the asset's `description` only.

Content gated by tier (`minMode`):

| Tier | Content |
|---|---|
| Expert (1) | Capstone nodes; `Box Slime`, `Box Bat`, `Box Crab`, `Reroll` pickups; Blackout, Duel and Fission anomalies (Regular and Unlimited); enemy attacks Jellyfish SplashA, Lich B (Plant), Bat Mark, Crab B (Disc), Cultist Teleport, Cultist Circleballs |
| Master (2) | Ethereal Mirage keystone; `Box Cult`, `Box Doppelganger`, `SkillPoint` pickups; Unlimited Fission (`USplit`); Frost Slime Snowstorm; Golem TDLR Barrage; Lich Circle-In |

| Attack | Unlock wave here | Unlock wave in `rarePool` |
| --- | --- | --- |
| Shattered Singularity | 0 | 25 |
| Solar Collapse | 0 | 25 |
| Starfury | 0 | 25 |
| Exodus | 0 | 25 |
| Revelation | 15 | 35 |
| Nirvana | 15 | 35 |
| Luminaria | 15 | 35 |
| Nocturnis | 25 | 25 and 45 |

---

# Reward Lock

Each `RewardButton` has a `RewardLockButton` child that toggles a lock on that card
(`lockedIcon` / `unlockedIcon`). Rerolling keeps the locked card in its slot and only generates
the remaining choices.

- Only one card can be locked at a time. While one is locked, the other lock buttons are
  disabled; click the locked card's button again to unlock it and lock a different one.
- The lock is released after each reroll, so it has to be set again before the next one.
- A reroll rolls the choice count as usual, then subtracts one for the locked card.
- The reroll never re-offers the locked attack, Awakening, milestone or synergy.
- Corrupt skips the locked card, then hides every lock button.
- Hidden in Ironman Mode, on the anomaly panel, and when the panel has only one choice.
- The Corrupt button is also hidden when the run's mode doesn't allow corruption.

---

# Anomaly Reroll & Skip

`WaveManager.OpenAnomalyButtons` and `OpenRewardButtons` both go through `OpenActionButtons`,
which shows Reroll unless `RerollLocked` and Skip unless `SkipLocked`. The click handlers
check the same flags, so a hidden action can't fire.

| Rule | Source | Reroll | Skip |
|---|---|---|---|
| Ironman Mode | `IronmanSelector.Enabled` | hidden on every panel | allowed |
| Nightmare | `DifficultyData.lockAnomalyChoice` | hidden on the anomaly panel | hidden on the anomaly panel |
| Master | `ModeData.lockAnomalySkip` | allowed (unless Ironman) | hidden on the anomaly and contract panels |

The rules stack: Nightmare locks both no matter the mode or Ironman setting. The reroll count
text follows the Reroll button and is re-shown whenever rerolls become available again.
On the contract panel only Ironman (Reroll) and `lockAnomalySkip` (Skip) apply; `lockAnomalyChoice` does not.

---

# Contracts

A contract is a run-long anomaly picked before wave 1. Any `AnomalyData` with `isContract` can be
offered; the asset also stays in the normal anomaly pool.

**Offer.** `RegularWaveButtonController` and `UnlimitedWaveButtonController` call
`WaveManager.TryStartContract()` first, then `TryStartPreRunPicks()`, then `StartNextWave()`. The
panel (`RewardType.Contract`, `contractTitle`) shows `minContractCount`..`maxContractCount` cards
(plus `DifficultyData.minContractCountAdd` / `maxContractCountAdd`) built with `contractPrefab` /
`ContractButtonUI`, drawn with duplicates from every `isContract` asset with `minMode <= RunMode.Tier`.
`minWave` / `maxWave` are ignored. No eligible asset or no `contractPrefab` skips the panel. Picking or
skipping continues to the pre-run picks.

**Each wave.** `BeginWave` calls `ArmContract`: on a boss wave a contract with `disallowOnBossWave`
pauses (counts as held), otherwise `StartAnomaly()` re-arms it (fresh Time Trial timer, No Hit
subscription, Blackout vision, Sealed slot and cooldown buffs). While armed it runs `UpdateCheck`,
`ApplyEnemyBuffs`, `OnEnemySpawned`, and feeds `IsDuel` / `EnemyCountMult`, alongside any wave anomaly.
`EndWave` calls `DisarmContract` (`ResetForWave()`), keeping the instance and its rolled values for
the next wave.

**Payout.** Rolled in `RollAndAnnounceWaveRewards` when the contract held (`isActive` at wave end, or paused):

| Field | Effect |
|---|---|
| `contractRerolls` | Rerolls granted (0 on Ironman) |
| `contractSkillPointChance` | % chance for 1 skill point |
| `contractMixedPoolChance` | % chance for a bonus mixed reward pool |

A broken contract (Time Trial ran out, No Hit took damage) forfeits that wave's payout and adds
"Contract Broken" to the completion subtitle. After a wave the reward panels run contract bonus pool,
then the anomaly pool, then the standard reward.

**Exclusion.** While a contract is held, `HasAnomalyChoices` / `GenerateAnomalyChoices` drop every
anomaly whose `anomalyType` matches it. Skipping the contract excludes nothing.

**HUD.** `contractInfoText` shows `Contract: <name>`, with the Time Trial timer, `(Paused)` or `- Broken`.

---

# Damage Mitigation

`DamageCalculator.CalculateDamageTaken` multiplies incoming damage by each of these, then rolls dodge.
`DefenseMult(x)` is `100 / (x + 100)` for `x >= 0` and `2 - 100 / (100 - x)` below 0, so it is
continuous at 0 and negative values amplify damage up to 2x.

`resPen` and `defShred` are read from the snapshot the projectile captured when it spawned, not from the
attacker's live stats when it lands. Damage that is not built from a projectile (rush impacts, DoT
detonations) still reads live stats.

| Layer | Applies to | Value | Reduced by |
| --- | --- | --- | --- |
| Resistance | All types | `1 - (damageRes + physicalRes/spellRes - resPen) %`, floored at -100% | `resPen` |
| Armor | Physical | `DefenseMult(EffArmor - defShred)` | `defShred` |
| Arcane Shield | Spell | `DefenseMult(EffArcaneShield - defShred)` | `defShred` |
| Defense | All types | `DefenseMult(EffDefense)` | - |

`damageRes`, `physicalRes` and `spellRes` are capped at 90 when read. They are not in any reward pool or gear roll. They only come from
status effects, enemy presets and skill tree nodes. Enemies above level 1 multiply `armor` and
`arcaneShield` by `1.07^(level - 1)`.

---

# Stat Synergies

Serialized in `WaveManager.synergyStatPool` and the `synergy*` fields next to it (set separately
on the regular and unlimited managers; the settings match, the stat pools differ). A synergy converts a
percentage of a source stat into a flat bonus on a target stat. `StatSynergyManager` on the player
recomputes every synergy each frame from the current source value, so the bonus tracks the source
for the rest of the run.

Offer roll: after every wave at or past `synergyMinWave`, roll `synergyBaseChance` plus the
accumulated bonus. A miss adds `synergyChanceGrowth` to the bonus; a hit queues a synergy panel after
that wave's regular reward and resets the bonus. Synergies never appear in other pools or on corruption.

| Setting | Value |
| --- | --- |
| `synergyMinWave` | 15 |
| `synergyBaseChance` | 2% |
| `synergyChanceGrowth` | +2% per wave without an offer |
| `synergyChoices` | 3 |
| `minSynergyConversion` / `maxSynergyConversion` | 8% / 20% |

Regular manager:

| Stat (pool entry) | Read as source | Can be source | Can be target | Weight |
| --- | --- | --- | --- | --- |
| `attack` | `EffAtk` | Yes | Yes | 1 |
| `maxHp` | `EffMaxHp` | Yes | Yes | 1 |
| `armor` | `EffArmor` | Yes | Yes | 1 |
| `Intelligence` | `EffInt` | Yes | Yes | 1 |
| `maxMana` | `EffMaxMana` | Yes | Yes | 1 |
| `maxStamina` | `EffMaxStamina` | Yes | Yes | 1 |

Unlimited manager:

| Stat (pool entry) | Read as source | Can be source | Can be target | Weight |
| --- | --- | --- | --- | --- |
| `attack` | `EffAtk` | Yes | Yes | 6 |
| `maxHp` | `EffMaxHp` | Yes | Yes | 6 |
| `armor` | `EffArmor` | Yes | Yes | 6 |
| `Intelligence` | `EffInt` | Yes | Yes | 6 |
| `arcaneShield` | `EffArcaneShield` | Yes | Yes | 6 |
| `defense` | `EffDefense` | Yes | Yes | 5 |
| `critDamage` | `critDamage` | Yes | Yes | 4 |
| `hpRegen` | `EffHpReg` | Yes | Yes | 4 |
| `critChance` | - | No | Yes | 3 |
| `ProjSpd` | - | No | Yes | 2 |
| `defShred` | - | No | Yes | 1 |
| `resPen` | - | No | Yes | 1 |
| `EffectRes` | - | No | Yes | 1 |

Rules: source and target must be from different stat families; a pair already owned or already on
another card in the same roll is skipped; flat targets are floored to whole numbers.

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
- Costs: Stamina 15, Mana 30 +15%
- Gains on hit: Stamina +1, Mana +2 +2%
- Projectile:
  - Speed: 1.4
  - Lifetime: 10s
  - Pierce: 3000
  - Size: 3
  - Damage: 70% Spell
  - Scaling: EffInt
  - Time Before Same Enemy: 1.5s
  - Orbit: radius 1.25, orbits self, CCW
  - Additional: 20% chance on hit to create Warp Rift
  - Knockback: 2 force for 0.15s
- Unlocked by: `Node_warp` ("Warp" capstone, 3 skill points, prerequisite `Node_mm3`,
  requires the base Warp attack)

## Hypernova (Capstone)
- Asset: `Hypernova AD`
- Type: Basic
- Cooldown: 2.2s
- Pattern: Single (1 count)
- Spawn: 1 dist (fixed)
- Animation: 0.5s
- Costs: none
- Gains on hit: Stamina +2, Health +3 +1%, Mana +2
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 14
  - Size: 5
  - Damage: 190% Phys, 55% Spell
  - Scaling: EffArmor
  - Effects: 65% on hit (Weaken, 5s, max 4 stacks, -10% attack per stack) + 100% on hit
    (Possessed, 0.6s, pulls the target toward the projectile at speed 15) + 30% self on cast
    (Celestial Protection, 8s, max 4 stacks) + 20% on hit (Stun, 2s)
  - Knockback: none
- Unlocked by: `Node_hypernova` ("Hypernova" capstone, 3 skill points, prerequisite
  `Node_apdp3`, requires the base Supernova attack)

## Astral Disjunction (Capstone)
- Asset: `Astral Disjunction AD`
- Type: Basic
- Cooldown: 4s
- Pattern: Single (1 count)
- Spawn: 8 dist, 0.5s delay
- Animation: 0.5s
- Teleport: `teleportToProjectile` on, 0 extra delay — the player is moved to the
  projectile once the spawn delay elapses, which also fires the `OnTeleport` upgrade trigger
- Gains on hit: Stamina +5 +12%, Mana +5 +12%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 6
  - Size: 2.5
  - Damage: 220% Spell, 60% True
  - Scaling: EffInt
  - Effect: 100% on hit (Vulnerable, 8s, max 2 stacks, -20% damageRes per stack)
  - Knockback: none
- Unlocked by: `Node_astraldisjunction` ("Astral Disjunction" capstone, 3 skill points,
  `undoCost` 50, requires the base Astral Nova attack)

## Nitro Accelerator (Capstone)
- Asset: `Nitro Accelerator AD` (folder `Attacks/SkillTree/Nitro Accelerator`)
- Type: Skill
- Cooldown: 0.9s
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 0.5s
- Costs: Stamina 13, Mana 11
- Gains on hit: Stamina +2, Mana +1
- Rush: toward the cursor for 0.3s at 8x move speed, immune while rushing. Attacks pressed
  mid-rush are queued until it ends, and the rush stops on collision.
- Rush impact: an enemy the rush collides with takes 80% of the projectile's damage and
  3 force knockback for 0.15s, and Nitro Explosion spawns at the contact point. Both damage
  and knockback scale with `rushImpactPct`.
- Projectile:
  - Speed: 10
  - Lifetime: 0.5s
  - Pierce: 9
  - Size: 2.5
  - Damage: 245% Phys, 25% True
  - Scaling: EffAtk + 60% moveSpeedPct
  - Rotation Offset: -45
  - Effect: 70% on hit (Stun, 3s) + 100% self on cast (Decay, 4s, max 6 stacks)
  - Knockback: 1 force for 0.15s
- Unlocked by: `Node_nitroaccelerator` ("Nitro Accelerator" capstone, 3 skill points,
  `undoCost` 50, prerequisite `Node_ip4`, requires the base Subspace Blitz attack)

## Nitro Explosion
- Asset: `Nitro Explosion AD`
- Type: Additional
- Cooldown: 0s (spawned by Nitro Accelerator's rush impact via `impactAttack`)
- Pattern: Single (1 count)
- Spawn: 0 dist
- Animation: 0.5s
- Gains on hit: Stamina +2, Mana +2
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 3000
  - Size: 6
  - Damage: 115% Spell
  - Scaling: EffAtk + 35% EffInt
  - Effect: 60% on hit (Vulnerable, 6s, max 3 stacks, -8% damageRes per stack)
  - Knockback: 3 force for 0.15s

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
  - Size: 4
  - Damage: 225% Spell
  - Scaling: EffAtk
  - Effect: 100% on hit (Vulnerable, 4s, -30% damageRes)
  - Knockback: 5 force for 0.15s
- Used by: the `Decoy Upgraded` player upgrade, granted by `Node_decoy`
  ("Cosmic Superimposition" capstone)

## Ultrasonic (Capstone)
- Asset: `Ultrasonic AD`
- Type: Additional
- Cooldown: 0s (upgrade-spawned)
- Pattern: Circle (7 count, 45 random spread)
- Spawn: 0.25 dist (fixed)
- Animation: 1s
- Gains on hit: Stamina +3, Mana +3
- Projectile:
  - Speed: 14
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 1.5
  - Damage: 110% True
  - Scaling: moveSpeedPct + 30% EffAtk
  - Effect: 40% on hit (Stun, 2s)
  - Knockback: 5 force for 0.15s
- Used by: the `Ultrasonic` player upgrade, granted by `Node_ultrasonic`

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
  - Pierce: 3 (destroys on max pierce)
  - Size: 2
  - Damage: 335% Phys
  - Scaling: EffArmor
  - Movement: Spiral (spacing 2)
  - Homing: 0.5 follow distance
  - Knockback: 8 force for 0.15s

## Blood Bank
- Asset: `Blood Bank`
- Type: BloodBank
- Conditions: OnUltAttack
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Store: 50% of all health lost
- Effect: `Blood Pool` (permanent)
- Description: 50% of all health lost is stored in a Blood Pool (shown on the status effect
  tooltip as `Stored: N`). Casting an Ultimate releases everything stored as overhealth, which
  then decays at the default 25% per 0.5s unless an Overhealth upgrade sets its own rate.
- In `treasurePool`, no unlock wave.

## Chaos Theory
- Asset: `Chaos Theory AD`
- Type: Additional
- Cooldown: 0s (upgrade-spawned)
- Pattern: Single (1 count)
- Spawn: 0 dist, 0.15s delay
- Animation: 0s
- Gains on hit: Stamina +3, Mana +5
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 3.5
  - Damage: 260% Spell, 40% True
  - Scaling: EffInt
  - Effects: 100% on hit (Spellworn, 4s, max 2 stacks, -15% spellRes per stack), 40% on hit
    (Stun, 2s)
  - Knockback: 6 force for 0.15s

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
  - Damage: 25% Spell, 8% True
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
- Pattern: Circle (3 count)
- Spawn: 0 dist
- Animation: 1s
- Gains on hit: Stamina +3, Mana +3
- Projectile:
  - Speed: 9
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 1.5
  - Damage: 135% True
  - Scaling: moveSpeedPct
  - Effects: 100% self on cast (Supersonic Cooldown, 3s)
  - Knockback: 4 force for 0.15s

---

# Player Upgrades

The `GrantStatusEffect` type (`PlayerUpgrade/GrantStatusEffect`) applies an authored
`StatusEffect` to the player for `stacks` stacks under any trigger condition, and removes it
again on `OnRemove`. Used by `Eldritch Exchange`, `Solar Wind`, `Shock Absorber`, `Momentum`, `Moonbound Instinct` and
`Ethereal Mirage`.

The `FreeCast` type (`PlayerUpgrade/FreeCast`) counts consecutive casts of one slot and makes the
next cast free after `castsRequired`, granting `stacks` of `effect`. It has no trigger conditions:
`PlayerAttackHandler` calls it on every player-triggered cast, and `chance` / `cooldown` gate only
the free cast. Used by `Resonance`.

The `GoldBuff` type (`PlayerUpgrade/GoldBuff`) grants its `buffs` once per `goldPerStack` gold held, up
to `maxStacks`, and removes them on `OnRemove`. Used by `Midas Touch`.

The `Overhealth` and `AddChain` types are passive: they configure the player on `OnUnlock`
and undo it on `OnRemove`, so they carry no trigger conditions, chance or cooldown.

Overhealth from any source sits above `EffMaxHp`, is spent before health when damage lands and is
cleared on death. Without an `Overhealth` upgrade it decays by the `EntityHealth` default of 25% of
the current pool every 0.5s (`DefaultOverhealthDecayPct` / `DefaultOverhealthDecayInterval`);
an `Overhealth` upgrade overrides that rate and restores the default when removed.

The `BloodBank` type (`PlayerUpgrade/BloodBank`) applies its `bloodPool` status effect permanently
on `OnUnlock` and writes its `storePct` into it. `EntityHealth.ChangeHealth` feeds every point of
real health lost into the pool (overhealth-absorbed damage and overkill past 0 HP don't count;
health spent on attack costs does). Any trigger condition releases the whole pool as overhealth.
Used by `Blood Bank`.

Folder: `Assets/data/PlayerData/PlayerUpgrade`. All except `Decoy Upgraded`, `Solar Wind`,
`Oblivion`, `Ultrasonic` and `Moonbound Instinct` (the capstone-only upgrades), the keystone-only `Ethereal Mirage` pair,
the keystone-only `Eldritch Exchange`, `Hypercarry`, `Reminiscence`, `Resonance` and `Serenade` are present in `WaveManager.treasurePool`. Entries marked with an unlock wave carry a `minWave` on
their `PlayerUpgradeReward` and cannot be rolled before that wave.

Upgrades with `noMirror` set are never copied by the [Mirror Boss](#mirror-boss): `Eldritch Exchange`, `Hypercarry`,
`Hex Cast`, `Starlit Reflexes`, `Moonbound Instinct`, `Resonance`, and Ethereal Mirage's `Cosmic Afterimage` and `Cosmic Superimposition`.

## Hypercarry
- Asset: `DashAdvance`
- Type: CooldownAdvance
- Conditions: OnStartDash
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Amount: 12
- Advance Type: All
- Description: Dashing advances all cooldowns by 12%.
- Unlocked by: `Node_hycarry` ("Hypercarry" keystone, 5 skill points, `undoCost` 50, Master mode)
- Not in `treasurePool` — keystone-only.

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

## Chaos Theory
- Asset: `Chaos Theory`
- Type: SpawnProjectile
- Conditions: OnTeleport
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Projectile: `Chaos Theory.prefab`
- Description: Teleporting through a `teleportToProjectile` attack drops a Chaos Theory
  explosion on the player 0.15s later. The blast applies Spellworn on every hit and stuns
  40% of the time. `OnTeleport` dispatches without a spawn center, so the explosion lands
  on the player at the teleport destination rather than on the projectile that caused it.

## Contagion
- Asset: `Contagion`
- Type: DoTSpreadOnKill
- Conditions: OnKill
- Chance: 100%
- Cooldown: 0s
- Delay: 0s (must stay 0 — the dying enemy's DoTs are cleared right after `OnKill` fires)
- Max Targets: 3
- Radius: 3 tiles
- Duration: 50% of remaining
- Description: When the player kills an enemy, every player-sourced DoT on it spreads to
  the 3 nearest living enemies within 3 tiles that do not already carry that DoT. Each
  copy keeps the original's stacks and lasts 50% of the original's remaining duration.
  Copies are re-applied from the DoT's `origin` asset.

## Crescendo
- Asset: `Crescendo`
- Type: CooldownAdvance
- Conditions: OnBasicAttack
- Chance: 100%
- Cooldown: 1s
- Delay: 0s
- Amount: 8
- Advance Type: Ult
- Description: Basic attacks advance the Ultimate cooldown by 8% of its length, at most
  once per second.

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
- Description: Dashing spawns a decoy that lasts 4 seconds. The decoy is a `Targetable`,
  so enemies chase it when it is the nearest target on their next 1-3s retarget. It does
  not taunt on spawn (removed in v0.6.9).

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
- Description: Dashing spawns a decoy that draws enemies for 6 seconds, then detonates at its
  own position for 225% Spell damage and applies Vulnerable.
- Unlocked by: `Node_decoy` ("Cosmic Superimposition", 3 skill points, prerequisite
  `Node_ms2`, requires the base Decoy upgrade, which it consumes on unlock and returns
  on refund)

## Eldritch Exchange (Keystone)
- Asset: `Eldritch Exchange`
- Type: GrantStatusEffect
- Conditions: OnConsumeHealth
- Chance: 100%
- Cooldown: 2s
- Delay: 0s
- Effect: `eldex` (Lifesteal, 8s, 10 stacks), 1 stack per trigger
- Description: Paying health for an attack grants a stack of Lifesteal: each stack has a 50%
  chance to heal 0.5% of damage dealt on hit (1s cooldown).
- Unlocked by: `Node_eldex` ("Eldritch Exchange" keystone, 5 skill points, `undoCost` 50, Master
  mode, prerequisite `Node_hp3b`)
- Not in `treasurePool` — keystone-only.

## Ethereal Mirage (Keystone)
- Asset: `Ethereal Mirage` (folder `PlayerUpgrade/Keystone`)
- Type: GrantStatusEffect
- Conditions: OnUltAttack
- Chance: 100%
- Cooldown: 24s
- Delay: 0s
- Effect: `Mirage` (18s), 1 stack per trigger
- Companion: `Ethereal Mirage Cooldown Indicator` (GrantStatusEffect, OnUltAttack, 24s
  cooldown) applies the `Mirage Cooldown` Info marker (24s) so the cooldown shows in the
  status bar
- Description: Casting an Ultimate summons 3 translucent clones (60% opacity) in a ring
  2 units around the player. They follow the player at their spawn offsets and repeat
  every attack the player casts, firing the same way from their own positions. Each
  living clone grants +12% `moveSpeedPct` and -15% `damagePct`, removed when it dies or
  the effect ends.
- Clones:
  - Receive 50% of the player's flat stats (attack, Intelligence, max HP, armor, regen,
    move speed and their `Eff` versions). Percent and chance stats copy at 100%
  - Have their own health and take hits from enemy projectiles, but ignore knockback and
    crowd control. Enemies target the nearest clone, decoy or player in range
  - Repeat attacks for free: no cost, cooldown or cast bar. Rushes, orbit interactions and
    the player's own on-cast upgrade triggers are not repeated
  - Their hits count as the player's: on-hit, on-crit, on-deal-damage, overkill and on-kill
    upgrades fire on the player (sharing its upgrade cooldowns), along with lifesteal,
    resource gains on hit, XP and gold
  - Vanish without effect when Mirage expires or the player dies. Re-triggering Mirage
    refreshes it and refills any killed slots
- Unlocked by: `Node_ethmirage` ("Ethereal Mirage" keystone, 5 skill points, `undoCost`
  50, prerequisite any of `Attack/Armor`, `Health/Armor`, `Health/Intelligence` or
  `Projectile Speed`). Grants both upgrades.
- Not in `treasurePool` — keystone-only.

## Exsanguinate
- Asset: `Exsanguinate`
- Unlocks: wave 15
- Type: Overhealth
- Conditions: none (applied on unlock)
- Conversion: 50% of healing received at full health
- Decay: 20% of the current pool per 0.5s
- Convert Regen: off (see note)
- Description: While at full health, half of every heal becomes overhealth instead. Overhealth
  sits above `EffMaxHp`, is spent before health when damage lands, and bleeds off 20% of what
  remains every 0.5s. Cleared on death.
- Note: `convertRegen` is authored off, so health regen is *not* one of the heals that convert
  — regen stops at full health as usual. Before v0.4.9 the `RegenHp` guard ignored the flag
  and regen converted here anyway.
- Upgraded by the `Node_oblivion` capstone into Oblivion, which consumes it.

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

## Midas Touch
- Asset: `Midas Touch`
- Type: GoldBuff
- Conditions: none (passive loop while equipped)
- Gold Per Stack: 250
- Max Stacks: 100
- Buffs per stack: +2% damagePct
- Description: Every 250 gold held grants +2% damage, up to 100 stacks. Gold is checked every 0.2s
  of real time, so spending gold in the shop drops the bonus straight away, even while paused.

## Momentum
- Asset: `Momentum`
- Type: GrantStatusEffect
- Conditions: OnStartDash, OnRushStart
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Effect: `Momentum` (11s), 1 stack per trigger
- Description: Starting a dash or a rush grants Momentum: +6% moveSpeedPct and +14%
  rushImpactPct per stack, max 3 stacks.

## Oblivion (Capstone)
- Asset: `Oblivion`
- Type: Overhealth
- Conditions: none (applied on unlock)
- Conversion: 100% of healing received at full health
- Decay: 15% of the current pool per 0.5s
- Convert Regen: on (see note)
- Description: Exsanguinate with every number improved. At full health the entire heal —
  health regen included, which Exsanguinate no longer converts — becomes overhealth instead
  of being wasted, and the pool bleeds off 15% every 0.5s rather than 20%. Cleared on death.
- Unlocked by: `Node_oblivion` ("Oblivion" capstone, 3 skill points, prerequisites `Node_h2`
  and `Node_h2a`, requires the Exsanguinate Awakening). Unlocking it consumes Exsanguinate —
  both upgrades write the same `EntityHealth.SetOverhealth` config, so they never stack.
  Refunding the node returns Exsanguinate.
- Not in `treasurePool` — capstone-only.
- Note: `convertRegen` is authored on, which is what makes health regen keep ticking at full
  health so it can feed the pool. As of v0.4.9 the flag is read by `EntityHealth.RegenHp`
  rather than being implied by a non-zero conversion percent — see the `RegenHp` note in the
  v0.4.9 changelog, the guard needs one more pass before this reads correctly at runtime.

## Paradox
- Asset: `Paradox`
- Unlocks: wave 15
- Type: Paradox
- Conditions: none
- Chance: 0%
- Cooldown: 0s
- Delay: 0s
- Description: On unlock, grants globalDoTCanCrit; removed on unequip. Allows global DoTs
  to crit.

## Reminiscence
- Asset: `Reminiscence`
- Type: Reminiscence
- Conditions: OnCrit
- Chance: 30%
- Cooldown: 3s
- Delay: 0.35s
- Cooldown Effect: Reminiscence Cooldown (4s)
- Description: 30% chance on a critical hit to immediately perform an extra attack of a
  randomly chosen equipped attack type. Slots sealed by the Sealed anomaly are never picked, and
  the extra attack does not count toward, or spend, a Resonance free cast.
- Unlocked by: `Node_remin` ("Reminiscence" keystone, 5 skill points, `undoCost` 50, Master mode)
- Not in `treasurePool` — keystone-only.

## Resonance
- Asset: `Resonance`
- Type: FreeCast
- Conditions: none (`PlayerAttackHandler` reports each cast)
- Chance: 100%
- Cooldown: 0s
- Casts Required: 3
- Effect: `Stellar Resonance`, 1 stack per free cast granted
- Description: Casting the same attack slot 3 times in a row makes the next cast of that slot free:
  no resource cost and no cooldown used. Chance and cooldown are rolled when the streak completes,
  and a failed roll resets the streak. Casting a different slot resets the streak and cancels an
  unused free cast. Upgrade-triggered casts don't count. An interrupted free cast stays available.
- Unlocked by: `Node_reso` ("Resonance" keystone, 5 skill points, `undoCost` 50, Master mode)
- Not in `treasurePool` — keystone-only.

## Serenade
- Asset: `Serenade`
- Type: AdditionalDamage
- Conditions: OnDealDamage
- Chance: 35%
- Cooldown: 0s
- Delay: 0s
- Percent Amount: 24%
- Damage Type: True
- Description: 35% chance to deal 24% of the damage dealt again as True damage.
- Unlocked by: `Node_seren` ("Serenade" keystone, 5 skill points, `undoCost` 50, Master mode)
- Not in `treasurePool` — keystone-only.

## Shatterpoint
- Asset: `Shatterpoint`
- Type: SpawnProjectile
- Conditions: OnCrit
- Chance: 100%
- Cooldown: 0.5s
- Delay: 0s
- Projectile: `Shatterpoint.prefab` (`Shatterpoint ad` / `Shatterpoint pd`, folder `Treasure Pool/Shatterpoint`)
  - Size: 2, lifetime 0.5s, pierce 6, random direction
  - Damage: 70% True
  - Effect: 100% on hit (`Shatterpoint`)
- Description: Critical hits shatter the enemy and remove their Stun or Freeze to deal additional true damage.
- In `treasurePool`, no unlock wave.

## Shock Absorber
- Asset: `Shock Absorber`
- Type: GrantStatusEffect
- Conditions: OnTakeHit
- Chance: 100%
- Cooldown: 1s
- Delay: 0s
- Effect: `Voltaic Pulse` (9s, 4 stacks), 1 stack per trigger
- Description: Taking a hit grants a stack of Voltaic Pulse, at most once per second. Each
  stack gives +12% physicalDmgPct and +8% moveSpeedPct, and costs 5% damageRes.

## Solar Wind (Capstone)
- Asset: `SolarWind`
- Type: GrantStatusEffect
- Conditions: OnHealthRegen
- Chance: 60%
- Cooldown: 1s
- Delay: 0s
- Effect: `Solar Wind` (8s, 6 stacks), 1 stack per trigger
- Description: 60% chance on each health regen tick to gain a stack of Solar Wind, at
  most once every 1s.
- Unlocked by: `Node_solarwind` ("Solar Wind" capstone, 3 skill points, prerequisite
  `Node_hprp5`, requires the Stellar Surge Awakening). Unlocking it consumes Stellar
  Surge — the Awakening is removed as Solar Wind is granted, so the health regen tick
  rolls for Solar Wind instead of the heal. Refunding the node returns Stellar Surge.
- Not in `treasurePool` — capstone-only.

## Soul Rend
- Asset: `SoulRendPU`
- Type: SoulRendPU
- Conditions: OnUltAttack
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Projectile: `Shattered Vessel A.prefab`
- Soul Rend Effect: `Soul Rend` status effect
- Description: On unlock, registers the Soul Rend stacking buff against the Basic and Skill
  attack slots, so it applies to whatever attack fills them (an attack swapped in later is
  covered) and is removed with the upgrade. Using an Ultimate at 50 or more stacks fires the
  Shattered Vessel projectile and then clears the stacks after 0.3s.

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
- Upgraded by: `Node_moonboundinstinct` into [Moonbound Instinct](#moonbound-instinct-capstone)

## Moonbound Instinct (Capstone)
- Asset: `Moonbound Instinct` (`PlayerUpgrade/Tree`)
- Type: GrantStatusEffect
- Conditions: OnCounterDodge
- Chance: 100%
- Cooldown: 0.5s
- Delay: 0s
- Effect: `Moonbound`, 1 stack
- `noMirror`: on
- Description: Dashing through a hostile hit grants a stack of Moonbound (8s, max 4): +12%
  spellDmgPct, +16% manaGainPct and +3% resPen per stack. Replaces Starlit Reflexes' flat mana.
- Unlocked by: `Node_moonboundinstinct` ("Moonbound Instinct", 3 skill points, `undoCost` 50,
  prerequisite `Node_mxm4`, requires Starlit Reflexes, which it consumes on unlock and returns
  on refund). The prerequisite chain is four Maximum Mana nodes (`Node_mxm1`-`Node_mxm4`, +4 maxMana
  each, 1 skill point each) off `Node_spr2`

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
- Conditions: OnEndDash, OnRushEnd
- Chance: 100%
- Cooldown: 1s
- Delay: 0s
- Projectile: `Supersonic.prefab`
- Description: Ending a dash or a rush spawns 3 Supersonic projectiles, at most once per
  second.`

## Ultrasonic (Capstone)
- Asset: `Ultrasonic`
- Type: SpawnProjectile
- Conditions: OnEndDash, OnTeleport, OnRushEnd
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Projectile: `Ultrasonic.prefab`
- Description: Ending a dash or a rush, or teleporting through a `teleportToProjectile`
  attack, spawns 7 Ultrasonic projectiles, with no cooldown.
- Unlocked by: `Node_ultrasonic` ("Ultrasonic" capstone, 3 skill points, prerequisites
  `Node_dd3` and `Node_dcr3`, requires the Supersonic upgrade). Unlocking it consumes
  Supersonic. Refunding the node returns Supersonic.
- Not in `treasurePool` — capstone-only.

## Tempo
- Asset: `Tempo`
- Type: CooldownAdvance
- Conditions: OnAttack
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Amount: 18
- Advance Type: Dash
- Description: Every attack advances the dash cooldown by 18% of its length.

## Terminal Cascade
- Asset: `Terminal Cascade`
- Unlocks: wave 15
- Type: AddChain
- Conditions: none (applied on unlock)
- Retrigger Chance: 12%
- Description: When a chain of `additionalAttack` spawns reaches its end — the last link
  has no further additional attack, or its `additionalChance` roll fails — there is a 12%
  chance to fire the attack that started the chain again, from the player and aimed at the
  cursor. The retrigger pays no cooldown or resource cost, and the new chain can loop again.

## Wipeout
- Asset: `Wipeout`
- Type: DoTSpread
- Conditions: none (passive loop while equipped)
- Radius: 3 tiles
- Spread Chance: 25%
- Spread Interval: the debuff's own `tickInterval` (`useTickInterval` on)
- Description: Every player-sourced debuff on an active enemy has a 25% chance, on each of
  its own ticks, to copy itself onto every living enemy within 3 tiles that does not
  already carry it. Re-applied from the debuff's `origin` asset, so a spread chain never
  clones a clone. Per-debuff timers are pruned every 2s, and the loop sits out
  zero-timescale frames.
- Unlocked by: `Node_wipeout` ("Wipeout" keystone, 3 skill points, `undoCost` 50, Master mode,
  prerequisite `Node_sepp4`, requires the Contagion Awakening)
- Not in `treasurePool` — keystone-only.

---

# Status Effects

Folder: `Assets/data/StatusEffect`. `Pulled`, `Slow`, `Stun` and `Vulnerable` assets sit in
subfolders named after their class.

Status effect potency (`sePotPct` on the applier) scales DoT damage per tick, `StatBuffs` values,
`StatReduction` percentages and `Pulled` pull speed. Flag stats in a `StatBuffs` (`isImmune`, `CanMove`,
`CanAttack` and the other `Can*` / `Is*` toggles, `globalDoTCanCrit`, `Level`) are never scaled.

| Asset | Class | Name | Duration | Tick | Max stacks | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| `AttackInc 14 2 40` | StatBuffs | Sharpened Instincts | 14s | - | 2 | +40% atkPct per stack |
| `Blood Pool` | BloodPool | Blood Pool | permanent | - | 1 | Stores `storePct`% of health lost (set by Blood Bank, 50%); emptied into overhealth when Blood Bank triggers |
| `Blaze Soul` | AttackReplacement | Blaze Soul | 6s | - | 1 | Replaces the attack with `Blaze A1 AD` (Cosmic Blaze) |
| `Bleed 5 1 3 30 EffAtk` | DoT | Bleed | 3s | 0.5s | 5 | 8% EffMaxHp per tick |
| `Burn 6 1 5 15` | DoT | Burn | 6s | 1s | 5 | 35% EffAtk per tick |
| `Burn 8 1 6 15` | DoT | Burn | 8s | 1s | 5 | 15% EffAtk per tick |
| `Afflicted` | StatReduction | Afflicted | 12s | - | 8 | -10% maxHp per stack |
| `Celestial Protection` | StatBuffs | Celestial Protection | 8s | - | 4 | +3% damageRes, +6 armor, +4% armorPct per stack |
| `Cosmic Afterimage` | Info | Cosmic Afterimage Cooldown | 6s | - | 1 | Cooldown marker |
| `Crumbling 6 10 4` | StatReduction | Crumbling | 6s | - | 4 | -10% armor per stack |
| `Decay` | StatBuffs | Decay | 4s | - | 6 | -12% hpPct, -14% stRegPct, +4% resPen per stack |
| `DotDetonator 0.5 2` | Detonator | Detonator | 0.5s | - | 1 | Detonates every DoT stack for 250% as True, scaled by each DoT's potency, then removes them all |
| `Enraged` | StatBuffs | Enraged | permanent | - | 1 | +30% atkPct, +25% attackSpeedPct, +20% moveSpeedPct |
| `Freeze` | Freeze | Frozen | 2s | - | 1 | Cannot move, attack or dash; no passive health regen; knockback and pulls do nothing, and any rush in progress ends |
| `Heartburn` | StatBuffs | Heartburn | 6s | - | 15 | +4% damagePct, +12% critDamage, +18% stCostPct, -16% hpRegPct per stack |
| `Holy Bounty` | StatBuffs | Holy Bounty | 24s | - | 1 | +80% addDmgPct, +30% resPen, +15% damageRes |
| `Mirage` | EtherealMirage | Ethereal Mirage | 18s | - | 1 | Summons 3 clones at 50% flat stats, 60% opacity, radius 2; each living clone gives +12% moveSpeedPct, -15% damagePct. See Ethereal Mirage above |
| `Mirage Cooldown` | Info | Ethereal Mirage Cooldown | 24s | - | 1 | Cooldown marker |
| `Momentum` | StatBuffs | Momentum | 11s | - | 3 | +6% moveSpeedPct, +14% rushImpactPct |
| `Moonbound` | StatBuffs | Moonbound | 8s | - | 4 | +12% spellDmgPct, +16% manaGainPct, +3% resPen per stack |
| `Overheat` | StatBuffs | Overheat | 7s | - | 5 | -8% atkPct, -12% stRegPct per stack |
| `Poison 2 0.5 1 20 Atk` | DoT | Poison | 2s | 0.5s | 1 | 20% EffAtk per tick |
| `Pulled 0.6 1 1.5 5 0.1` | Pulled | Possessed | 0.6s | 0.016s | 1 | Pull speed 5 (+2/stack), 1.5 radius |
| `Pulled 0.75 1 1.4 2 1.5` | Pulled | Possessed | 0.75s | 0.016s | 1 | Pull speed 1.4 (+2/stack), 1.5 radius |
| `Pulled 0.6 1 15 3` | Pulled | Possessed | 0.6s | 0.016s | 1 | Pull speed 15 (+2/stack), 3 radius |
| `Radiation 4 0.25 8 2 CritDmg` | DoT | Radiation | 5s | 0.25s | 10 | 5% critDamage per tick |
| `Reminiscence Cooldown` | Info | Reminiscence Cooldown | 4s | - | 1 | Cooldown marker |
| `Shatterpoint` | Shatter | Shatterpoint | 0.05s | - | 1 | Removes every Stun and Freeze on the target; each removed effect deals one hit of 400% of the source's `critDamage` stat as True |
| `Slow 3 8 10` | StatReduction | Slow | 3s | - | 8 | -10% moveSpeed per stack |
| `Slow 5 3 15` | StatReduction | Slow | 5s | - | 3 | -15% moveSpeed per stack |
| `Slow 4 15 5` | StatReduction | Slow | 4s | - | 15 | -5% moveSpeed per stack |
| `Slow 8 2 30` | StatReduction | Slow | 8s | - | 2 | -30% moveSpeed per stack |
| `Slow 8 4 10` | StatReduction | Slow | 8s | - | 4 | -10% moveSpeed per stack, capped at -90% |
| `Solar Wind` | StatBuffs | Solar Wind | 8s | - | 6 | +4 hpRegen, +9% hpRegPct, +6% moveSpeedPct per stack; all stacks drop on expiry |
| `Soul Rend` | SoulRend | Soul Rend | 1.5s | - | 100 | See the Soul Rend upgrade above |
| `Spellworn` | StatBuffs | Spellworn | 4s | - | 2 | -15% spellRes per stack |
| `Stellar Resonance` | StatBuffs | Stellar Resonance | 11s | - | 3 | +8% resPen, +12% ProjSpd per stack |
| `Stun 1` | Stun | Stun | 1s | - | 1 | Cannot move or attack |
| `Stun 2` | Stun | Stun | 2s | - | 1 | Cannot move or attack |
| `Stun 3` | Stun | Stun | 3s | - | 1 | Cannot move or attack |
| `Stun 6` | Stun | Stun | 6s | - | 1 | Cannot move or attack |
| `Supersonic Cooldown` | Info | Supersonic Cooldown | 3s | - | 1 | Cooldown marker |
| `Vulnerable 6 30` | StatBuffs | Vulnerable | 6s | - | 1 | -30% damageRes |
| `Vulnerable 6 3 8` | StatBuffs | Vulnerable | 6s | - | 3 | -8% damageRes per stack |
| `Vulnerable 8 2 20` | StatBuffs | Vulnerable | 8s | - | 2 | -20% damageRes per stack |
| `Voltaic Pulse` | StatBuffs | Voltaic Pulse | 9s | - | 4 | +12% physicalDmgPct, +8% moveSpeedPct, -5% damageRes per stack |
| `eldex` | Lifesteal | Lifesteal | 8s | - | 10 | Each stack: 50% chance to heal 0.5% of damage dealt on hit, 1s cooldown. See Eldritch Exchange above |
| `Weaken 5 10 4` | StatReduction | Weaken | 5s | - | 4 | -10% attack per stack |

Used by enemies rather than the player: `Crumbling 6 10 4` (Crab, and the Golem's Orbit),
`Poison 2 0.5 1 20 Atk` (Slime), `Slow 8 2 30` (Lich), `Stun 6` (Cultist),
`Afflicted` (Bat Mark), `Stun 2` (also used by BallSpam), `Slow 4 15 5` and
`Freeze` (Slime (Frost)'s Blizzard at 20%, and the player's Subspace Blitz), `Overheat` (Slime (Magma)'s Eruption),
`Slow 8 4 10` (the Golem's Orbit), `Stun 1` (the Reaper's Toss), `Vulnerable 8 2 20`
(the Reaper's Strike), `Burn 8 1 6 15` (the Reaper's Spam).

`Stun 3` and `AttackInc 14 2 40` are authored for the Golem's Charge and are both
`selfApply`, so the Golem inflicts them on itself: the stun is guaranteed, the attack buff
lands 40% of the time. The player's Nitro Accelerator also applies `Stun 3`, to enemies (70% on hit).
`Vulnerable 6 3 8` is shared — Ignition Flash, Nitro Explosion and the Golem's Cross all apply it.
`Decay` is self-applied on every Nitro Accelerator cast.

`Slow 5 3 15` is authored but no longer referenced by any projectile — Blizzard moved to
`Slow 6 15 5` in v0.3.9, which was reauthored as `Slow 4 15 5` (4s instead of 6s) in v0.4.1_2.
The `Radiation 4 0.25 8 2 CritDmg` asset name is likewise stale: it now runs 5s with 10 stacks.
`Pulled source` (1s, pull speed 40) is authored but not referenced by anything.
`Enraged` is applied permanently by the Twin Crowns and Rampage anomalies.

---

# Enemy Attack Animation

`EnemyAttackHandler` sets the Animator `attackIndex` integer when an attack starts. It uses the
attack's `AttackData.animationIndex` when that is 0 or above, otherwise (`-1`, the default) the
attack's position in the `attacks` list. This lets attacks share a clip or be added to the list
without reordering the controller.

| Attack | `animationIndex` |
|---|---|
| Cultist Circleballs | 2 |
| Cultist Teleport | 3 |

---

# Mirror Boss

Folder: `Assets/data/entity/enemy/Bosses/mirror`. The `MirrorBoss` prefab is in the Unlimited
`bossPrefabs` pool, so it can roll on any Unlimited boss wave alongside the other five bosses.
It is also the sixth and final fight of both Boss Rush parts, shown on the boss bar as **Echo**
(`[Lv. 90] Echo` closing `ws_6`, `[Lv. 105] Echo` in `BossRush Part 2`).

When it spawns, the `MirrorBoss` component copies the player's build onto the boss:

- **Attacks** (`copyAttacks`): every attack in the player's `PlayerAttackHandler.attacks` replaces the
  boss's `EnemyAttackHandler.attacks`. Cooldowns use the player formula (attack speed and the matching
  Basic/Skill/Ultimate cooldown reduction), read from the boss's own stats. Attacks with no max range
  use `fallbackRange` (6), and aimed attacks point at the boss's target.
- **Awakenings** (`copyUpgrades`): the player's active `PlayerUpgrade`s, minus any with `noMirror`,
  are added to the boss's own `PlayerUpgradeManager`. `maxUpgrades` (`0` = all) keeps a random subset.
  Casting a copied attack fires the boss's `OnAttack` and per-type attack triggers.

The copy is a snapshot: attacks or Awakenings the player gains after the boss spawns are not added.
Player stats, synergies and gear are never copied. The boss uses the `mirror base` stats (values before level scaling):

| Stat | Value | Stat | Value |
| --- | --- | --- | --- |
| `maxHp` | 400 | `damageRes` | 10 |
| `attack` | 2 | `dodgeChance` | 5 |
| `damagePct` | -30 | `dodgeResPct` | 40 |
| `critChance` | 10 | `spellRes` | 10 |
| `critDamage` | 30 | `effectRes` | 0 |
| `armor` | 30 | `detectionRange` | 15 |
| `moveSpeed` | 0.9 | | |
| `xpDrop` | 800 | `goldDrop` | 80 |

`globalCooldown` is 1s. The prefab reuses the player's animator controller.

## Doppelganger

Folder: `Assets/data/entity/enemy/Enemies/doppelganger`. A regular enemy built on the same `MirrorBoss`
component (`copyAttacks` and `copyUpgrades` on, `maxUpgrades` 0 = all, `fallbackRange` 6, `globalCooldown` 1s),
so every Doppelganger copies the player's attacks and Awakenings when it spawns. It joins the Unlimited
`enemyPrefabs` roster from wave 40 and uses the `Doppelganger base` stats (values before level scaling):

| Stat | Value | Stat | Value |
| --- | --- | --- | --- |
| `maxHp` | 80 | `damageRes` | 5 |
| `attack` | 2 | `dodgeChance` | 5 |
| `damagePct` | -40 | `dodgeResPct` | 35 |
| `critChance` | 3 | `spellRes` | 5 |
| `critDamage` | 15 | `effectRes` | 0 |
| `armor` | 15 | `detectionRange` | 15 |
| `moveSpeed` | 0.65 | | |
| `xpDrop` | 22 | `goldDrop` | 8 |

# Sealed Anomaly

`AnomalyType.Sealed`, run by `SealedInstance`. No fail condition; it pays the standard anomaly reward.

- One attack slot (Basic, Skill or Ultimate) is sealed for the wave. The slot is picked from the slots the player
  has an attack in when the anomaly choices are rolled, so the anomaly button names it.
- The other two slots gain `+x%` of their cooldown reduction stat (`basicCdRedPct`, `skillCdRedPct`,
  `ultCdRedPct`), with `x` rolled from `anomalyMinVal`..`anomalyMaxVal` and rounded.
- The seal goes through `PlayerAttackHandler.SetSlotLocked`. A sealed slot fails `CanCast`, and `PerformAttack`
  refuses it, upgrade-triggered casts included. Its cooldown keeps recovering while sealed.
- `Cleanup()` removes the seal and the cooldown reduction at the end of the wave.

| Asset | Waves | Cooldown reduction | Boss waves |
| --- | --- | --- | --- |
| `Regular/Sealed` | 10–105 | 15–30% | allowed |
| `Unlimited/USealed` | 0–128 | 5–30% | allowed |

All Unlimited anomaly assets now cap at `maxWave` 128.

# Attack Cooldown Buttons

`PlayerAttackCooldownUI` sets the button's border color from the attack's state, highest priority first:

| State | Border | Field |
| --- | --- | --- |
| Sealed | grey, plus a grey icon, a full cooldown fill and the optional `lockOverlay` | `lockedBorderColor`, `lockedIconColor` |
| Free cast ready ([Resonance](#resonance)) | cyan | `freeCastBorderColor` |
| Can't cast (on cooldown, can't afford, can't attack) | red | `blockedBorderColor` |
| Stacked attack with more than 0 but fewer than max stacks | yellow | `partialStackBorderColor` |
| Ready | green for stacked attacks, otherwise the prefab's border color | `stackedBorderColor` |

Flashes interrupt the border for a moment:

- **Blocked**: 3 red flashes (`flashBorderColor`, 0.06s) when a cast is refused, sealed slots included.
- **Ready**: 2 green flashes (`readyFlashColor`, 0.08s) whenever the attack regains a stack, whether from the
  cooldown or a cooldown advance. Attacks with an effective cooldown under `minReadyFlashCooldown` (0.5s)
  skip it. The check runs the frame the cooldown finishes.

A sealed button's tooltip gains a "Sealed" line, and a button with a free cast ready gains
"Resonance: next cast free".

# Collectibles

Folder: `Assets/data/Collectibles`. Spawned by the `CollectibleSpawner` in `New.unity`,
which holds all twelve assets, prewarms 8 pickups, ticks every 2s, caps the field at 10 live
pickups and places them 3–6 units from the player.

| Asset | Type | Pays | Roll | Chance | Cooldown | Lifetime | Color | Mode |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `XP` | Xp | % of `XpReq` | 3–15% | 8% | 10s | 30s | magenta | Simple |
| `Gold` | Gold | gold | 5–65 | 6% | 5s | 25s | gold | Simple |
| `Health` | Heal | % of `EffMaxHp` | 3–20% | 4% | 10s | 25s | red | Simple |
| `Stamina` | Stamina | % of `EffMaxStamina` | 3–15% | 4% | 15s | 25s | green | Simple |
| `Mana` | Mana | % of `EffMaxMana` | 3–15% | 3% | 15s | 25s | blue | Simple |
| `Reroll` | Rerolls | rerolls | 1 | 2% | 15s | 20s | teal | Expert |
| `SkillPoint` | SkillPoints | skill points | 1 | 1% | 20s | 20s | purple | Master |
| `Box Slime` | SpawnerBox | ambush | — | 2% | 45s | 45s | orange | Expert |
| `Box Bat` | SpawnerBox | ambush | — | 1% | 45s | 45s | orange | Expert |
| `Box Crab` | SpawnerBox | ambush | — | 1% | 45s | 45s | orange | Expert |
| `Box Cult` | SpawnerBox | ambush | — | 0.5% | 60s | 60s | orange | Master |
| `Box Doppelganger` | SpawnerBox | ambush | — | 0.25% | 45s | 45s | orange | Master |

**Chance** is rolled per spawner tick (2s), not per second, and only one pickup can spawn
per tick: candidates are filtered (chance above 0, not on cooldown, `minMode` at or below the run's mode tier), shuffled, and the
first to pass its own roll spawns. **Cooldown** then blocks that asset — and only that
asset — for its duration, counting down even between waves. **Lifetime** (`maxTime`) only
counts down while a wave is active, so a pickup left on the ground at wave end is still
there with the same time left when the next wave starts; it fades over its last 0.5s.

`Heal`, `Xp`, `Stamina` and `Mana` treat the rolled value as a percentage of the live stat
named above, so their worth scales with the build. `Gold`, `SkillPoints` and `Rerolls` are
flat counts. `Reroll` pickups are skipped entirely while Ironman Mode is on, and
`WaveManager.GrantRerolls` refuses the grant as a second guard.

Each pickup bobs in place, pulses a glow tinted with its `lightColor`, and shows what it
will pay as a world-space label (`+35 Gold`, `+8% HP`, `+1 Skill Point`). The same string
pops as a floating indicator on pickup.

During the Blackout anomaly, a pickup outside the vision circle (`BlackoutVision.InSight`)
hides its sprite, glow and label, and shows them again once it is in sight, the anomaly
ends, or it is reused from the pool. It can still be collected while hidden.

## Spawner Boxes

A `SpawnerBox` pickup reads "Ambush!". Touching it spawns `ambushMin`–`ambushMax` enemies,
each a random pick from `ambushEnemies`, within `ambushRadius` of the box. They use the
current wave's enemy level (the `WaveData` level on the regular manager, the wave-scaled
level on Unlimited) plus `ambushLevelBonus`, get the active anomaly's buffs and spawn hooks,
and are added to the wave's enemy count like split enemies, so the wave can't end until
they are dead. The box only triggers while a wave is active; outside one it stays put.

When the last ambush enemy dies, `rewardMin`–`rewardMax` collectibles drop within
`rewardRadius` of the box. Each drop is an independent weighted roll over `rewards`
(duplicates allowed) and rolls its own value and lifetime. Drops ignore the spawner's
live cap, but count toward it while on the ground. Other spawner boxes are never rolled,
`Reroll` entries are skipped in Ironman Mode, and entries above the mode tier (`SkillPoint`
below Master, `Reroll` in Simple) are skipped.

| Box | Enemies | Count | Level | Drops | Reward weights (XP / Gold / Reroll / SkillPoint) |
| --- | --- | --- | --- | --- | --- |
| `Box Slime` | Slime, Frost Slime, Magma Slime | 2–6 | +2 | 2–5 | 6 / 5 / 2 / 1 |
| `Box Bat` | Bat | 2–4 | +2 | 2–5 | 6 / 5 / 2 / 1 |
| `Box Crab` | Crab | 2–4 | +2 | 2–5 | 6 / 5 / 2 / 1 |
| `Box Cult` | Cultist Clone | 1–3 | +2 | 3–6 | 3 / 4 / 3 / 2 |
| `Box Doppelganger` | Doppelganger | 1–2 | +2 | 2–7 | 2 / 3 / 4 / 3 |

Every box spawns enemies within 3 units and drops rewards within 2 units. The rarer boxes
(Cult, Doppelganger) lean their drops toward rerolls and skill points.
