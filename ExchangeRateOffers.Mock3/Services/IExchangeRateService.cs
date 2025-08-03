namespace ExchangeRateOffers.Mock3.Services
{
    public interface IExchangeRateService
    {
        decimal GetConvertedTotal(string sourceCurrency, string targetCurrency, decimal quantity);
    }
}
