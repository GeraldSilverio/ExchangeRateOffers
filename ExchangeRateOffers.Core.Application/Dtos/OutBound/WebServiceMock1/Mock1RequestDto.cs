namespace ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock1
{
    /// <summary>
    /// DTO para solicitudes al servicio web Mock1
    /// </summary>
    /// <param name="From">Moneda de la cual se quiere convertir</param>
    /// <param name="To">Moneda a la cual se va a convertir</param>
    /// <param name="Value">Valor al cual se quiere convertir</param>
    public record Mock1RequestDto(string From, string To, double Value)
    {
    }
}
