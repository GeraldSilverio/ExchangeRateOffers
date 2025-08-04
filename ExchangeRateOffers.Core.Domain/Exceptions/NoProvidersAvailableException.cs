using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Core.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]

    /// <summary>
    /// Se lanza cuando ningún proveedor está disponible o habilitado
    /// </summary>
    public class NoProvidersAvailableException : ExchangeRateException
    {
        public NoProvidersAvailableException()
            : base("No hay proveedores de tasas de cambio disponibles") { }
    }
}
