using Microsoft.AspNetCore.Mvc;
using ExchangeRateOffers.Mock3.DTOs;
using ExchangeRateOffers.Mock3.Services;
using ExchangeRateOffers.Mock3.Dtos;

namespace ExchangeRateOffers.Mock3.Controllers
{
    [ApiController]
    [Route("api/mock3")]
    public class ExchangeController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ExchangeController> _logger;

        public ExchangeController(IExchangeRateService exchangeRateService,
            ILogger<ExchangeController> logger)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        [HttpPost("convert")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<ExchangeResponse> ConvertCurrency([FromBody] ExchangeRequest request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"]?.ToString();
                _logger.LogInformation("Procesando solicitud de intercambio avanzado para el usuario: {UserId}", userId);

                // Validar solicitud
                if (request?.Exchange == null)
                {
                    var errorResponse = new ErrorResponse(
                        StatusCode: 400,
                        Message: "Se requiere el cuerpo de la solicitud con el objeto 'exchange'"
                    );
                    return BadRequest(errorResponse);
                }

                var exchange = request.Exchange;

                if (string.IsNullOrWhiteSpace(exchange.SourceCurrency) ||
                    string.IsNullOrWhiteSpace(exchange.TargetCurrency) ||
                    exchange.Quantity <= 0)
                {
                    var errorResponse = new ErrorResponse(
                        StatusCode: 400,
                        Message: "Datos de intercambio inválidos. Se requieren SourceCurrency, TargetCurrency y Quantity > 0"
                    );
                    return BadRequest(errorResponse);
                }

                // Obtener total convertido
                var total = _exchangeRateService.GetConvertedTotal(
                    exchange.SourceCurrency.ToUpper(),
                    exchange.TargetCurrency.ToUpper(),
                    exchange.Quantity);

                var response = new ExchangeResponse(
                    StatusCode: 200,
                    Message: "Intercambio completado exitosamente",
                    Data: new ExchangeData(total)
                );

                _logger.LogInformation("Intercambio procesado exitosamente: {Quantity} {Source} a {Total} {Target}",
                    exchange.Quantity, exchange.SourceCurrency, total, exchange.TargetCurrency);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Par de monedas inválido: {Message}", ex.Message);
                var errorResponse = new ErrorResponse(
                    StatusCode: 404,
                    Message: ex.Message
                );
                return NotFound(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado procesando la solicitud de intercambio avanzado");
                var errorResponse = new ErrorResponse(
                    StatusCode: 500,
                    Message: "Ocurrió un error inesperado"
                );
                return StatusCode(500, errorResponse);
            }
        }
    }
}