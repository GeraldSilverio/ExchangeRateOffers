using Microsoft.AspNetCore.Mvc;
using ExchangeRateOffers.Mock2.Services;
using System.Xml.Serialization;
using System.Text;
using ExchangeRateOffers.Mock2.Dtos;

namespace ExchangeRateOffers.Mock2.Controllers
{
    [ApiController]
    [Route("api/mock2")]
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
        [Consumes("application/xml")]
        [Produces("application/xml")]
        public async Task<IActionResult> ConvertCurrency()
        {
            try
            {
                var userId = HttpContext.Items["UserId"]?.ToString();
                _logger.LogInformation("Procesando solicitud de tasa de cambio XML para el usuario: {UserId}", userId);

                // Leer el cuerpo de la solicitud XML
                using var reader = new StreamReader(Request.Body);
                var xmlContent = await reader.ReadToEndAsync();

                _logger.LogDebug("XML recibido: {XmlContent}", xmlContent);

                // Deserializar solicitud XML
                var request = DeserializeXml<ExchangeRequest>(xmlContent);

                if (request == null)
                {
                    return BadRequest(SerializeToXml(new ErrorResult
                    {
                        Code = 400,
                        Message = "Formato de solicitud XML inválido"
                    }));
                }

                // Validar solicitud
                if (string.IsNullOrWhiteSpace(request.From) ||
                    string.IsNullOrWhiteSpace(request.To) ||
                    request.Amount <= 0)
                {
                    return BadRequest(SerializeToXml(new ErrorResult
                    {
                        Code = 400,
                        Message = "Solicitud inválida. Se requieren las monedas From, To y Amount > 0"
                    }));
                }

                // Obtener cantidad convertida
                var convertedAmount = _exchangeRateService.GetConvertedAmount(
                    request.From.ToUpper(),
                    request.To.ToUpper(),
                    request.Amount);

                var result = new ExchangeResult
                {
                    Result = convertedAmount
                };

                _logger.LogInformation("Conversión exitosa: {Amount} {From} a {ConvertedAmount} {To}",
                    request.Amount, request.From, convertedAmount, request.To);

                var xmlResponse = SerializeToXml(result);
                return Content(xmlResponse, "application/xml");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Par de monedas inválido: {Message}", ex.Message);
                var errorResult = new ErrorResult
                {
                    Code = 404,
                    Message = ex.Message
                };
                return NotFound(SerializeToXml(errorResult));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado procesando la solicitud de intercambio XML");
                var errorResult = new ErrorResult
                {
                    Code = 500,
                    Message = "Ocurrió un error inesperado"
                };
                return StatusCode(500, SerializeToXml(errorResult));
            }
        }

        private T DeserializeXml<T>(string xml) where T : class
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var reader = new StringReader(xml);
                return (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializando XML: {Xml}", xml);
                return null;
            }
        }

        private string SerializeToXml<T>(T obj)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var writer = new StringWriter();
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serializando objeto a XML");
                return "<Error><Code>500</Code><Message>Error de serialización</Message></Error>";
            }
        }
    }
}