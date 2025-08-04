using ExchangeRateOffers.Core.Application.Common;
using ExchangeRateOffers.Core.Application.Contract.Interfaces;
using ExchangeRateOffers.Core.Application.Enums;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace ExchangeRateOffers.Infraestructure.Shared.Services.Implementations
{
    public class WebServiceConsultService<TRequest, TResponse> : IWebServiceConsultService<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebServiceConsultService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PayloadResponse<TResponse>> SendPostAsync(PayloadRequest<TRequest> request)
        {
            try
            {
                var response = new PayloadResponse<TResponse>()
                {
                    WebServiceName = request.ClientName
                };

                var httpClient = _httpClientFactory.CreateClient(request.ClientName);

                var content = CreateHttpContent(request.Request, request.ContentType);

                Log.Information($"Ejecutando {request.ClientName} - Endpoint: {httpClient.BaseAddress}, ContentType: {request.ContentType}");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _httpContextAccessor.HttpContext.Items["Token"]?.ToString());

                var httpResponse = await httpClient.PostAsync("", content);

                response.StatusCode = (int)httpResponse.StatusCode;

                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    response.Response = DeserializeResponse<TResponse>(responseContent, request.ContentType);
                    response.Success = true;
                    Log.Information($"{request.ClientName} completada exitosamente");
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    response.ErrorMessage = $"Error en {request.ClientName}: {httpResponse.StatusCode} - {errorContent}";
                    response.Success = false;
                    Log.Error($"{request.ClientName} falló: {response.ErrorMessage}");
                }
                return response;

            }
            catch (Exception ex)
            {
                var response = new PayloadResponse<TResponse>()
                {
                    WebServiceName = request.ClientName,
                    Success = false,
                    ErrorMessage = $"Error al consultar {request.ClientName}: {ex.Message}",

                };
                Log.Error(ex, $"{request.ClientName} - Error: {ex.Message}");
                return response;
            }
        }
        private HttpContent CreateHttpContent<T>(T request, ContentType contentType)
        {
            return contentType switch
            {
                ContentType.JSON => new StringContent(
                    JsonSerializer.Serialize(request, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    }),
                    Encoding.UTF8,
                    "application/json"),

                ContentType.XML => new StringContent(
                    SerializeToXml(request),
                    Encoding.UTF8,
                    "application/xml"),

                _ => throw new ArgumentException($"ContentType {contentType} no soportado")
            };
        }
        private T? DeserializeResponse<T>(string content, ContentType contentType)
        {
            try
            {
                return contentType switch
                {
                    ContentType.JSON => JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }),

                    ContentType.XML => DeserializeFromXml<T>(content),

                    _ => throw new ArgumentException($"ContentType {contentType} no soportado")
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error deserializando respuesta como {contentType}: {content}");
                return default(T);
            }
        }

        private string SerializeToXml<T>(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }

        private T? DeserializeFromXml<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(xml);
            return (T?)serializer.Deserialize(stringReader);
        }
    }
}
