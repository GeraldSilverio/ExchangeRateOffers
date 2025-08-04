namespace ExchangeRateOffers.Core.Application.Common
{
    public class PayloadResponse<T>
    {
        public T? Response { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
        public int StatusCode { get; set; }
        public string? WebServiceName { get; set; }
    }
}
