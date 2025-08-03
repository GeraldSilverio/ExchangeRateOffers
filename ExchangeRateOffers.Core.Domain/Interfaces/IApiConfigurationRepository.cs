using ExchangeRateOffers.Core.Domain.Entities;

namespace ExchangeRateOffers.Core.Domain.Interfaces
{
    /// <summary>
    /// Repositorio para gestionar configuraciones de APIs
    /// </summary>
    public interface IApiConfigurationRepository
    {
        Task<IReadOnlyList<ApiConfiguration>> GetEnabledConfigurationsAsync();
        Task<ApiConfiguration?> GetByProviderIdAsync(string providerId);
    }
}
