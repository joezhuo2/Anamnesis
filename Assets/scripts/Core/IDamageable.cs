public interface IDamageable
{
    void TakeDamage(DamagePacket packet);
    bool IsAlive { get; }
}