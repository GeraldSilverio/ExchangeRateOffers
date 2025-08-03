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

            _logger.LogInformation("Loaded {Count} exchange rates for API2", _exchangeRates.Count);
        }

        public decimal GetConvertedAmount(string fromCurrency, string toCurrency, decimal amount)
        {
            var key = $"{fromCurrency}_{toCurrency}";

            decimal rate;
            if (_exchangeRates.TryGetValue(key, out rate))
            {
                _logger.LogDebug("Found rate for {Key}: {Rate}", key, rate);
            }
            else
            {
                var reverseKey = $"{toCurrency}_{fromCurrency}";
                if (_exchangeRates.TryGetValue(reverseKey, out var reverseRate))
                {
                    rate = 1 / reverseRate;
                    _logger.LogDebug("Calculated reverse rate for {Key}: {Rate}", key, rate);
                }
                else
                {
                    _logger.LogWarning("Exchange rate not found for {Key}", key);
                    throw new ArgumentException($"Exchange rate not available for {fromCurrency} to {toCurrency}");
                }
            }

            var convertedAmount = amount * rate;

            _logger.LogDebug("Converted {Amount} {From} to {ConvertedAmount} {To} at rate {Rate}",
                amount, fromCurrency, convertedAmount, toCurrency, rate);

            return Math.Round(convertedAmount, 2);
        }
    }
}
