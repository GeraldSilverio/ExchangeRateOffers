using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Presentation.Api.Extensions
{
    [ExcludeFromCodeCoverage]  
    public static class ServiceRegistration
    {
        public static void AddPresentationLayer(this IServiceCollection service)
        {
            service.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "Exchange Rate Offers API",
                    Version = "v1",
                    Description = "API para comparar ofertas de cambio de divisa de múltiples proveedores",
                    Contact = new OpenApiContact { Email = "Ing.geraldsilverioserrata@gmail,com", Name = "Gerald Antonio Silverio Serrata" }
                });

                // Configurar autenticación en Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }
    }
}
