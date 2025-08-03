namespace ExchangeRateOffers.Mock1.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly Dictionary<string, decimal> _exchangeRates;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(IConfiguration configuration, ILogger<ExchangeRateService> logger)
        {
            _logger = logger;
            _exchangeRates = new Dictionary<string, decimal>();

            // Cargando las tasas de cambio desde el appsettings
            var ratesSection = configuration.GetSection("ExchangeRates");

            foreach (var rate in ratesSection.GetChildren())
            {
                if (decimal.TryParse(rate.Value, out var rateValue))
                {
                    _exchangeRates[rate.Key] = rateValue;
                }
            }

            _logger.LogInformation("Cantidad de tasas: {Count}", _exchangeRates.Count);
        }

        public decimal GetExchangeRate(string fromCurrency, string toCurrency)
        {
            var key = $"{fromCurrency}_{toCurrency}";

            if (_exchangeRates.TryGetValue(key, out var rate))
            {
                return rate;
            }

            var reverseKey = $"{toCurrency}_{fromCurrency}";
            if (_exchangeRates.TryGetValue(reverseKey, out var reverseRate))
            {
                var calculatedRate = 1 / reverseRate;
                return calculatedRate;
            }

            throw new ArgumentException($"Exchange rate not available for {fromCurrency} to {toCurrency}");
        }

        public decimal CalculateConvertedAmount(string fromCurrency, string toCurrency, decimal amount)
        {
            var rate = GetExchangeRate(fromCurrency, toCurrency);
            var convertedAmount = amount * rate;

            return Math.Round(convertedAmount, 2);
        }
    }
}
