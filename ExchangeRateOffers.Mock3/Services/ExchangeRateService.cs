namespace ExchangeRateOffers.Mock3.Services
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

            _logger.LogInformation("Cargadas {Count} tasas de cambio para API3", _exchangeRates.Count);
        }

        public decimal GetConvertedTotal(string sourceCurrency, string targetCurrency, decimal quantity)
        {
            var key = $"{sourceCurrency}_{targetCurrency}";
            decimal rate;

            if (_exchangeRates.TryGetValue(key, out rate))
            {
                _logger.LogDebug("Tasa encontrada para {Key}: {Rate}", key, rate);
            }
            else
            {
                _logger.LogWarning("Tasa de cambio no encontrada para {Key}", key);
                throw new ArgumentException($"Tasa de cambio no disponible para {sourceCurrency} a {targetCurrency}");
            }

            var total = quantity * rate;
            _logger.LogDebug("Convertido {Quantity} {Source} a {Total} {Target} con tasa {Rate}",
                quantity, sourceCurrency, total, targetCurrency, rate);

            return Math.Round(total, 2);
        }
    }
}