using ExchangeRateOffers.Mock3.Dtos;

namespace ExchangeRateOffers.Mock3.DTOs
{
    public record ExchangeResponse(
        int StatusCode,
        string Message,
        ExchangeData Data
    );
}
