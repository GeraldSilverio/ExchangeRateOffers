namespace ExchangeRateOffers.Core.Domain.Entities
{
    /// <summary>
    /// Configuración para los proveedores de APIs externos
    /// </summary>
    public record ApiConfiguration(
        string ProviderId,
        string ProviderName,
        string BaseUrl,
        string ApiKey,
        int TimeoutSeconds,
        int MaxRetries,
        bool IsEnabled
    );
}
