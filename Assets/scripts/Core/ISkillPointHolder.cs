public interface ISkillPointHolder
{
    int SkillPoints { get; }
    void AddSkillPoints(int amount);
    bool TrySpend(int amount);
}