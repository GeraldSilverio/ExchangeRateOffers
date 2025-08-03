namespace ExchangeRateOffers.Mock1.Services
{
    public interface IExchangeRateService
    {
        decimal GetExchangeRate(string fromCurrency, string toCurrency);
        decimal CalculateConvertedAmount(string fromCurrency, string toCurrency, decimal amount);
    }
}
