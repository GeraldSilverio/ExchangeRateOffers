namespace ExchangeRateOffers.Mock3.DTOs
{
    public record ErrorResponse(
        int StatusCode,
        string Message,
        object? Data = null
    );
}