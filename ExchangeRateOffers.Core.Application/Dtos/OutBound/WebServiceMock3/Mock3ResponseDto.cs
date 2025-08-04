namespace ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock3
{
    public record Mock3ResponseDto(
        int StatusCode,
        string Message,
        Mock3DataDto Data
    );
}
