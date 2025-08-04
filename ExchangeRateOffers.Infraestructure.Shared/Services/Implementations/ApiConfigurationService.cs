using ExchangeRateOffers.Core.Application.Contract.Interfaces;
using ExchangeRateOffers.Core.Domain.Entities;
using ExchangeRateOffers.Core.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Infraestructure.Shared.Services.Implementations
{
    [ExcludeFromCodeCoverage]
    public class ApiConfigurationService : IApiConfigurationService
    {
        private readonly IConfiguration _configuration; 
        public ApiConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ApiConfiguration GetApiConfiguration(string providerName)
        {
            try
            {
                var externalApis = _configuration.GetSection("ExternalApis");
                if (externalApis == null || !externalApis.Exists())
                {
                    Log.Error("No se encontró la sección 'ExternalApis' en la configuración.");
                    throw new NoProvidersAvailableException();
                }

                var providerConfig = externalApis.GetSection(providerName);
                if (providerConfig == null || !providerConfig.Exists())
                {
                    Log.Error("No se encontró la configuración para el proveedor {ProviderName}", providerName);
                }

                return new ApiConfiguration(providerName, providerConfig["BaseUrl"] ?? string.Empty, Convert.ToDouble(providerConfig["TimeoutSeconds"]));


            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener la configuración de la API para el proveedor {ProviderName}", providerName);
                throw new Exception($"Error al obtener la configuración de la API para el proveedor {providerName}", ex);
            }
        }
    }
}
