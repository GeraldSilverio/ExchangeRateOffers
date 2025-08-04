using ExchangeRateOffers.Core.Application.Dtos.InBound;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Core.Application.Validators
{
    [ExcludeFromCodeCoverage]

    /// <summary>
    /// Validador para solicitudes de cambio de divisa
    /// </summary>
    public class ExchangeRequestDtoValidator : AbstractValidator<ExchangeRequestDto>
    {
        public ExchangeRequestDtoValidator()
        {
            RuleFor(x => x.SourceCurrency)
                .NotEmpty()
                .WithMessage("La divisa de origen es requerida")
                .Length(3)
                .WithMessage("La divisa de origen debe tener exactamente 3 caracteres");

            RuleFor(x => x.TargetCurrency)
                .NotEmpty()
                .WithMessage("La divisa de destino es requerida")
                .Length(3)
                .WithMessage("La divisa de destino debe tener exactamente 3 caracteres");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("El monto debe ser mayor que cero");

            RuleFor(x => x)
                .Must(x => x.SourceCurrency != x.TargetCurrency)
                .WithMessage("Las divisas de origen y destino deben ser diferentes");
        }
    }
}
