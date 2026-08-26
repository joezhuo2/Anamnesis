public interface ICurrencyHolder
{
    bool TrySpend(int amount);
    int CurrentAmount { get; }
}