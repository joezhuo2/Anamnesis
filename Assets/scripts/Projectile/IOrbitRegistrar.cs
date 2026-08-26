using UnityEngine;

public interface IOrbitRegister
{
    void RegisterOrbitingProjectile(Projectile p);
    void UnregisterOrbitingProjectile(Projectile p);
}