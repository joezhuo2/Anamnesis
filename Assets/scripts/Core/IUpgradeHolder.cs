namespace CrystalFlux.EntitySystem
{
    public interface IUpgradeHolder
    {
        bool HasUpgrade(UpgradeAsset pu);
        void AddUpgrade(UpgradeAsset pu);
        void RemoveUpgrade(UpgradeAsset pu);
    }
}
