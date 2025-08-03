using Microsoft.AspNetCore.Mvc;
using ExchangeRateOffers.Mock3.DTOs;
using ExchangeRateOffers.Mock3.Services;
using ExchangeRateOffers.Mock3.Dtos;

namespace ExchangeRateOffers.Mock3.Controllers
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
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<ExchangeResponse>> ConvertCurrency([FromBody] ExchangeRequest request)
        {
            try
            {
                // Get user from context (set by middleware)
                var userId = HttpContext.Items["UserId"]?.ToString();
                _logger.LogInformation("Processing advanced exchange rate request for user: {UserId}", userId);

                // Validate request
                if (request?.Exchange == null)
                {
                    var errorResponse = new ErrorResponse(
                        StatusCode: 400,
                        Message: "Request body with 'exchange' object is required"
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
                        Message: "Invalid exchange data. SourceCurrency, TargetCurrency and Quantity > 0 are required"
                    );
                    return BadRequest(errorResponse);
                }

                // Get converted total
                var total = _exchangeRateService.GetConvertedTotal(
                    exchange.SourceCurrency.ToUpper(),
                    exchange.TargetCurrency.ToUpper(),
                    exchange.Quantity);

                var response = new ExchangeResponse(
                    StatusCode: 200,
                    Message: "Exchange completed successfully",
                    Data: new ExchangeData(total)
                );

                _logger.LogInformation("Successfully processed exchange: {Quantity} {Source} to {Total} {Target}",
                    exchange.Quantity, exchange.SourceCurrency, total, exchange.TargetCurrency);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid currency pair: {Message}", ex.Message);
                var errorResponse = new ErrorResponse(
                    StatusCode: 404,
                    Message: ex.Message
                );
                return NotFound(errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing advanced exchange request");
                var errorResponse = new ErrorResponse(
                    StatusCode: 500,
                    Message: "An unexpected error occurred"
                );
                return StatusCode(500, errorResponse);
            }
        }
    }
}