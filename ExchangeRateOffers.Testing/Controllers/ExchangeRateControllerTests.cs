using ExchangeRateOffers.Core.Application.Contract.Interfaces;
using ExchangeRateOffers.Core.Application.Dtos.InBound;
using ExchangeRateOffers.Core.Domain.Entities;
using ExchangeRateOffers.Core.Domain.Exceptions;
using ExchangeRateOffers.Presentation.Api.Controllers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ExchangeRateOffers.Tests.Unit.Controllers
{
    public class ExchangeRateControllerTests
    {
        private readonly Mock<IExchangeRateService> _exchangeRateServiceMock;
        private readonly Mock<IValidator<ExchangeRequestDto>> _validatorMock;
        private readonly ExchangeRateController _controller;

        public ExchangeRateControllerTests()
        {
            _exchangeRateServiceMock = new Mock<IExchangeRateService>();
            _validatorMock = new Mock<IValidator<ExchangeRequestDto>>();
            _controller = new ExchangeRateController(_exchangeRateServiceMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task GetBestOffer_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD","DOP", 100);

            var validationResult = new ValidationResult();
            _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var serviceResponse = CreateSuccessfulServiceResponse();
            _exchangeRateServiceMock.Setup(x => x.GetBestOfferAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetBestOffer(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<ExchangeResponseDto>>(okResult.Value);
            Assert.True(response.Successfull);
            Assert.Equal("Consulta Exitosa", response.Message);
        }

        [Fact]
        public async Task GetBestOffer_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new ExchangeRequestDto("US", "DOP", 100);


            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("SourceCurrency", "La divisa de origen debe tener exactamente 3 caracteres")
            });

            _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.GetBestOffer(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<ExchangeResponseDto>>(badRequestResult.Value);
            Assert.False(response.Successfull);
            Assert.Equal("Errores de validación en la solicitud", response.Message);
        }

        [Fact]
        public async Task GetBestOffer_WithServiceFailure_ReturnsBadRequest()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "DOP", 100);

            var validationResult = new ValidationResult();
            _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var serviceResponse = ApiResponse<ExchangeResponseDto>.Fail("No se pudo obtener cotizaciones", "Todas las APIs fallaron");
            _exchangeRateServiceMock.Setup(x => x.GetBestOfferAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetBestOffer(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<ExchangeResponseDto>>(badRequestResult.Value);
            Assert.False(response.Successfull);
        }


        [Fact]
        public async Task GetBestOffer_WithAllProvidersFailedException_ReturnsServiceUnavailable()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "DOP", 100);

            var validationResult = new ValidationResult();
            _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _exchangeRateServiceMock.Setup(x => x.GetBestOfferAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AllProvidersFailedException(new List<string> { "Todos los proveedores fallaron al obtener tasas de cambio" }));

            // Act
            var result = await _controller.GetBestOffer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(503, statusCodeResult.StatusCode);
            var response = Assert.IsType<ApiResponse<ExchangeResponseDto>>(statusCodeResult.Value);
            Assert.Equal("Todos los proveedores fallaron al obtener tasas de cambio", response.Message);
        }

        [Fact]
        public async Task GetBestOffer_WithUnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "DOP", 100);


            var validationResult = new ValidationResult();
            _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _exchangeRateServiceMock.Setup(x => x.GetBestOfferAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error inesperado"));

            // Act
            var result = await _controller.GetBestOffer(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = Assert.IsType<ApiResponse<ExchangeResponseDto>>(statusCodeResult.Value);
            Assert.Equal("Error interno del servidor", response.Message);
        }

        [Theory]
        [InlineData("", "DOP", 100)]
        [InlineData("USD", "", 100)]
        [InlineData("USD", "DOP", 0)]
        [InlineData("USD", "DOP", -100)]
        public async Task GetBestOffer_WithInvalidData_ReturnsBadRequest(string sourceCurrency, string targetCurrency, double amount)
        {
            // Arrange
            var request = new ExchangeRequestDto(sourceCurrency, targetCurrency, amount);

            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("Request", "Datos inválidos")
            });

            _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.GetBestOffer(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<ExchangeResponseDto>>(badRequestResult.Value);
            Assert.False(response.Successfull);
            Assert.Equal("Errores de validación en la solicitud", response.Message);
        }

        [Fact]
        public async Task GetBestOffer_WithCancellationToken_PassesToService()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "DOP", 100);


            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var validationResult = new ValidationResult();
            _validatorMock.Setup(x => x.ValidateAsync(request, cancellationToken))
                .ReturnsAsync(validationResult);

            var serviceResponse = CreateSuccessfulServiceResponse();
            _exchangeRateServiceMock.Setup(x => x.GetBestOfferAsync(request, cancellationToken))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetBestOffer(request, cancellationToken);

            // Assert
            _exchangeRateServiceMock.Verify(x => x.GetBestOfferAsync(request, cancellationToken), Times.Once);
        }

        private ApiResponse<ExchangeResponseDto> CreateSuccessfulServiceResponse()
        {
            var bestOffer = new ExchangeOffer("MOCK_API_3", 5882, 58.82, true);
            var allOffers = new List<ExchangeOffer>
            {
                new ExchangeOffer("MOCK_API_1", 5550, 55.5, true),
                new ExchangeOffer("MOCK_API_2", 5263, 52.63, true),
                bestOffer
            };

            var bestOfferResult = new BestOfferResult(bestOffer, allOffers);
            var exchangeResponseDto = new ExchangeResponseDto(bestOfferResult);

            return ApiResponse<ExchangeResponseDto>.Success(exchangeResponseDto, "Consulta Exitosa");
        }
    }
}
