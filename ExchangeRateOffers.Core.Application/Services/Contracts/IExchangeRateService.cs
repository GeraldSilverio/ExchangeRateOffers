using ExchangeRateOffers.Core.Application.Dtos.InBound;
using ExchangeRateOffers.Core.Domain.Entities;

namespace ExchangeRateOffers.Core.Application.Contract.Interfaces
{
    /// <summary>
    /// Servicio principal para comparar ofertas de cambio de múltiples proveedores
    /// </summary>
    public interface IExchangeRateService
    {
        Task<ApiResponse<ExchangeResponseDto>> GetBestOfferAsync(ExchangeRequestDto request, CancellationToken cancellationToken = default);
    }
}

