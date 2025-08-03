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

        [HttpPost("convert-currency")]
        public ActionResult<ResponseDto> ConvertCurrency([FromBody] RequestDto request)
        {
            try
            {
                // Get user from context (set by middleware)
                var userId = HttpContext.Items["UserId"]?.ToString();
                _logger.LogInformation("Processing exchange rate request for user: {UserId}", userId);

                // Validate request
                if (request == null)
                {
                    return BadRequest(new ErrorResponse("BadRequest", "Request body is required", 400));
                }

                if (string.IsNullOrWhiteSpace(request.From) ||
                    string.IsNullOrWhiteSpace(request.To) ||
                    request.Value <= 0)
                {
                    return BadRequest(new ErrorResponse("BadRequest",
                        "Invalid request. From, To currencies and Value > 0 are required", 400));
                }

                // Get exchange rate
                var rate = _exchangeRateService.CalculateConvertedAmount(request.From.ToUpper(), request.To.ToUpper(),request.Value);

                var response = new ResponseDto(rate);

                _logger.LogInformation("Successfully processed exchange rate: {From} to {To} = {Rate}",
                    request.From, request.To, rate);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid currency pair: {Message}", ex.Message);
                return NotFound(new ErrorResponse("NotFound", ex.Message, 404));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing exchange rate request");
                return StatusCode(500, new ErrorResponse("InternalServerError", "An unexpected error occurred", 500));
            }
        }
    }
}
