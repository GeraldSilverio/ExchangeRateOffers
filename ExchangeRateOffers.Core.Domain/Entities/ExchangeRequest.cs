namespace ExchangeRateOffers.Core.Domain.Entities
{
    /// <summary>
    /// Representa una solicitud de cambio de divisa del cliente
    /// </summary>
    public record ExchangeRequest(
        string SourceCurrency,
        string TargetCurrency,
        decimal Amount
    );
}
