namespace CrystalFlux.Core
{
    public interface IPoolable
    {
        void OnPoolAcquire();
        void OnPoolRelease();
    }
}
