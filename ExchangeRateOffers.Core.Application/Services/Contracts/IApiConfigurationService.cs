using ExchangeRateOffers.Core.Domain.Entities;

namespace ExchangeRateOffers.Core.Application.Contract.Interfaces
{
    public interface IApiConfigurationService
    {
        ApiConfiguration GetApiConfiguration(string providerName);
    }
}
