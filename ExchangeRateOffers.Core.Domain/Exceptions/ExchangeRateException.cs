namespace ExchangeRateOffers.Core.Domain.Exceptions
{
    /// <summary>
    /// Excepción base para errores relacionados con cambio de divisas
    /// </summary>
    public abstract class ExchangeRateException : Exception
    {
        protected ExchangeRateException(string message) : base(message) { }
        protected ExchangeRateException(string message, Exception innerException) : base(message, innerException) { }
    }
}
