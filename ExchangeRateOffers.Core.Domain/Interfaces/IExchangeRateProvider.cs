using ExchangeRateOffers.Core.Domain.Entities;

namespace ExchangeRateOffers.Core.Domain.Interfaces
{
    /// <summary>
    /// Contrato para proveedores de tasas de cambio externos
    /// </summary>
    public interface IExchangeRateProvider
    {
        string ProviderId { get; }
        string ProviderName { get; }
        Task<ExchangeOffer> GetExchangeOfferAsync(ExchangeRequest request, CancellationToken cancellationToken = default);
    }
}
