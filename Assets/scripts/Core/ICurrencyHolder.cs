public interface ICurrencyHolder
{
    bool TrySpend(int amount);
    bool AddCurrency(int amount);
    int CurrentAmount { get; }
}