namespace ExchangeRateOffers.Mock3.DTOs
{
    public record ExchangeRequest(ExchangeDetails Exchange);

    public record ExchangeDetails(
        string SourceCurrency,
        string TargetCurrency,
        decimal Quantity
    );
}