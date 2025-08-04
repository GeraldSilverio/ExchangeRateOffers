
using ExchangeRateOffers.Core.Application.Enums;

namespace ExchangeRateOffers.Core.Application.Common
{
    /// <summary>
    /// Representa una solicitud generica Http
    /// 
    /// </summary>
    /// <typeparam name="TRequest">Objeto del tipo que se enviara en la solicitud Http</typeparam>
    public class PayloadRequest<TRequest>
    {
        public PayloadRequest(string? clientName, TRequest? request, ContentType contentType)
        {
            ClientName = clientName;
            Request = request;
            ContentType = contentType;
        }

        /// <summary>
        /// Nombre del cliente que realiza la solicitud
        /// </summary>
        public string? ClientName { get; set; }
        /// <summary>
        /// Objeto de solicitud que se enviara en la solicitud Http
        /// </summary>
        public TRequest? Request { get; set; }
        /// <summary>
        /// Tipo de contenido de la solicitud
        /// </summary>
        public ContentType ContentType { get; set; }
    }
}
