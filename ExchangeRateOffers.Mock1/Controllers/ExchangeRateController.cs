using ExchangeRateOffers.Mock1.Dtos;
using ExchangeRateOffers.Mock1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRateOffers.Mock1.Controllers
{
    [ApiController]
    [Route("api/mock1")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ExchangeRateController> _logger;

        public ExchangeRateController(IExchangeRateService exchangeRateService,
            ILogger<ExchangeRateController> logger)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        [HttpPost("get-rate")]
        public ActionResult<ResponseDto> ConvertCurrency([FromBody] RequestDto request)
        {
            try
            {
                var userId = HttpContext.Items["UserId"]?.ToString();
                _logger.LogInformation("Procesando solicitud de tasa de cambio para el usuario: {UserId}", userId);

                // Validar solicitud
                if (request == null)
                {
                    return BadRequest(new ErrorResponse("BadRequest", "Se requiere el cuerpo de la solicitud", 400));
                }

                if (string.IsNullOrWhiteSpace(request.From) ||
                    string.IsNullOrWhiteSpace(request.To) ||
                    request.Value <= 0)
                {
                    return BadRequest(new ErrorResponse("BadRequest",
                        "Solicitud inválida. Se requieren las monedas From, To y Value > 0", 400));
                }

                // Obtener tasa de cambio
                var rate = _exchangeRateService.GetExchangeRate(request.From.ToUpper(), request.To.ToUpper());
                var response = new ResponseDto(rate);

                _logger.LogInformation("Tasa de cambio procesada exitosamente: {From} a {To} = {Rate}",
                    request.From, request.To, rate);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Par de monedas inválido: {Message}", ex.Message);
                return NotFound(new ErrorResponse("NotFound", ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado procesando la solicitud de tasa de cambio");
                return StatusCode(500, new ErrorResponse("InternalServerError", "Ocurrió un error inesperado", 500));
            }
        }
    }
}