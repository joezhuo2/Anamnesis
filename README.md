<div align="center">

# Anamnesis

> A 2D wave-based action roguelite built in Unity 6. Survive escalating hordes, draft rewards between waves, gamble on corruption and anomalies, and rebuild your power through a 210-node skill tree — every attack, effect, upgrade and wave authored as ScriptableObject data.

![Unity](https://img.shields.io/badge/Unity-6000.4.6f1-000000?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET-512BD4?logo=dotnet&logoColor=white)
![URP](https://img.shields.io/badge/URP_2D-17.4-222C37?logo=unity&logoColor=white)
![Input System](https://img.shields.io/badge/Input_System-1.19-4A90D9)
![Cinemachine](https://img.shields.io/badge/Cinemachine-3.1.7-E0457B)
![Version](https://img.shields.io/badge/version-0.5.2-6366F1)
![License](https://img.shields.io/badge/License-Source--Available-orange)

| [📖 About](./README.md) | [📜 Changelog](./CHANGELOG.md) | [🗺️ Roadmap](./ROADMAP.md) | [📝 Upcoming](./TODO.md) | [👏 Credits](./CREDITS.md) | [⚔️ Game Index](./GAME.md)
| :---: | :---: | :---: | :---: | :---: | :---: |

</div>

Current release: **v0.5.2** — see [CHANGELOG.md](CHANGELOG.md) for release history.

---

## 🔁 Core Loop

1. **Pick a difficulty and a gamemode** — **Easy**, **Normal** or **Hard**, optionally **Ironman**, then **Regular** (escalating sequence) or **Unlimited** (infinite scaling, periodic bosses, endless rewards).
2. **Survive the wave** — enemies scale exponentially, split on death, and gain extra spawns every 10 waves, with boss waves along the way.
3. **Choose a reward** — buffs, rare attacks or treasure-pool Awakenings. Reroll, pay 200g when out of rerolls, or corrupt the rewards for a bigger gamble.
4. **Face anomalies** — optional wave modifiers (*Time Trial*, *No Hit*, *Augment*, *Swarm*, *Duel*) that trade risk for rerolls and skill points.
5. **Spend skill points and gold** — unlock skill tree nodes, refund them for gold, level up from XP, and repeat.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| **🎚️ Difficulty** | Easy / Normal / Hard as `DifficultyData` assets of additive offsets (enemy level, counts, rewards, corruption, rerolls, pre-run free picks). The tooltip lists only non-zero offsets; the choice persists to `settings.json` |
| **💀 Ironman Mode** | Home-screen toggle that removes every take-back: 0 rerolls, no corruption, no skill node refunds |
| **🌊 Wave System** | Scriptable sequences, boss waves with boss bars, a live progress indicator (`Wave 7/68 (12/30)`), and an **Unlimited** mode that scales level, counts and spawn rate forever |
| **🌀 Anomalies** | `AnomalyData` modifiers with wave ranges and a `disallowOnBossWave` flag. *Swarm* weakens enemies but multiplies their count; *Duel* collapses the wave into one buffed enemy |
| **🎲 Rewards & Corruption** | Randomized buffs, rare attacks and Awakenings with wave gating, milestone bundles every 25 waves, and once-per-wave corruption with a 4% chance of a *Corrupted* special attack |
| **⚔️ Data-Driven Attacks** | `AttackData` with projectile patterns (circle, spread, barrage, converging lines), wave/spiral/boomerang/follow-cursor paths, orbit interactions, summons, resource costs and chained on-hit attacks |
| **⏳ Cast & Charge** | Interruptible cast times with a pooled cast bar, and hold-to-sustain charged attacks that drain cost per tick and re-snapshot damage mid-hold |
| **✨ Awakenings** | `PlayerUpgrade` assets driven by 22 trigger conditions with chance/cooldown/delay, or passive via `OnUnlock` / `OnRemove` |
| **🌳 Skill Tree** | Pan/zoom tree of 210 nodes with bidirectional (OR) connections, incompatible nodes, gold refunds, **Refund All**, and capstones that upgrade an owned attack or Awakening in place |
| **🧪 Status Effects** | Stackable DoTs, stuns, stat buffs and reductions, attack replacement and cleansing, with cooldown UI |
| **👹 Enemies** | Splitting on death, HP-threshold phases that buff stats and unlock attacks, a global spawner, and a four-boss **Boss Rush** gauntlet |
| **❤️ Resources** | Health, stamina and mana, dash, knockback with resistance, and an **overhealth** pool spent before HP |
| **📈 Progression** | XP and gold drops with 15% variance, level-up stat gains and skill points, and a Stealing stat that boosts gold |
| **⚙️ Settings & Menus** | `Escape` pause panel with gameplay toggles, interactive keyboard rebinding, a restart confirmation, quit buttons, a *You Died* screen, and the build version on the home screen — all persisted to `settings.json` |
| **🖱️ UI Polish** | Floating damage/XP/gold numbers, `1.2k` / `3.4M` bar readouts, red borders and flashes on blocked attacks, and unscaled hover scaling that animates while paused |

Full detail for every system lives in [GAME.md](GAME.md).

---

## 🎮 Controls

| Action | Binding |
|--------|---------|
| Move | `WASD` / Arrow keys |
| Basic attack | `Left Click` / `Space` |
| Charge an attack | Hold its binding (chargeable attacks only) |
| Skill | `E` / `1` |
| Ultimate | `R` / `2` |
| Dash | `Q` / `Right Click` |
| Toggle skill tree | `K` |
| Close skill tree / Settings menu | `Escape` |
| Skill tree pan | Drag (Alt+Left / Alt+Right / Middle) |
| Skill tree zoom | Mouse wheel (zoom-to-cursor) |

Every keyboard binding except skill tree pan/zoom can be rebound in the settings menu; `Escape` itself is fixed.

---

## 📚 Content

| Category | Entries |
|----------|---------|
| **Basic Attacks** | Blaze, Lacerate, Aphelion, Astral Nova, Blood Pact, Ignition Flash, Supernova |
| **Skills** | Warp, Cyclone Cleave, Meteor Shower, Nebula, Stellar Maelstrom, Lifeforce, Sacred Surge |
| **Ultimates** | Nirvana, Revelation, Shattered Singularity, Solar Collapse, Starfury, Exodus, Luminaria, Nocturnis |
| **Awakenings** | Reminiscence, Serenade, Feedback Loop, Soul Rend, Supersonic, Hex Cast, Stellar Surge, Starlit Reflexes, Paradox, Decoy, Hypercarry, Autopilot, Exsanguinate, Terminal Cascade, Cresendo, Tempo, plus capstone-only Solar Wind and Oblivion |
| **Capstones** | Warp and Hypernova upgrade their required attack; Decoy Upgraded, Solar Wind and Oblivion upgrade their required Awakening |
| **Enemies** | Bat, Crab, Slime, Slime (Frost), Slime (Magma) |
| **Bosses** | Cultist (clone summoning), Jellyfish, Lich, Golem (phase-gated moveset), The Grim Reaper (in progress, not yet in wave sets) |
| **Boss Rush** | `BossRush` (Lv 70 Lich → Jellyfish → Cultist → Golem) chaining into `BossRush Part 2` (same four at Lv 105) |
| **Anomalies** | *Time Trial I-IV*, *No Hit*, *Augment*, *Swarm*, *Duel* — separate Regular and Unlimited lists |
| **Upgrade Effects** | Add Chain, Additional Damage, Cooldown Advance, Decoy, Gain Mana, Grant Status Effect, Hex Cast, Overhealth, Paradox, Reminiscence, Soul Rend, Spawn Projectile, Stellar Surge |

In Unlimited waves the roster unlocks as the run goes: Slime from wave 0, Crab from 5, Slime (Magma) from 10, Bat from 15, Slime (Frost) from 20. *Time Trial* and *No Hit* can be failed; *Augment*, *Swarm* and *Duel* always pay out, and *Swarm* / *Duel* never appear before a boss wave.

### Awakening trigger conditions

Every `PlayerUpgrade` asset lists one or more `TriggerCondition` values, plus a chance, cooldown and delay. `PlayerUpgradeManager` rolls the chance and checks the cooldown once per condition match, then calls one of the `TriggerUpgradeEffect` overloads — so an upgrade only responds to a condition if it overrides the overload that condition dispatches to.

| Condition | Fires when | Overload |
|-----------|------------|----------|
| `OnAttack` | Any attack is performed | `(player)` |
| `OnBasicAttack` | A Basic attack is performed | `(player)` |
| `OnSkillAttack` | A Skill attack is performed | `(player)` |
| `OnUltAttack` | An Ultimate attack is performed | `(player)` |
| `OnCalculateAttackCost` | An attack's resource costs are about to be paid | `(player)` |
| `OnSpawnProjectile` | A projectile the player owns is spawned, once per projectile | `(player, spawnCenter)` |
| `OnProjectileHit` | A projectile the player owns hits a target | `(player, hitPosition)` |
| `OnDealDamage` | A damage instance the player owns lands | `(player, target, damageDealt)` |
| `OnTargetRecievedHit` | A target takes damage from the player | `(player)` |
| `OnCrit` | A critical damage instance the player owns lands | `(player)` |
| `OnOverkill` | The player's killing blow is at least 3× the target's remaining HP | `(player)` |
| `OnKill` | An entity dies to damage the player dealt | `(player)` |
| `OnTakeDamage` | The player takes any damage — direct hits, DoT ticks and health costs alike | `(player)` |
| `OnTakeHit` | The player is hit directly by an enemy. Excludes DoT ticks, health costs (`Consume`), heals, and any packet that bypasses i-frames | `(player)` |
| `OnCounterDodge` | The player is hit while immune and dashing | `(player)` |
| `OnStartDash` | A dash begins | `(player)` |
| `OnEndDash` | A dash ends | `(player)` |
| `OnHealthRegen` | Passive health regen ticks for at least 1 HP | `(player)` |
| `OnStaminaRegen` | Passive stamina regen ticks for at least 1 stamina | `(player)` |
| `OnManaRegen` | The player actually gains mana. There is no passive mana regen tick, so this covers every mana gain | `(player)` |
| `OnLevelUp` | The player gains a level. A single XP pickup that crosses several thresholds fires once per level | `(player)` |
| `OnDeath` | The player dies, before the death sequence tears the object down. An upgrade whose `delay` outlasts the death animation is cut off | `(player)` |

- `OnSpawnProjectile` and `OnManaRegen` are reentrancy-guarded, but only for the immediate call — an upgrade with a non-zero `delay` that re-triggers its own condition still needs a cooldown.
- A non-zero `delay` on a positional condition drops the position and calls the plain `(player)` overload, so position-sensitive upgrades should leave `delay` at 0.
- Passive Awakenings leave `conditions` empty and install their effect in `OnUnlock`, reversing it in `OnRemove` — Exsanguinate and Terminal Cascade both work this way.

---

## 🏗️ Tech Stack

| Category | Technologies |
|----------|--------------|
| **Engine** | Unity `6000.4.6f1` (Unity 6), C# |
| **Rendering** | Universal Render Pipeline 2D `17.4` |
| **Input** | Input System `1.19` (`PlayerControls.inputactions`) with interactive rebinding |
| **Camera** | Cinemachine `3.1.7` |
| **UI** | uGUI + TextMeshPro |
| **Data** | ScriptableObjects (`AttackData`, `PlayerUpgrade`, `StatusEffect`, `DifficultyData`, `AnomalyData`, skill tree nodes) |
| **Contracts** | [CrystalFlux-Core](https://github.com/joezhuo2/CrystalFlux-Core) — interfaces, asset bases and shared value types, imported as a git package |
| **Architecture** | Per-system assembly definitions, prefab-keyed object pooling (`PrefabPool` + `IPoolable`) |
| **Persistence** | `GameSettings` JSON (`settings.json` in the persistent data path) |

---

## 🚀 Quick Start

### Prerequisites
- **Unity 6000.4.6f1** (or newer) via Unity Hub
- Git — Unity resolves `com.crystalflux.core` from its GitHub URL on first open

### Installation

```bash
git clone https://github.com/joezhuo2/Anamnesis.git
```

1. In Unity Hub, **Add → Add project from disk** and select the cloned folder.
2. Open the main scene: `Assets/New.unity`.
3. Press **Play**, or **File → Build and Run** (`Ctrl + B`).

`Core` is imported automatically from `Packages/manifest.json`:

```json
"com.crystalflux.core": "https://github.com/joezhuo2/CrystalFlux-Core.git#3284d28ba9782972e69eab548cfe807bc3e589b8"
```

> **Note:** the build settings must include `Assets/New.unity` — `Assets/data/Scenes/SampleScene.unity` is an empty placeholder scene.

---

## 📁 Project Structure

```
Assets/
├── New.unity                  # Main game scene (WaveManager, SkillTree, Player UI)
├── data/                      # ScriptableObject data
│   ├── _example/              # Template attack folder (AD/PD/controller/prefab) to copy when authoring
│   ├── Difficulty/            # Easy / Normal / Hard DifficultyData assets
│   ├── entity/                # Enemy/Player base stats, attacks, animation data, prefabs
│   │   └── enemy/             # Split into Bosses/ and Enemies/
│   ├── images/                # Image assets
│   ├── PlayerData/            # Player attacks, upgrades, skill tree data, controls
│   ├── prefabs/               # UI element prefabs
│   ├── StatusEffect/          # Authored status effects (DoTs, stuns, buffs, debuffs)
│   └── Wave/                  # Wave sequences (waves/) and anomalies (Anomaly/Regular, Anomaly/Unlimited)
└── scripts/
    ├── Entity/                # [asmdef] Player, Enemy, stats, health, levelling, summoning, XP
    │   ├── Enemy/             # Enemy AI, movement, attack handlers, spawner, stats
    │   └── Player/            # Player movement, attack, resources, UI, upgrades, level
    ├── Items/                 # Items/Gear system (Assembly-CSharp)
    ├── Misc/                  # Game Controller (IAnnouncer), settings menu, death screen, restart (Assembly-CSharp)
    ├── Pooling/               # [asmdef] PrefabPool + IPoolable — shared prefab-keyed object pool
    ├── Projectile/            # [asmdef] Projectiles, attack data, damage calculator
    ├── Settings/              # [asmdef] GameSettings — persisted toggles and keybind overrides
    ├── StatusEffect/          # [asmdef] Status effect system and implementations
    ├── SkillTree/             # [asmdef] Skill tree manager (ISkillPointHolder), UI, pan/zoom, connections
    ├── TextIndicator/         # [asmdef] Floating damage numbers, XP/Gold indicators
    └── Wave/                  # [asmdef] WaveManager, UnlimitedWaveManager, rewards, anomalies, difficulty and Ironman selectors

Packages/
└── com.crystalflux.core       # (git) Contracts: IDamageable, IResourcePool, IStatusEffectReceiver, AttackAsset, DamagePacket, StatType, …
```

### Assembly Boundaries

Each `[asmdef]` folder compiles to its own assembly, so cross-system dependencies are enforced by the compiler rather than by convention:

```
                  ┌─ Projectile
                  ├─ StatusEffect
Core (package) ───┼─ SkillTree
                  ├─ Wave
                  └─ Entity ──→ Projectile, StatusEffect, SkillTree, TextIndicator

                  ┌─ Projectile
                  ├─ StatusEffect
Pooling ──────────┼─ Wave
                  ├─ TextIndicator
                  └─ Entity

TextIndicator ── (references Pooling and TextMeshPro only)

Settings ─────────┬─ Entity
                  └─ Wave
```

| Assembly | Rule |
|----------|------|
| **Core** | References nothing. Contracts only — interfaces, abstract `ScriptableObject` bases, shared value types — shipped as its own package |
| **Pooling** | References nothing. `PrefabPool` and `IPoolable`, used by every assembly that spawns something |
| **Settings** | References nothing. `GameSettings` and its bootstrap, read by `Entity` and `Wave`; the menu UI lives in `Misc` |
| **Projectile / StatusEffect / SkillTree / Wave** | Never reference each other — only `Core`, plus `Pooling` where they spawn. A cross-reference is a compile error |
| **Entity** | The only assembly that composes the leaf systems |
| **TextIndicator** | References only `Pooling` and TextMeshPro |
| **Items / Misc** | Stay in `Assembly-CSharp`, which auto-references everything above |

**Design notes**

- Types shared across a boundary live in `Core` as abstract bases (`AttackAsset`, `UpgradeAsset`, `EffectAsset`) rather than interfaces, because Unity cannot serialize interface-typed asset fields.
- `Wave` never names a concrete system type — it talks to the `Core` interfaces those systems implement, plus the `EnemySpawning` and `PlayerEvents.OnPlayerTakeDamage` hooks.
- Reward tooltips come from each asset's own `GetTooltipLines`, so `RewardButton` never reads concrete data fields.
- Attack data is **shared, not cloned**: `AttackData` and `ProjectileData` fields are private behind read-only properties. Per-run changes register on the owner instead, via `IAttackEffectSource` on `PlayerUpgradeManager`.

> **Moving a `[SerializeReference]` type between assemblies breaks existing assets.** Unity stores a literal `{class, ns, asm}` triplet, so add `[MovedFrom(sourceAssembly: "...")]` when relocating one — see `UnlockEffect` and `NodeRequirement`.

---

## 📄 License

**Anamnesis Source-Available License** — the source is public to read and learn from, but it is not open source and is not licensed for redistribution. See [LICENSE](LICENSE).
