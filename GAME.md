## Aphelion 
- Type: Basic
- Cooldown: 1.6s
- Pattern: Single (1 count)
- Animation: 0.5s
- Stamina: -6 +1
- Mana: -8 +2%
- Projectile:
  - Speed: 6
  - Lifetime: 10.5s
  - Pierce: 3000
  - Size: 2
  - Damage: 35% Spell
  - Scaling: EffAtk
  - Time Before Same Enemy: 0.5s
  - Orbit: radius 1-2, orbits self, CCW

## Cyclone Cleave (Starting)
- Type: Skill
- Cooldown: 6s
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0.5s
- Stamina: -15
- Mana: +8%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 6
  - Size: 3
  - Damage: 540% Phys
  - Scaling: Atk
  - Knockback: 5 force

## Lacerate (Starting)
- Type: Basic
- Cooldown: 1s
- Pattern: Single (1 count)
- Spawn: 0.5 dist
- Animation: 0.5s
- Stamina: +6 +4
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 3
  - Size: 2.5
  - Damage: 180% Phys
  - Scaling: Atk
  - Rotation: 270°
  - Knockback: 3 force

## Astral Nova
- Type: Basic
- Cooldown: 3s
- Pattern: Single (1 count)
- Spawn: 3 dist, 1.5s delay
- Animation: 0.5s
- Stamina: +15%
- Mana: +15%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 6
  - Size: 2
  - Damage: 280% Spell, 30% True
  - Scaling: EffAtk
  - Effect: 100% chance on hit (applies -20% damage resistance)

## Autopilot
- Type: Additional
- Cooldown: 0s (upgrade-spawned)
- Pattern: Circle (3 count)
- Animation: 1s
- Health: +6 (on hit, based on damage dealt)
- Stamina: +4 (on hit, based on damage dealt)
- Projectile:
  - Speed: 6
  - Lifetime: 6s
  - Pierce: 1 (destroys on max pierce)
  - Size: 2
  - Damage: 295% Phys
  - Scaling: EffArmor
  - Movement: Spiral (spacing 2)
  - Homing: 0.5 range
  - Knockback: 8 force

## Blaze
- Type: Basic
- Cooldown: 26s
- Pattern: Single (1 count)
- Spawn: 0.5 dist, 0.25s delay
- Animation: 1s
- Stamina: +6 +3 +2%
- Mana: +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2.5
  - Damage: 930% Phys
  - Scaling: Atk
  - Additional: 30% chance on hit to create Blaze Spark
  - Effect: 100% self on cast (upgrades Blaze to Blaze Soul)
  - Knockback: 12 force

## Blaze Soul
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Animation: 0.5s
- Stamina: +1 +1%
- Projectile:
  - Speed: 12
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2
  - Damage: 685% Phys, 30% True
  - Scaling: Atk
  - Additional: 40% chance on hit to create Blaze Hyperspark
  - Effects: 2x 100% self on cast (refreshes Blaze Soul (8s), grants Heartburn (For each stack, increases crit damage by 20%, damage bonus by 6% and stamina cost by 18%, but reduce health regen by 12%.))
  - Knockback: 12 force

## Blaze Spark
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Animation: 0.5s
- Stamina: -20 -10% +1 +1%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 6
  - Size: 1.5
  - Damage: 185% Phys
  - Scaling: Atk

## Blaze Hyperspark
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist (fixed)
- Animation: 0.5s
- Stamina: +2 +2%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 8
  - Size: 2
  - Damage: 215% Phys, 12% True
  - Scaling: Atk

## Blood Pact
- Type: Basic
- Cooldown: 1.8s
- Pattern: Single (1 count)
- Spawn: 0.5 dist (fixed)
- Animation: 0.5s
- Health: -14 -11%
- Stamina: +2 +6 +6%
- Mana: +2
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 10
  - Size: 2.5
  - Damage: 60% Phys, 20% True
  - Scaling: MaxHP
  - Effect: 40% chance on hit (Bleed, 15% EffMapHp as DoT every 0.5s for 3s, max 5 stacks)
  - Knockback: 4 force

## Decoy Burst (Capstone)
- Type: Additional
- Cooldown: 0s (spawned on decoy expiry)
- Pattern: Single (1 count)
- Animation: 1s
- Stamina: +3 (on hit, based on damage dealt)
- Mana: +3 (on hit, based on damage dealt)
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 2.5
  - Damage: 225% Spell
  - Scaling: EffAtk
  - Effect: 100% chance on hit (Vulnerable, -30% Damage Res for 4s)
  - Knockback: 5 force
- Unlocked by: Cosmic Superimposition capstone skill node (requires Cosmic Afterimage, 3 skill points)

## Exodus
- Type: Ultimate
- Cooldown: 90s
- Pattern: Single (1 count)
- Spawn: 5 dist
- Animation: 0.75s
- Stamina: -40 -55%
- Mana: -40%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 2
  - Damage: 1365% Phys
  - Scaling: Atk
  - Additional: 60% chance on hit to create Exodus Wave
  - Knockback: 2 force

## Exodus Wave
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist, 0.75s delay
- Animation: 0.75s
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 3
  - Damage: 880% Spell
  - Scaling: EffAtk
  - Additional: 40% chance on hit to create Exodus Core
  - Knockback: 1 force

## Exodus Core
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist, 0.75s delay
- Animation: 0.75s
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 6
  - Damage: 110% True
  - Scaling: MaxHP
  - Knockback: 0 force

## Ignition Flash
- Type: Basic
- Cooldown: 2.8s
- Pattern: Single (1 count)
- Spawn: 0.65 dist
- Animation: 0.75s
- Stamina: +3
- Mana: +1%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 6
  - Size: 2.5
  - Damage: 285% Phys
  - Scaling: Atk
  - Effects: 100% (Burn, 6s, 1s tick interval, 5 max stacks, 35% EffAtk as DoT per tick) + 45% chance on hit (Vulnerable, 6s, max 3 stacks, -8% damage resistance per stack)
  - Knockback: 5 force

## Lifeforce
- Type: Skill
- Cooldown: 11s
- Pattern: Single (1 count)
- Spawn: 1 dist
- Animation: 0.75s
- Health: -30 -40%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 5
  - Size: 2.25
  - Damage: 90% Spell
  - Scaling: MaxHP
  - Special: 0.5x Orbits
  - Additional: 100% chance on hit to create Lifeforce Shard
  - Knockback: 6 force

## Lifeforce Shard
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Spread (3 count, 10 spread)
- Spawn: 0.75 dist, 0.25s delay
- Animation: 0.75s
- Stamina: +2
- Mana: +2
- Projectile:
  - Speed: 4
  - Lifetime: 6s
  - Pierce: 3000
  - Size: 1.25
  - Damage: 85% Phys
  - Scaling: MaxHP
  - Time Before Same Enemy: 0.5s
  - Additional: 35% chance on hit to create Lifeforce Burst
  - Knockback: 5 force

## Lifeforce Burst
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Spawn: 0 dist, 1s delay
- Animation: 0.75s
- Stamina: +4
- Health: +4 +3%
- Mana: +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 3000
  - Size: 3
  - Damage: 165% Spell
  - Scaling: MaxHP
  - Knockback: 14 force

## Luminaria
- Type: Ultimate
- Cooldown: 18s
- Pattern: Single (1 count)
- Animation: 1s
- Stamina: -15 +2
- Health: -10%
- Mana: -60 +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 3
  - Damage: 270% True
  - Scaling: EffHpReg
  - Use True Angle
  - Effect: 100% self on cast (Holy Bounty, +80% Additional Damage, +30% Res Pen, +15% Damage Res for 24s)
  - Effect: 40% chance on cast (Stun, 2s)
  - Knockback: 8 force

## Meteor Shower
- Type: Skill
- Cooldown: 5s
- Pattern: Barrage (48-76 count, 3 spread)
- Spawn: 5 dist
- Animation: 0.5s
- Stamina: -15 +2
- Mana: -48 +5
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 4
  - Size: 1.25
  - Damage: 80% Spell
  - Scaling: EffAtk
  - Use True Angle
  - Delay: 0.05-0.12s between projectiles

## Nebula
- Type: Skill
- Cooldown: 2s
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0.5s
- Stamina: -14 -8% +4
- Mana: -12 +3
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 12
  - Size: 3.5
  - Damage: 440% Phys
  - Scaling: Atk
  - Effect: 100% chance on hit (Radiation, Deals damaage equal to 3% of crit damage. This effect stacks up to 8 times and triggers every 0.25s.)
  - Knockback: 4 force

## Nirvana
- Type: Ultimate
- Cooldown: 24s
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0.5s
- Mana: -145 -10%
- Mana: +4 +1%
- Fires Orbits
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.5s
  - Pierce: 3000
  - Size: 4
  - Damage: 550% Spell
  - Scaling: EffAtk
  - Special: +20% damage per orbit
  - Knockback: 12 force

## Revelation
- Type: Ultimate
- Cooldown: 16s
- Pattern: Single (1 count)
- Spawn: 4 dist, 0.25s delay
- Animation: 0.5s
- Stamina: -70
- Mana: -50
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 5
  - Damage: 340% Spell
  - Scaling: EffAtk
  - Effect: 100% chance on hit (Detonates all DoTs for 250% of their remaining damage as true damage)
  - Knockback: 6 force

## Shattered Singularity
- Type: Ultimate
- Cooldown: 12s
- Pattern: Single (1 count)
- Spawn: 0.5 dist
- Animation: 0.5s
- Stamina: -60
- Mana: -60
- Projectile:
  - Speed: 0.5
  - Lifetime: 4.5s
  - Pierce: 1
  - Size: 1.5
  - Damage: 60% Spell
  - Scaling: EffAtk
  - Additional: 100% chance on hit to create Singularity Fragment

## Singularity Fragment
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Circle (6 count)
- Spawn: 1.25 dist (fixed), 0.25s delay
- Animation: 0.5s
- Stamina: +5%
- Mana: +5%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 8
  - Size: 2.5
  - Damage: 180% Phys, 660% Spell
  - Scaling: EffAtk

## Solar Collapse
- Type: Ultimate
- Cooldown: 14s
- Pattern: Single (1 count)
- Spawn: 2 dist
- Animation: 0.5s
- Stamina: -90% +3%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 6s
  - Pierce: 3000
  - Size: 3
  - Damage: 340% Phys, 25% True
  - Scaling: Atk
  - Time Before Same Enemy: 0.5s
  - Effects: 30% (Slow, Reduces speed by 10% per stack. Max 8 stacks, 3s duration) + 100% chance on hit (Pulled, 1.4 force, 1.5 radius)

## Starfury
- Type: Ultimate
- Cooldown: 6s
- Pattern: Single (1 count)
- Spawn: 3 dist
- Animation: 0s
- Mana: -24 -22%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 4
  - Damage: 120% Spell, 20% True
  - Scaling: MoveSpeedPct
  - Time Before Same Enemy: 0.1s

## Stellar Maelstrom
- Type: Skill
- Cooldown: 3s
- Pattern: Spread (14-24 count, 30±15 spread)
- Spawn: 0 dist
- Animation: 0.5s
- Stamina: -40 -44% +3
- Mana: -18 -52% +2
- Projectile:
  - Speed: 8
  - Lifetime: 1.25s
  - Pierce: 1
  - Size: 1.5
  - Damage: 180% Phys, 70% Spell
  - Scaling: Atk
  - Follow Distance: 2
  - Delay: 0.15-0.35s between projectiles
  - Knockback: 2 force

## Supernova
- Type: Skill
- Cooldown: 3s
- Pattern: Single (1 count)
- Spawn: 1 dist (fixed)
- Animation: 0.5s
- Stamina: -20 -10% +4
- Mana: -5
- Health: +5%
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 9
  - Size: 4
  - Damage: 190% Phys, 30% True
  - Scaling: Armor
  - Effect: 40% chance on hit (Weaken, Reduces attack by 10% per stack. Max 4 stacks, 5s duration)
  - Knockback: 3 force

## Warp
- Type: Skill
- Cooldown: 9s
- Pattern: Circle (2-4 count)
- Spawn: 0 dist (fixed)
- Animation: 1s
- Stamina: -15
- Mana: -50 -15%
- Mana: +3
- Projectile:
  - Speed: 0.8
  - Lifetime: 10s
  - Pierce: 3000
  - Size: 2
  - Damage: 60% Spell
  - Scaling: EffAtk
  - Time Before Same Enemy: 1.5s
  - Orbit: radius 1.25, orbits self, CCW
  - Additional: 15% chance on hit to create Warp Rift
  - Knockback: 2 force

## Warp (Capstone)
- Type: Skill
- Cooldown: 9s
- Pattern: Circle (2-4 count)
- Spawn: 0 dist (fixed)
- Animation: 1s
- Stamina: -40 +1
- Mana: -60 -20%
- Mana: +2 +2%
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
  - Knockback: 2 force
- Unlocked by: Warp capstone skill node (requires Warp, 3 skill points)

## Warp Rift
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count)
- Animation: 1s
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 1.5s
  - Pierce: 3000
  - Size: 1.25
  - Damage: 215% Spell
  - Scaling: EffAtk
  - Use True Angle
  - Effect: 100% chance on hit (Pulled, 5 force, 1.5 radius)

## Feedback Loop
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Circle (6 count)
- Spawn: 0.75 dist (fixed)
- Animation: 0.5s
- Mana: +1%
- Projectile:
  - Speed: 16
  - Lifetime: 1.5s
  - Pierce: 4
  - Size: 2
  - Damage: 15% Spell
  - Scaling: EffAtk

## Soul Rend
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Single (1 count) → Circle (8 count)
- Spawn: 3 dist → 0.1 dist (fixed), 0.1s delay
- Animation: 0.5s
- Projectile:
  - Speed: 0 (melee)
  - Lifetime: 0.75s
  - Pierce: 12
  - Size: 3
  - Damage: 580% Phys, 60% Spell
  - Scaling: Atk
  - Additional: 100% chance on hit to create Soul Fragment

## Soul Fragment
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
  - Scaling: Atk

## Supersonic
- Type: Additional
- Cooldown: 0s (follow-up)
- Pattern: Circle (3 count, 45 random spread)
- Animation: 1s
- Stamina: +3
- Mana: +3
- Projectile:
  - Speed: 9
  - Lifetime: 1s
  - Pierce: 3000
  - Size: 1.5
  - Damage: 110% True
  - Scaling: MoveSpeedPct
  - Effects: 60% (Slow, reduces speed by 15% per stack. Max 3 stacks, 5s duration)
  - Knockback: 4 force

# Player Upgrades

## Hypercarry
- Type: CooldownAdvance
- Conditions: OnSkillAttack, OnUltAttack
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Amount: 15
- Advance Type: All
- Description: Advance all cooldowns by 15% of their cooldown when dashing.

## Autopilot
- Type: SpawnProjectile
- Conditions: OnTakeHit
- Chance: 100%
- Cooldown: 2s
- Delay: 0.25s
- Projectile: Autopilot projectile
- Description: Taking a direct hit spawns 3 projectiles in a circle that home in on nearby enemies and heal the player.

## Decoy
- Type: Decoy
- Conditions: OnStartDash
- Chance: 100%
- Cooldown: 6s
- Delay: 0s
- Lifetime: 4s
- Spawn Offset: (0, 0, 0)
- Tint: White (61% alpha)
- Cooldown Effect: Yes
- Projectile: None (base version does not detonate)
- Description: Spawn a decoy that taunts enemies for 4 seconds when dashing. Applies a cooldown indicator effect on trigger.

## Decoy Upgraded (Capstone)
- Type: Decoy
- Conditions: OnStartDash
- Chance: 100%
- Cooldown: 5s
- Delay: 0s
- Lifetime: 6s
- Spawn Offset: (0, 0, 0)
- Tint: White (78% alpha)
- Cooldown Effect: Yes
- Projectile: Decoy Burst projectile
- Description: Spawn a decoy that taunts enemies for 6 seconds when dashing, then detonates at its own position for 225% Spell damage and applies Vulnerable. Applies a cooldown indicator effect on trigger.
- Unlocked by: Cosmic Superimposition capstone skill node (requires Cosmic Afterimage, 3 skill points)

## Feedback Loop
- Type: SpawnProjectile
- Conditions: OnSkillAttack, OnUltAttack, OnAttack
- Chance: 70%
- Cooldown: 0.3s
- Delay: 0s
- Projectile: Feedback Loop projectile
- Description: 70% chance to spawn Feedback Loop projectile when attacking, using a Skill, or using an Ultimate.

## Hex Cast
- Type: HexCast
- Conditions: None
- Chance: 0%
- Cooldown: 0s
- Delay: 0s
- Description: Allows Health to replace Stamina for attacks

## Paradox
- Type: Paradox
- Conditions: None
- Chance: 0%
- Cooldown: 0s
- Delay: 0s
- Description: Allows global DoTs to crit. 

## Reminiscence
- Type: Reminiscence
- Conditions: OnCrit
- Chance: 25%
- Cooldown: 4s
- Delay: 0.35s
- Cooldown Effect: Yes
- Description: 25% chance to randomly performa an additional attack when dealing critical damage. Has a 4s cooldown and applies a cooldown indicator effect.

## Serenade
- Type: AdditionalDamage
- Conditions: OnDealDamage
- Chance: 40%
- Cooldown: 0s
- Delay: 0s
- Percent Amount: 24%
- Damage Type: True
- Description: 40% chance to deal 24% additional True damage on all damage instances

## Soul Rend
- Type: SoulRendPU
- Conditions: OnSkillAttack, OnBasicAttack
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Projectile: Shattered Vessel A projectile
- Soul Rend Data: Yes
- Description: Grants 1 stack of Soul Rend on Basic or Skill attack, firing a Shattered Vessel projectile.

Soul Rend:
[1] +0.3% Attack
[5] +1 DefShred
[10] + 1% ResPen
[20] +4% PhysDmg
[25] +5% CritDmg
[50] +100% Ult Dmg
[50] Infused Ultimate

## Starlit Reflexes
- Type: GainMana
- Conditions: OnCounterDodge
- Chance: 100%
- Cooldown: 0s
- Delay: 0s
- Flat Amount: 10
- Percent Amount: 6%
- Description: Gain 10 flat mana + 6% max mana when dashing into an projectile.

## Stellar Surge
- Type: StellarSurge
- Conditions: OnHealthRegen
- Chance: 20%
- Cooldown: 0s
- Delay: 0s
- HP Percent: 6%
- Description: 20% chance on any attack to heal for 6% of max HP on every health regen tick.

## Supersonic
- Type: SpawnProjectile
- Conditions: OnDash
- Chance: 100%
- Cooldown: 1s
- Delay: 0s
- Projectile: Supersonic projectile
- Description: Spawns a Supersonic projectile on dashing. 1s cooldown.
