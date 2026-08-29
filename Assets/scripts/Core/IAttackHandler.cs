namespace CrystalFlux.ProjectileSystem
{
    public interface IAttackHandler
    {
        bool HasAttack(AttackAsset a);
        AttackAsset FindAttackOfType(AttackType type);
        void UpdateAttack(AttackType type, AttackAsset newAttack);
        void RemoveAttack(AttackType type);
    }
}
