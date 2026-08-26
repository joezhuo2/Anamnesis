public interface IResourcePool 
{
    bool TrySpend(ResourceType type, float amount);
    void Gain(ResourceType type, float amount);
}