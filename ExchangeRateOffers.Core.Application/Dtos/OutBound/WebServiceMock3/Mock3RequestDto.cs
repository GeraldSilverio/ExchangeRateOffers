using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock3
{
    [ExcludeFromCodeCoverage]
    public record Mock3RequestDto(ExchangeDetails Exchange);

    public record ExchangeDetails(
        string SourceCurrency,
        string TargetCurrency,
        double Quantity
    );
}