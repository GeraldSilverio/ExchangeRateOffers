using ExchangeRateOffers.Core.Domain.Entities;

namespace ExchangeRateOffers.Core.Domain.Interfaces
{
    /// <summary>
    /// Servicio principal para comparar ofertas de cambio de múltiples proveedores
    /// </summary>
    public interface IExchangeRateService
    {
        Task<BestOfferResult> GetBestOfferAsync(ExchangeRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ExchangeOffer>> GetAllOffersAsync(ExchangeRequest request, CancellationToken cancellationToken = default);
    }
}

