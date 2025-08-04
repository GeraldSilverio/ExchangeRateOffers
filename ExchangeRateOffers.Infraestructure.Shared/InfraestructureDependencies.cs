using ExchangeRateOffers.Infraestructure.Shared.Services.Implementations;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ExchangeRateOffers.Core.Application.Contract.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Infraestructure.Shared
{
    [ExcludeFromCodeCoverage]

    public static class InfraestructureDependencies
    {
        public static void AddSharedDependencies(this IServiceCollection services, IConfiguration configuration)
        {

            #region FireBaseConfiguration
            var firebaseConfigPath = configuration["Firebase:ServiceAccountKeyPath"];

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(firebaseConfigPath)
            });
            #endregion

            #region ServicesConfiguration
            services.AddSingleton<IApiConfigurationService, ApiConfigurationService>();
            services.AddScoped(typeof(IWebServiceConsultService<,>), typeof(WebServiceConsultService<,>));
            #endregion

            #region HttpClientFactoryConfiguration
            services.AddHttpClient("MOCK_API_1", (serviceProvider, client) =>
            {
                var configRepo = serviceProvider.GetRequiredService<IApiConfigurationService>();
                var config = configRepo.GetApiConfiguration("MOCK_API_1");

                client.BaseAddress = new Uri(config?.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(config?.TimeoutSeconds ?? 30);
                client.DefaultRequestHeaders.Add("User-Agent", "ExchangeRateOffers/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            services.AddHttpClient("MOCK_API_2", (serviceProvider, client) =>
            {
                var configRepo = serviceProvider.GetRequiredService<IApiConfigurationService>();
                var config = configRepo.GetApiConfiguration("MOCK_API_2");

                client.BaseAddress = new Uri(config?.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(config?.TimeoutSeconds ?? 30);
                client.DefaultRequestHeaders.Add("User-Agent", "ExchangeRateOffers/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/xml");
            });
            services.AddHttpClient("MOCK_API_3", (serviceProvider, client) =>
            {
                var configRepo = serviceProvider.GetRequiredService<IApiConfigurationService>();
                var config = configRepo.GetApiConfiguration("MOCK_API_3");

                client.BaseAddress = new Uri(config?.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(config?.TimeoutSeconds ?? 30);
                client.DefaultRequestHeaders.Add("User-Agent", "ExchangeRateOffers/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            #endregion


        }
    }
}
