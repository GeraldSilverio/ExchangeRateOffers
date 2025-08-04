using ExchangeRateOffers.Core.Application.Common;
using ExchangeRateOffers.Core.Application.Contanst;
using ExchangeRateOffers.Core.Application.Contract.Interfaces;
using ExchangeRateOffers.Core.Application.Dtos.InBound;
using ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock1;
using ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock2;
using ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock3;
using ExchangeRateOffers.Core.Application.Enums;
using ExchangeRateOffers.Core.Domain.Entities;
using Serilog;

namespace ExchangeRateOffers.Core.Application.Contract.Services
{
    /// <summary>
    /// Servicio para obtener la mejor oferta de cambio de divisas consultando múltiples proveedores simultáneamente
    /// </summary>
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IWebServiceConsultService<Mock1RequestDto, Mock1ResponseDto> _mock1WebService;
        private readonly IWebServiceConsultService<Mock2RequestDto, Mock2ResponseDto> _mock2WebService;
        private readonly IWebServiceConsultService<Mock3RequestDto, Mock3ResponseDto> _mock3WebService;

        public ExchangeRateService(
            IWebServiceConsultService<Mock1RequestDto, Mock1ResponseDto> mock1WebService,
            IWebServiceConsultService<Mock2RequestDto, Mock2ResponseDto> mock2WebService,
            IWebServiceConsultService<Mock3RequestDto, Mock3ResponseDto> mock3WebService)
        {
            _mock1WebService = mock1WebService;
            _mock2WebService = mock2WebService;
            _mock3WebService = mock3WebService;
        }
        public async Task<ApiResponse<ExchangeResponseDto>> GetBestOfferAsync(ExchangeRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Information($"Iniciando consulta simultánea a 3 APIs para conversión {request.SourceCurrency} -> {request.TargetCurrency}, Cantidad: {request.Amount}");

                var startTime = DateTime.UtcNow;

                var tasks = new[]
                {
                    GetOfferFromMock1(request, cancellationToken),
                    GetOfferFromMock2(request, cancellationToken),
                    GetOfferFromMock3(request, cancellationToken)
                };

                var offers = await Task.WhenAll(tasks);

                var totalDuration = DateTime.UtcNow - startTime;
                Log.Information($"Todas las APIs completadas en {totalDuration.TotalMilliseconds}ms");

                var successfulOffers = offers.Where(x => x.IsSuccessful).ToList();

                if (!successfulOffers.Any())
                {
                    Log.Information("Ninguna API respondió exitosamente");
                    return ApiResponse<ExchangeResponseDto>.ErrorResponse("No se pudo obtener cotizaciones", "Todas las APIs fallaron");
                }

                var bestOffer = successfulOffers.OrderByDescending(x => x.ConvertedAmount).First();

                var result = new BestOfferResult(bestOffer, successfulOffers.ToList());

                Log.Information($"Mejor oferta obtenida de {bestOffer.ProviderName} - Rate: {bestOffer.ExchangeRate}, APIs exitosas: {successfulOffers.Count}/3");

                return ApiResponse<ExchangeResponseDto>.Success(new ExchangeResponseDto(result),"Consulta Exitosa");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error general procesando solicitud de exchange rate");
                return ApiResponse<ExchangeResponseDto>.ErrorResponse("Error processing request", ex.Message);
            }
        }

        private async Task<ExchangeOffer> GetOfferFromMock1(ExchangeRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = new Mock1RequestDto(request.SourceCurrency, request.TargetCurrency, request.Amount);
                var payload = new PayloadRequest<Mock1RequestDto>(WebServiceName.MOCK_WEB_WEBSERVICE_1, dto, ContentType.JSON);
                var response = await _mock1WebService.SendPostAsync(payload);

                if (response.Success)
                {
                    var converted = response.Response.Rate * request.Amount;
                    var rate = converted / request.Amount;

                    Log.Information($"Mock1 exitoso - Result: {converted}, Rate: {rate}");
                    return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_1, converted, rate, true);
                }
                return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_1, 0, 0, false, response.ErrorMessage);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Mock1 falló");
                return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_1, 0, 0, false);
            }
        }

        private async Task<ExchangeOffer> GetOfferFromMock2(ExchangeRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = new Mock2RequestDto
                {
                    From = request.SourceCurrency,
                    To = request.TargetCurrency,
                    Amount = request.Amount
                };
                var payload = new PayloadRequest<Mock2RequestDto>(WebServiceName.MOCK_WEB_WEBSERVICE_2, dto, ContentType.XML);
                var response = await _mock2WebService.SendPostAsync(payload);

                if (response.Success)
                {
                    var converted = response.Response.Result;
                    var rate = converted / request.Amount;

                    Log.Information($"Mock2 exitoso - Result: {converted}, Rate: {rate}");
                    return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_2, converted, rate, true);
                }
                return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_2, 0, 0, false,response.ErrorMessage);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Mock2 falló");
                return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_2, 0, 0, false);
            }
        }

        private async Task<ExchangeOffer> GetOfferFromMock3(ExchangeRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var dto = new Mock3RequestDto(new ExchangeDetails(request.SourceCurrency, request.TargetCurrency, request.Amount));
                var payload = new PayloadRequest<Mock3RequestDto>(WebServiceName.MOCK_WEB_WEBSERVICE_3, dto, ContentType.JSON);
                var response = await _mock3WebService.SendPostAsync(payload);

                if (response.Success)
                {
                    var converted = response.Response.Data.Total;
                    var rate = converted / request.Amount;

                    Log.Information($"Mock3 exitoso - Result: {converted}, Rate: {rate}");
                    return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_3, converted, rate, true);
                }
                return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_3, 0, 0, false, response.ErrorMessage);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Mock3 falló");
                return new ExchangeOffer(WebServiceName.MOCK_WEB_WEBSERVICE_3, 0, 0, false);
            }
        }
    }
}
