using System.Diagnostics.CodeAnalysis;

namespace ExchangeRateOffers.Core.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]

    /// <summary>
    /// Se lanza cuando todos los proveedores fallan al obtener tasas de cambio
    /// </summary>
    public class AllProvidersFailedException : ExchangeRateException
    {
        public IReadOnlyList<string> FailureReasons { get; }

        public AllProvidersFailedException(IReadOnlyList<string> failureReasons)
            : base("Todos los proveedores fallaron al obtener tasas de cambio")
        {
            FailureReasons = failureReasons;
        }
    }
}
