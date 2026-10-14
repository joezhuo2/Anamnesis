using System;
using System.Collections.Generic;
using CrystalFlux.Core;

namespace CrystalFlux.SaveSystem
{
    public enum SaveKind { Build, Run }
    public enum WaveManagerKind { Regular, Unlimited }

    [Serializable]
    public class SaveMeta
    {
        public const int CurrentFormat = 1;

        public int formatVersion = CurrentFormat;
        public string gameVersion = "";
        public SaveKind kind;
        public string id = "";
        public string name = "";
        public string savedAtUtc = "";
        public int wave;
        public WaveManagerKind waveManagerKind;
        public string difficultyName = "";
        public string modeName = "";
        public bool ironman;
        public bool readOnly;
        public float peakDps;
        public float playtimeSeconds;
        public List<string> attackNames = new();
        public List<string> referencedIds = new();
    }

    [Serializable]
    public class AttackSlot
    {
        public AttackType type;
        public string id = "";
    }

    [Serializable]
    public class SkillTreeState
    {
        public List<string> unlockedOrder = new();
        public int skillPoints;
        public bool choseStarting;
    }

    [Serializable]
    public class SynergyState
    {
        public StatType source;
        public StatType target;
        public float pct;
        public float applied;
    }

    [Serializable]
    public class ContractState
    {
        public string anomalyId = "";
        public UnityEngine.Random.State creationState;
    }

    [Serializable]
    public class BuildData
    {
        public string statsJson = "";
        public List<AttackSlot> attacks = new();
        public List<string> upgradeIds = new();
        public SkillTreeState skillTree = new();
        public List<SynergyState> synergies = new();
        public bool hasContract;
        public ContractState contract = new();
        public float peakDps;
        public float playtimeSeconds;
    }

    [Serializable]
    public class RunData
    {
        public WaveManagerKind waveManagerKind;
        public string sequenceId = "";
        public int waveIndex;
        public string difficultyId = "";
        public string modeId = "";
        public bool ironman;
        public int rerolls;
        public float additionalQuality;
        public float synergyChanceBonus;
        public List<string> availableRareIds = new();
        public List<string> availableTreasureIds = new();
        public List<string> availableCorruptionSpecialIds = new();
        public int pendingOccasionalRerolls = -1;
        public int pendingOccasionalSkillPoints;
        public int pendingAnomalyRerolls = -1;
        public int pendingAnomalySkillPoints = -1;
        public int unlimitedLastBossWave;
        public int unlimitedMaxTotalEnemies;
    }

    [Serializable]
    public class SaveFile
    {
        public SaveMeta meta = new();
        public BuildData build = new();
        public RunData run = new();
    }
}
