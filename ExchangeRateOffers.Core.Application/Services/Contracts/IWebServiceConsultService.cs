using ExchangeRateOffers.Core.Application.Common;

namespace ExchangeRateOffers.Core.Application.Contract.Interfaces
{
    public interface IWebServiceConsultService<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        Task<PayloadResponse<TResponse>> SendPostAsync(PayloadRequest<TRequest> request);
    }
}
