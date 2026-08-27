public interface IResourcePool 
{
    bool TrySpend(ResourceType type, float amount);
    bool TryGain(ResourceType type, float amount);
}