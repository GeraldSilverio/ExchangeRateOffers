namespace ExchangeRateOffers.Mock2.Services
{
    public interface IExchangeRateService
    {
        decimal GetConvertedAmount(string fromCurrency, string toCurrency, decimal amount);
    }
}
