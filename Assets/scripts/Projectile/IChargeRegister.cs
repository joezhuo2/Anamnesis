namespace CrystalFlux.ProjectileSystem
{
    public interface IChargeRegister
    {
        void RegisterChargedProjectile(Projectile p);
        void UnregisterChargedProjectile(Projectile p);
    }
}
