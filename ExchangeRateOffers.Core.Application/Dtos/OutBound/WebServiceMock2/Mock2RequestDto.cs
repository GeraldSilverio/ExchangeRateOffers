using System.Xml.Serialization;

namespace ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock2
{
    [XmlRoot("Exchange")]
    public class Mock2RequestDto
    {
        [XmlElement("From")]
        public string From { get; set; }

        [XmlElement("To")]
        public string To { get; set; }

        [XmlElement("Amount")]
        public double Amount { get; set; }
    }
}
