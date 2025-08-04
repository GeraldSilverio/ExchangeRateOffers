using ExchangeRateOffers.Core.Application.Dtos.InBound;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Core.Application.Validators
{
    /// <summary>
    /// Validador para solicitudes de cambio de divisa
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ExchangeRequestDtoValidator : AbstractValidator<ExchangeRequestDto>
    {
        private readonly IConfiguration _configuration;
        private readonly HashSet<string> _supportedPairs;
        private readonly HashSet<string> _supportedCurrencies;

        public ExchangeRequestDtoValidator(IConfiguration configuration)
        {
            _configuration = configuration;
            _supportedPairs = LoadSupportedPairs();
            _supportedCurrencies = ExtractSupportedCurrencies();
            ConfigureValidationRules();
        }

        private void ConfigureValidationRules()
        {
            RuleFor(x => x.SourceCurrency)
                .NotEmpty()
                .WithMessage("La divisa de origen es requerida")
                .Length(3)
                .WithMessage("La divisa de origen debe tener exactamente 3 caracteres")
                .Must(BeValidCurrency)
                .WithMessage($"Moneda de Origen no soportada.Monedas disponibles: {string.Join(", ", _supportedCurrencies)}");

            RuleFor(x => x.TargetCurrency)
                .NotEmpty()
                .WithMessage("La divisa de destino es requerida")
                .Length(3)
                .WithMessage("La divisa de destino debe tener exactamente 3 caracteres")
                .Must(BeValidCurrency)
                .WithMessage($"Moneda de Origen no Destino.Monedas disponibles: {string.Join(", ", _supportedCurrencies)}");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("El monto debe ser mayor que cero")
                .LessThan(999999999999999)
                .WithMessage("Valor fuera de rango");

            RuleFor(x => x)
                .Must(x => x.SourceCurrency != x.TargetCurrency)
                .WithMessage("Las divisas de origen y destino deben ser diferentes")
                .Must(request => IsValidCurrencyPair(request.SourceCurrency, request.TargetCurrency))
                .WithMessage(request =>
                {
                    var availablePairs = string.Join(", ", _supportedPairs.OrderBy(p => p));
                    return $"El par de monedas {request.SourceCurrency}_{request.TargetCurrency} no está soportado. Pares disponibles: {availablePairs}";
                });
        }

        private bool BeValidCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                return false;

            return _supportedCurrencies.Contains(currency.ToUpper());
        }

        private bool IsValidCurrencyPair(string sourceCurrency, string targetCurrency)
        {
            if (string.IsNullOrWhiteSpace(sourceCurrency) || string.IsNullOrWhiteSpace(targetCurrency))
                return false;

            var currencyPair = $"{sourceCurrency.ToUpper()}_{targetCurrency.ToUpper()}";
            return _supportedPairs.Contains(currencyPair);
        }

        private HashSet<string> LoadSupportedPairs()
        {
            var exchangeRatesSection = _configuration.GetSection("ExchangeRates");
            var supportedPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in exchangeRatesSection.GetChildren())
            {
                if (!string.IsNullOrWhiteSpace(pair.Key))
                {
                    supportedPairs.Add(pair.Key.ToUpper());
                }
            }

            return supportedPairs;
        }

        private HashSet<string> ExtractSupportedCurrencies()
        {
            var currencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in _supportedPairs)
            {
                var parts = pair.Split('_');
                if (parts.Length == 2)
                {
                    currencies.Add(parts[0].ToUpper());
                    currencies.Add(parts[1].ToUpper());
                }
            }

            return currencies;
        }
    }
}