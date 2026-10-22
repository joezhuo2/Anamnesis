using System.Collections.Generic;
using CrystalFlux.Core;

namespace CrystalFlux.WaveSystem
{
    public class RunStateDto
    {
        public WaveSequence sequence;
        public int waveIndex;
        public int rerolls;
        public float additionalQuality;
        public float synergyChanceBonus;
        public List<AttackAsset> availableRare = new();
        public List<UpgradeAsset> availableTreasure = new();
        public List<AttackAsset> availableCorruptionSpecial = new();
        public int pendingOccasionalRerolls = -1;
        public int pendingOccasionalSkillPoints;
        public int pendingAnomalyRerolls = -1;
        public int pendingAnomalySkillPoints = -1;
        public int unlimitedLastBossWave;
        public int unlimitedMaxTotalEnemies;
        public List<MutationState> mutations = new();
        public int lastMutationWave;
        public float mutationBonusChance;
    }
}
