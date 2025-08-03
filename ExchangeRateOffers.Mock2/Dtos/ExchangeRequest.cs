using System.Xml.Serialization;

namespace ExchangeRateOffers.Mock2.Dtos
{
    [XmlRoot("Exchange")]
    public class ExchangeRequest
    {
        [XmlElement("From")]
        public string From { get; set; }

        [XmlElement("To")]
        public string To { get; set; }

        [XmlElement("Amount")]
        public decimal Amount { get; set; }
    }
}
