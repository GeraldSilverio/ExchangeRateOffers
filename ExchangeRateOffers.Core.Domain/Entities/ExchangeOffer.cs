namespace ExchangeRateOffers.Core.Domain.Entities
{
    /// <summary>
    /// Representa una oferta de cambio de un proveedor de API específico
    /// </summary>
    public record ExchangeOffer(
        string ProviderName,
        double ConvertedAmount,
        double ExchangeRate,
        bool IsSuccessful,
        string? ErrorMessage = null
    );
}
