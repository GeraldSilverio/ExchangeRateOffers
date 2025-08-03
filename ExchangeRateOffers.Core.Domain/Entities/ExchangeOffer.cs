namespace ExchangeRateOffers.Core.Domain.Entities
{
    /// <summary>
    /// Representa una oferta de cambio de un proveedor de API específico
    /// </summary>
    public record ExchangeOffer(
        string ProviderId,
        string ProviderName,
        decimal ConvertedAmount,
        decimal ExchangeRate,
        bool IsSuccessful,
        string? ErrorMessage = null,
        TimeSpan ResponseTime = default
    );
}
