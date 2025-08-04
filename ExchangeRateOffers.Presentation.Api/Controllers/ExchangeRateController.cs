using ExchangeRateOffers.Core.Application.Contract.Interfaces;
using ExchangeRateOffers.Core.Application.Dtos.InBound;
using ExchangeRateOffers.Core.Domain.Entities;
using ExchangeRateOffers.Core.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ExchangeRateOffers.Presentation.Api.Controllers
{
    /// <summary>
    /// Controlador principal para operaciones de cambio de divisa con respuestas JSend
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IValidator<ExchangeRequestDto> _validator;
        private readonly IExchangeRateService _exchangeService;

        public ExchangeRateController(IExchangeRateService exchangeService, IValidator<ExchangeRequestDto> validator)
        {
            _exchangeService = exchangeService;
            _validator = validator;
        }

        /// <summary>
        /// Obtiene la mejor oferta de cambio comparando múltiples proveedores
        /// </summary>
        [HttpPost("best-offer")]
        [ProducesResponseType(typeof(ApiResponse<ExchangeResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ExchangeResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        public async Task<ActionResult<ApiResponse<string>>> GetBestOffer(
            [FromBody] ExchangeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {

                // Validar la solicitud
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var failResponse = ApiResponse<ExchangeResponseDto>.Fail(
                        validationResult.Errors.FirstOrDefault().ErrorMessage,
                        "Errores de validación en la solicitud"
                    );

                    return BadRequest(failResponse);
                }

                var result = await _exchangeService.GetBestOfferAsync(request, cancellationToken);


                if (result.Successfull)
                {
                    Log.Information("Mejor oferta procesada exitosamente. Proveedor ganador: {ProviderId}", result.Data.BestOfferResponse.BestOffer);

                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (NoProvidersAvailableException ex)
            {
                Log.Information(ex, "No hay proveedores disponibles");
                var failResponse = ApiResponse<ExchangeResponseDto>.Fail("", ex.Message);
                return StatusCode(503, failResponse);
            }
            catch (AllProvidersFailedException ex)
            {
                Log.Warning(ex, "Todos los proveedores fallaron");
                var failResponse = ApiResponse<ExchangeResponseDto>.Fail("", ex.Message);
                return StatusCode(503, failResponse);
            }
            catch (ExchangeRateException ex)
            {
                Log.Error(ex, "Error de negocio al procesar solicitud de cambio");
                var failResponse = ApiResponse<ExchangeResponseDto>.Fail("", "Error de negocio");
                return BadRequest(failResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al procesar solicitud de mejor oferta");
                var errorResponse = ApiResponse<ExchangeResponseDto>.ErrorResponse("Error interno del servidor", "");
                return StatusCode(500, errorResponse);
            }
        }
    }
}

