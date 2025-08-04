namespace ExchangeRateOffers.Core.Application.Dtos.InBound
{
    /// <summary>
    /// DTO para solicitudes de cambio de divisa desde la API
    /// </summary>
    public record ExchangeRequestDto(
        string SourceCurrency,
        string TargetCurrency,
        double Amount
    );
}
