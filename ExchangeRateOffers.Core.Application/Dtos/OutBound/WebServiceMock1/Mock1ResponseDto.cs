namespace ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock1
{
    /// <summary>
    /// DTO para respuestas del servicio web Mock1
    /// </summary>
    /// <param name="Rate">Es la tasa de cambio devuelta por el servicio</param>
    public record Mock1ResponseDto(double Rate)
    {
    }
}
