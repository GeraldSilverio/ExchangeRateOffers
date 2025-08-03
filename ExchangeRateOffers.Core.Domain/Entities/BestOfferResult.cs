namespace ExchangeRateOffers.Core.Domain.Entities
{
    /// <summary>
    /// Contiene la mejor oferta de cambio junto con todas las respuestas de proveedores
    /// </summary>
    public record BestOfferResult(
        ExchangeOffer BestOffer,
        IReadOnlyList<ExchangeOffer> AllOffers,
        ExchangeRequest OriginalRequest,
        DateTime ProcessedAt
    );
}
