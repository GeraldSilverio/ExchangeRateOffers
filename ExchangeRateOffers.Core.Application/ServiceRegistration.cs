using ExchangeRateOffers.Core.Application.Contract.Interfaces;
using ExchangeRateOffers.Core.Application.Contract.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ExchangeRateOffers.Core.Application
{
    [ExcludeFromCodeCoverage]
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IExchangeRateService, ExchangeRateService>();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
