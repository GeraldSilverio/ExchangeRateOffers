namespace ExchangeRateOffers.Mock2.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly Dictionary<string, decimal> _exchangeRates;
        private readonly ILogger<ExchangeRateService> _logger;

        public ExchangeRateService(IConfiguration configuration, ILogger<ExchangeRateService> logger)
        {
            _logger = logger;
            _exchangeRates = new Dictionary<string, decimal>();

            var ratesSection = configuration.GetSection("ExchangeRates");
            foreach (var rate in ratesSection.GetChildren())
            {
                if (decimal.TryParse(rate.Value, out var rateValue))
                {
                    _exchangeRates[rate.Key] = rateValue;
                }
            }

            _logger.LogInformation("Cargadas {Count} tasas de cambio para API2", _exchangeRates.Count);
        }

        public decimal GetConvertedAmount(string fromCurrency, string toCurrency, decimal amount)
        {
            var key = $"{fromCurrency}_{toCurrency}";
            decimal rate;

            if (_exchangeRates.TryGetValue(key, out rate))
            {
                _logger.LogDebug("Tasa encontrada para {Key}: {Rate}", key, rate);
            }
            else
            {
                _logger.LogWarning("Tasa de cambio no encontrada para {Key}", key);
                throw new ArgumentException($"Tasa de cambio no disponible para {fromCurrency} a {toCurrency}");
            }


            var convertedAmount = amount * rate;
            _logger.LogDebug("Convertido {Amount} {From} a {ConvertedAmount} {To} con tasa {Rate}",
                amount, fromCurrency, convertedAmount, toCurrency, rate);

            return Math.Round(convertedAmount, 2);
        }
    }
}