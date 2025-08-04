using ExchangeRateOffers.Core.Application.Common;
using ExchangeRateOffers.Core.Application.Enums;
using ExchangeRateOffers.Infraestructure.Shared.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ExchangeRateOffers.Tests.Unit.Services
{
    public class WebServiceConsultServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly WebServiceConsultService<TestRequestDto, TestResponseDto> _service;
        private readonly HttpClient _httpClient;

        public WebServiceConsultServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.test.com/")
            };

            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var httpContext = new DefaultHttpContext();
            httpContext.Items["Token"] = "test-token";
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _service = new WebServiceConsultService<TestRequestDto, TestResponseDto>(
                _httpClientFactoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task SendPostAsync_WithSuccessfulJsonResponse_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = new PayloadRequest<TestRequestDto>(
                "TEST_API",
                new TestRequestDto { Name = "Test", Value = 100 },
                ContentType.JSON
            );

            var expectedResponse = new TestResponseDto { Result = "Success", Amount = 150 };
            var responseJson = JsonSerializer.Serialize(expectedResponse);

            SetupHttpResponse(HttpStatusCode.OK, responseJson);

            // Act
            var result = await _service.SendPostAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("TEST_API", result.WebServiceName);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Response);
            Assert.Equal("Success", result.Response.Result);
            Assert.Equal(150, result.Response.Amount);
        }

        [Fact]
        public async Task SendPostAsync_WithSuccessfulXmlResponse_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = new PayloadRequest<TestRequestDto>(
                "TEST_API",
                new TestRequestDto { Name = "Test", Value = 100 },
                ContentType.XML
            );

            var expectedResponse = new TestResponseDto { Result = "Success", Amount = 150 };
            var responseXml = "<?xml version=\"1.0\"?><TestResponseDto><Result>Success</Result><Amount>150</Amount></TestResponseDto>";

            SetupHttpResponse(HttpStatusCode.OK, responseXml);

            // Act
            var result = await _service.SendPostAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("TEST_API", result.WebServiceName);
            Assert.Equal("Success", result.Response.Result);
            Assert.Equal(150, result.Response.Amount);
        }

        [Fact]
        public async Task SendPostAsync_WithHttpError_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = new PayloadRequest<TestRequestDto>(
                "TEST_API",
                new TestRequestDto { Name = "Test", Value = 100 },
                ContentType.JSON
            );

            SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request Error");

            // Act
            var result = await _service.SendPostAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("TEST_API", result.WebServiceName);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Error en TEST_API: BadRequest", result.ErrorMessage);
        }

        [Fact]
        public async Task SendPostAsync_WithNetworkException_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = new PayloadRequest<TestRequestDto>(
                "TEST_API",
                new TestRequestDto { Name = "Test", Value = 100 },
                ContentType.JSON
            );

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Connection timeout"));

            // Act
            var result = await _service.SendPostAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("TEST_API", result.WebServiceName);
            Assert.Contains("Error al consultar TEST_API: Connection timeout", result.ErrorMessage);
        }

        [Fact]
        public async Task SendPostAsync_WithInvalidJsonResponse_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = new PayloadRequest<TestRequestDto>(
                "TEST_API",
                new TestRequestDto { Name = "Test", Value = 100 },
                ContentType.JSON
            );

            SetupHttpResponse(HttpStatusCode.OK, "Invalid JSON {");

            // Act
            var result = await _service.SendPostAsync(request);

            // Assert
            Assert.True(result.Success); // Aún es exitoso HTTP-wise
            Assert.Null(result.Response); // Pero la deserialización falló
        }

        [Theory]
        [InlineData(ContentType.JSON)]
        [InlineData(ContentType.XML)]
        public async Task SendPostAsync_WithDifferentContentTypes_ShouldSetCorrectHeaders(ContentType contentType)
        {
            // Arrange
            var request = new PayloadRequest<TestRequestDto>(
                "TEST_API",
                new TestRequestDto { Name = "Test", Value = 100 },
                contentType
            );

            HttpRequestMessage capturedRequest = null;
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"result\":\"Success\",\"amount\":150}")
                });

            // Act
            await _service.SendPostAsync(request);

            // Assert
            Assert.NotNull(capturedRequest);
            var expectedContentType = contentType == ContentType.JSON ? "application/json" : "application/xml";
            Assert.Equal(expectedContentType, capturedRequest.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task SendPostAsync_ShouldIncludeAuthorizationHeader()
        {
            // Arrange
            var request = new PayloadRequest<TestRequestDto>(
                "TEST_API",
                new TestRequestDto { Name = "Test", Value = 100 },
                ContentType.JSON
            );

            HttpRequestMessage capturedRequest = null;
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"result\":\"Success\",\"amount\":150}")
                });

            // Act
            await _service.SendPostAsync(request);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("Bearer", capturedRequest.Headers.Authorization?.Scheme);
            Assert.Equal("test-token", capturedRequest.Headers.Authorization?.Parameter);
        }

        [Theory]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        public async Task SendPostAsync_WithServerErrors_ShouldReturnFailureResponse(HttpStatusCode statusCode)
        {
            // Arrange
            var request = new PayloadRequest<TestRequestDto>(
                "TEST_API",
                new TestRequestDto { Name = "Test", Value = 100 },
                ContentType.JSON
            );

            SetupHttpResponse(statusCode, "Server Error");

            // Act
            var result = await _service.SendPostAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal((int)statusCode, result.StatusCode);
            Assert.Contains("Error en TEST_API", result.ErrorMessage);
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(content)
                });
        }
    }

    // DTOs de prueba
    public class TestRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class TestResponseDto
    {
        public string Result { get; set; } = string.Empty;
        public int Amount { get; set; }
    }
}