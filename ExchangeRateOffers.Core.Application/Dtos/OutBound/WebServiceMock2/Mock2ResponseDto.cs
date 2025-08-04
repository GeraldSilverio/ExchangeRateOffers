using System.Xml.Serialization;

namespace ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock2
{
    [XmlRoot("Exchange")]
    public class Mock2ResponseDto
    {
        [XmlElement("Result")]
        public double Result { get; set; }
    }
}
