namespace CrystalFlux.ProjectileSystem
{
    public interface IChargeRegister
    {
        AttackData ActiveChargeSource { get; }
        void RegisterChargedProjectile(Projectile p);
        void UnregisterChargedProjectile(Projectile p);
    }
}
