using Microsoft.AspNetCore.Mvc;
using ExchangeRateOffers.Mock2.Services;
using System.Xml.Serialization;
using System.Text;
using ExchangeRateOffers.Mock2.Dtos;

namespace ExchangeRateOffers.Mock2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                // Get user from context (set by middleware)
                var userId = HttpContext.Items["UserId"]?.ToString();
                _logger.LogInformation("Processing XML exchange rate request for user: {UserId}", userId);

                // Read the XML request body
                using var reader = new StreamReader(Request.Body);
                var xmlContent = await reader.ReadToEndAsync();

                _logger.LogDebug("Received XML: {XmlContent}", xmlContent);

                // Deserialize XML request
                var request = DeserializeXml<ExchangeRequest>(xmlContent);

                if (request == null)
                {
                    return BadRequest(SerializeToXml(new ErrorResult
                    {
                        Code = 400,
                        Message = "Invalid XML request format"
                    }));
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.From) ||
                    string.IsNullOrWhiteSpace(request.To) ||
                    request.Amount <= 0)
                {
                    return BadRequest(SerializeToXml(new ErrorResult
                    {
                        Code = 400,
                        Message = "Invalid request. From, To currencies and Amount > 0 are required"
                    }));
                }

                // Get converted amount
                var convertedAmount = _exchangeRateService.GetConvertedAmount(
                    request.From.ToUpper(),
                    request.To.ToUpper(),
                    request.Amount);

                var result = new ExchangeResult
                {
                    Result = convertedAmount
                };

                _logger.LogInformation("Successfully converted {Amount} {From} to {ConvertedAmount} {To}",
                    request.Amount, request.From, convertedAmount, request.To);

                var xmlResponse = SerializeToXml(result);
                return Content(xmlResponse, "application/xml");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid currency pair: {Message}", ex.Message);
                var errorResult = new ErrorResult
                {
                    Code = 404,
                    Message = ex.Message
                };
                return NotFound(SerializeToXml(errorResult));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing XML exchange request");
                var errorResult = new ErrorResult
                {
                    Code = 500,
                    Message = "An unexpected error occurred"
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
                _logger.LogError(ex, "Error deserializing XML: {Xml}", xml);
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
                _logger.LogError(ex, "Error serializing object to XML");
                return "<Error><Code>500</Code><Message>Serialization error</Message></Error>";
            }
        }
    }
}