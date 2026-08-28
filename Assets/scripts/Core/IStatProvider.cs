namespace CrystalFlux.Core
{
    public interface IStatProvider
    {
        float GetStat(StatType type);
        void AddStat(StatBuff buff, bool isAdding = true);
    }
}
