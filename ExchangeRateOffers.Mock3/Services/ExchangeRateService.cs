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

            _logger.LogInformation("Loaded {Count} exchange rates for API3", _exchangeRates.Count);
        }

        public decimal GetConvertedTotal(string sourceCurrency, string targetCurrency, decimal quantity)
        {
            var key = $"{sourceCurrency}_{targetCurrency}";

            decimal rate;
            if (_exchangeRates.TryGetValue(key, out rate))
            {
                _logger.LogDebug("Found rate for {Key}: {Rate}", key, rate);
            }
            else
            {
                // Try reverse rate
                var reverseKey = $"{targetCurrency}_{sourceCurrency}";
                if (_exchangeRates.TryGetValue(reverseKey, out var reverseRate))
                {
                    rate = 1 / reverseRate;
                    _logger.LogDebug("Calculated reverse rate for {Key}: {Rate}", key, rate);
                }
                else
                {
                    _logger.LogWarning("Exchange rate not found for {Key}", key);
                    throw new ArgumentException($"Exchange rate not available for {sourceCurrency} to {targetCurrency}");
                }
            }

            // API3 returns the converted total amount
            var total = quantity * rate;

            _logger.LogDebug("Converted {Quantity} {Source} to {Total} {Target} at rate {Rate}",
                quantity, sourceCurrency, total, targetCurrency, rate);

            return Math.Round(total, 2);
        }
    }
}