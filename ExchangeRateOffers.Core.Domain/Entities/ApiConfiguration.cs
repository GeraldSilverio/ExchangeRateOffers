using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Core.Domain.Entities
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Configuración para los proveedores de APIs externos
    /// </summary>
    public record ApiConfiguration(
        string ProviderName,
        string BaseUrl,
        double TimeoutSeconds
    );
}
