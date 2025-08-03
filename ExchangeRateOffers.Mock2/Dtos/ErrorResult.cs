using System.Xml.Serialization;

namespace ExchangeRateOffers.Mock2.Dtos
{
    [XmlRoot("Error")]
    public class ErrorResult
    {
        [XmlElement("Code")]
        public int Code { get; set; }

        [XmlElement("Message")]
        public string Message { get; set; }
    }
}
