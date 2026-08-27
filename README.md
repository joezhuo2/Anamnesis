<div align="center">

# Anamnesis

**A 2D wave-based action roguelite built in Unity 6**

*Anamnesis* — the recollection of memories. Survive endless waves of enemies, collect rewards, and rebuild your power through a branching skill tree. Choose between **Regular** waves or an **Unlimited** endless mode at the start of each run.

</div>

| [📖 About](./README.md) | [📜 Changelog](./CHANGELOG.md) | [🗺️ Roadmap](./ROADMAP.md) | [📝 Upcoming](./TODO.md) | [👏 Credits](./CREDITS.md) | [⚔️ Game Index](./GAME.md)
| :---: | :---: | :---: | :---: | :---: | :---: |

---

## About

Anamnesis is a top-down, wave-based action game currently in active development. Each wave throws escalating hordes of enemies at you while you weave together basic attacks, skills, and ultimates. Between waves you pick from randomized rewards — stat buffs, rare attacks, and powerful player upgrades — and spend skill points on a persistent skill tree.

At the start of each run you pick a **gamemode**: **Regular** waves follow the standard escalating sequence, while **Unlimited** waves scale infinitely with faster spawns, periodic boss waves, and endless rewards.

The game is built entirely with **ScriptableObject-driven data** (attacks, status effects, upgrades, skill tree nodes), so most content is data-authored and tuned in the Unity inspector.

## Core Loop

1. **Pick a gamemode** — choose **Regular** waves (standard escalating sequence) or **Unlimited** waves (infinite scaling, faster spawns, periodic boss waves, endless rewards).
2. **Survive the waves** — enemies spawn in escalating sequences with occasional boss encounters. Enemies can even **split** into more enemies on death, and extra spawns occur every 10 waves. Enemies scale exponentially.
3. **Choose a reward** — pick from buffs, special attacks, or treasure-pool Awakenings, using limited rerolls effectively. Some are even brave enough to "corrupt" the rewards, risking it all for a greater reward. Reroll with **gold** (200g) when out of rerolls.
4. **Face anomalies** — random world modifiers (e.g. *Time Trial*, *No Damage*, *Stat Modifier*, and more) that add risk for greater reward.
5. **Spend skill points** — unlock nodes on the skill tree to permanently empower the run. Gain skill points from levelling up, occasionally on wave clears and every 5 waves. Refund nodes using gold (default 50g).
6. **Level Up & Earn Gold** — collect experience from enemies to level up, gaining stat boosts and skill points along the way. Enemies drop XP (common enemies drop less, bosses drop more with 15% variance). **Enemies also drop gold (15% variance), increased by the Stealing stat.**
7. **Repeat** — waves get harder, and you get stronger.

## Features

- **Gamemode selector** — choose between **Regular** and **Unlimited** waves at the start of each run via dedicated buttons (with tooltips). Player actions are enabled in the lobby.
- **Wave system** — scriptable wave sequences, escalating spawns, extra enemy spawns every 10 waves, boss bars, and reward/anomaly button panels that update dynamically.
- **Unlimited waves** — an endless mode that scales infinitely: enemy level and max total enemies rise each wave, spawns speed up, boss waves appear periodically, and rewards never stop. Reuses all shared `WaveManager` settings (reroll cost, rewards, corruption, milestones, anomalies) with no reconfiguration.
- **Enemy splitting** — enemies can split into more enemies on death with configurable split count, health scaling, and behavior settings.
- **Global enemy spawner** — centralized spawning system for consistent enemy management.
- **Anomaly system** — randomized run modifiers with configurable frequency, counts, and reward bonuses (e.g., *Time Trial*, *No Damage*, *Stat Modifier*).
- **Data-driven attacks** — `AttackData` ScriptableObjects with projectile patterns (circle, spread, barrage, spread barrage), resource costs (stamina / mana / health), on-hit resource gains, summoning, boomerang travel patterns, orbit interactions (fire, absorb, redirect, explode), and **follow-source** option for projectiles.
- **Status effects** — stackable DoTs, stuns, stat buffs/reductions, attack replacement, and more, with cooldown UI.
- **Awakenings** — trigger-condition-based `PlayerUpgrade` ScriptableObjects (on attack, on crit, on hit, on dash, on deal damage, …) with chance/cooldown/delay.
- **Skill tree** — interactive pan/zoom tree with a **bidirectional connections system** (OR logic), incompatible nodes, tooltips, connector lines, and a skill-point currency. Nodes connect via the `prerequisites` field; unlocking works both ways (A→B means unlock A if B unlocked OR unlock B if A unlocked) and only **one** connected node needs to be unlocked. Left-click unlocked nodes to refund using **gold** (default 50g, configurable per node).
- **Corruption system** — once per wave, corrupt rewards for a chance at massive stat boosts (up to +80%) or severe penalties (down to -180%).
- **Milestone rewards** — every 25 waves (25, 50, 75, 100...), choose from 3 synergistic reward bundles that combine powerful buffs with meaningful drawbacks (e.g., *Glass Cannon*: +40% Damage / -40% Max Health). Each stat has ±15% variance for replayability.
- **Title system** — game title/subtitle with fade in/out, plus wave-complete and boss-killed title displays.
- **Resources** — health, stamina, and mana with dash, knockback, and cooldown systems.
- **Knockback** — full knockback for players and enemies, with knockback resistance and increased knockback stats.
- **Damage indicators** — floating damage numbers with small randomness. **XP gain indicators** and **XP wrapper option** for custom XP display. **Gold gain indicators** (+{gold}g in gold color).
- **Levelling system** — enemies drop XP, players collect XP to level up and gain stat buffs (HP, ATK, INT, SPD) and skill points. **Level-up indicator** on progression.
- **Gold system** — enemies drop gold on death (15% variance, same as XP). **Stealing stat** increases gold drops by {stealing}%. Spend gold to reroll rewards (200g when out of rerolls) or refund skill nodes.

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
- **Bosses**: Cultist (clone summoning), Jellyfish, Lich

Each enemies have their own stats, attack sets, movement patterns, behavior, and inflict unique status effects. Some even have unique behaviour such as summons, and more to come! 

**Status effects** — DoTs, Stun, Stat Buffs, Stat Reductions (Slow, Weaken, etc.), Attack Enhancements,and more.

**Player upgrades** — Additional Damage, Cooldown Advance, Decoy, Gain Mana, Hex Cast, Paradox, Reminiscence, Soul Rend, Spawn Projectile, Stellar Surge.

## Tech Stack

- **Engine:** Unity `6000.4.6f1` (Unity 6)
- **Rendering:** Universal Render Pipeline (2D)
- **Input:** New Input System (`PlayerControls.inputactions`)
- **UI:** uGUI + TextMeshPro

## Project Structure

```
Assets/
├── New.unity                  # Main game scene (WaveManager, SkillTree, Player UI)
├── data/                      # ScriptableObject data (attacks, entities, waves, skill tree)
│   ├── entity/                # Enemy/Player base stats, attacks, animation data, assets, and prefabs
│   ├── images/                # Images assets
│   ├── PlayerData/            # Player attacks, upgrades, skill tree data, controls
│   ├── prefabs/               # UI element prefabs
│   └── WaveData/              # Wave sequences
└── scripts/
    ├── Core/                  # Interfaces
    ├── Entity/                # Player, Enemy, stats, health, levelling, summoning, XP
    ├── Items/                 # items, gear
    ├── Misc/                  # Game Controller
    ├── Projectile/            # Projectiles/Attack data, damage calculator
    ├── StatusEffect/          # Status effect system & implementations
    ├── SkillTree/             # Skill tree manager, UI, pan/zoom
    ├── TextIndicator/         # Floating damage numbers, XP/Gold indicators
    ├── Wave/                  # WaveManager, UnlimitedWaveManager, rewards, anomalies, enemy spawner
```

## Getting Started

1. Open the project in **Unity 6000.4.6f1** (or newer).
2. Open the main scene: `Assets/New.unity`.
3. Press **Play** or **File > Build and Run (`Ctrl + B`)**

> **Note:** The build settings must include `Assets/New.unity` — `Assets/data/Scenes/SampleScene.unity` is an empty placeholder scene.

## Roadmap

See [ROADMAP.md](ROADMAP.md) for summary of major feature updates.

## Planned Features

See [TODO.md](TODO.md) for upcoming content and rebalances.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for the full release history.

## License

All rights reserved. This project is a personal work-in-progress and is not licensed for redistribution.
