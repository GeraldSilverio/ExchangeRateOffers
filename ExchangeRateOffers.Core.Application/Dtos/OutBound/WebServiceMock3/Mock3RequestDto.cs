namespace ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock3
{
    public record Mock3RequestDto(ExchangeDetails Exchange);

    public record ExchangeDetails(
        string SourceCurrency,
        string TargetCurrency,
        double Quantity
    );
}