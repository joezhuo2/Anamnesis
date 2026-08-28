using CrystalFlux.Core;

namespace CrystalFlux.EntitySystem
{
    public interface IResourcePool
    {
        bool TrySpend(ResourceType type, float amount);
        bool TryGain(ResourceType type, float amount);
    }
}