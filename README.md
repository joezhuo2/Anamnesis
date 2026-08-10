<div align="center">

# Anamnesis

**A 2D wave-based action roguelite built in Unity 6**

*Anamnesis* (ἀνάμνησις) — the recollection of memories. Survive endless waves of enemies, collect rewards, and rebuild your power through a branching skill tree.

</div>

---

## About

Anamnesis is a top-down, wave-based action game currently in active development. Each wave throws escalating hordes of enemies at you while you weave together basic attacks, skills, and ultimates. Between waves you pick from randomized rewards — stat buffs, rare attacks, and powerful player upgrades — and spend skill points on a persistent skill tree.

The game is built entirely with **ScriptableObject-driven data** (attacks, status effects, upgrades, skill tree nodes), so most content is data-authored and tuned in the Unity inspector.

## Core Loop

1. **Survive the waves** — enemies spawn in escalating sequences with occasional boss encounters.
2. **Choose a reward** — pick from buffs, special attacks, or treasure-pool Awakenings, using limited rerolls effectively.
3. **Face anomalies** — random world modifiers (e.g. *Time Trial*, *No Damage*, and more to come) that add risk for greater reward.
4. **Spend skill points** — unlock nodes on the skill tree to permanently empower the run.
5. **Repeat** — waves get harder, and you get stronger.

## Features

- **Wave system** — scriptable wave sequences, escalating spawns, extra enemy spawns every 10 waves, boss bars, and reward/reroll panels.
- **Anomaly system** — randomized run modifiers with configurable frequency, counts, and reward bonuses.
- **Data-driven attacks** — `AttackData` ScriptableObjects with projectile patterns (circle, spread, barrage), resource costs (stamina / mana / health), on-hit resource gains, summoning, boomerang travel patterns, and **orbit interactions** (fire, absorb, redirect, explode).
- **Status effects** — stackable DoTs, stuns, stat buffs/reductions, attack replacement, and more, with cooldown UI.
- **Awakenings** — trigger-condition-based `PlayerUpgrade` ScriptableObjects (on attack, on crit, on hit, on dash, on deal damage, …) with chance/cooldown/delay.
- **Skill tree** — interactive pan/zoom tree with prerequisites, incompatible nodes, tooltips, connector lines, and a skill-point currency.
- **Resources** — health, stamina, and mana with dash, knockback, and cooldown systems.
- **Knockback** — full knockback for players and enemies, with knockback resistance and increased knockback stats.
- **Damage indicators** — floating damage numbers with small randomness.

## Controls

| Action | Binding |
| --- | --- |
| Move | `WASD` / Arrow keys |
| Basic attack | `Left Click` / `Space` |
| Skill | `E` / `1` |
| Ultimate | `R` / `2` |
| Dash | `Q` / `Right Click` |
| Toggle skill tree | `K` |
| Skill tree pan | Drag (Alt+Left / Alt+Right / Middle) |
| Skill tree zoom | Mouse wheel (zoom-to-cursor) |

## Content

**Player attacks** — 
- **Basic Attacks**: Blaze, Lacerate, Aphelion, Astral Nova, Blood Pact
- **Skill**: Warp, Cyclone Cleave, Meteor Shower, Nebula, Stellar Maelstrom, Supernova
- **Ultimate**: Nirvana, Revelation, Shattered Singularity, Solar Collapse, Starfury, Exodus
- **Awakenings**: Reminiscence, Serenace, Feedback Loop, Soul Rend, Supersonic, Hex Cast, Stellar Surge, Starlit Reflexes, Paradox, Cosmic Afterimage, Hypercarry

**Enemies** — 
- **Regular Enemies**: Bat, Crab, and Slime
- **Bosses**: Cultist (clone summoning), Jellyfish, Lich, 

Each enemies have their own stats, attack sets, movement patterns, behavior, and inflict unique status effects. Some even have unique behaviour such as summons, and more to come! 

**Status effects** — DoTs, Stun, Stat Buffs, Stat Reductions (Slow, Weaken, etc.), Attack Enhancements,and more.

**Player upgrades** — Additional Damage, Cooldown Advance, Decoy, Gain Mana, Hex Cast, Paradox, Reminiscence, Soul Rend, Spawn Projectile, Stellar Surge.

## Tech Stack

- **Engine:** Unity `6000.4.6f1` (Unity 6)
- **Rendering:** Universal Render Pipeline (2D)
- **Input:** New Input System (`PlayerControls.inputactions`)
- **UI:** uGUI + TextMeshPro
- **Versioning:** [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) + [Semantic Versioning](https://semver.org/spec/v2.0.0.html)

## Project Structure

```
Assets/
├── New.unity                  # Main game scene (WaveManager, SkillTree, Player UI)
├── data/                      # ScriptableObject data (attacks, entities, waves, skill tree)
│   ├── attacks/               # Player & enemy AttackData
│   ├── entity/                # Enemy stats & data
│   ├── prefabs/               # Player, enemies, UI prefabs
│   ├── PlayerUpgrade/         # Upgrade ScriptableObjects
│   ├── SkillTree/             # Skill tree definition & node assets
│   └── WaveData/              # Wave sequences
└── scripts/
    ├── Entity/                # Player, Enemy, stats, health, summoning, gear
    ├── Projectile/            # Projectiles, damage calculator, attack data
    ├── StatusEffect/          # Status effect system & implementations
    ├── SkillTree/             # Skill tree manager, UI, pan/zoom
    ├── Wave/                  # WaveManager, rewards, anomalies
    ├── Items/                 # Item & gear definitions
    └── DamageIndicator/       # Floating damage numbers
```

## Getting Started

1. Open the project in **Unity 6000.4.6f1** (or newer).
2. Open the main scene: `Assets/New.unity`.
3. Press **Play**.

> **Note:** The build settings must include `Assets/New.unity` — `Assets/data/Scenes/SampleScene.unity` is an empty placeholder scene.

## Roadmap

- [x] Core wave-based combat loop
- [x] Reward & anomaly systems
- [x] Knockback, dash, and resource systems
- [x] Skill tree (v0.1.0 Skill Tree Update)

## Planned Features

See [TODO.md](TODO.md) for upcoming content and rebalances.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for the full release history.

## License

All rights reserved. This project is a personal work-in-progress and is not licensed for redistribution.
