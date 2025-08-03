using System.Xml.Serialization;

namespace ExchangeRateOffers.Mock2.Dtos
{
    [XmlRoot("Exchange")]
    public class ExchangeResult
    {
        [XmlElement("Result")]
        public decimal Result { get; set; }
    }
}
