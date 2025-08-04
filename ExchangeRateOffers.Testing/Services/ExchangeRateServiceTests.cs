using ExchangeRateOffers.Core.Application.Common;
using ExchangeRateOffers.Core.Application.Contanst;
using ExchangeRateOffers.Core.Application.Contract.Interfaces;
using ExchangeRateOffers.Core.Application.Contract.Services;
using ExchangeRateOffers.Core.Application.Dtos.InBound;
using ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock1;
using ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock2;
using ExchangeRateOffers.Core.Application.Dtos.OutBound.WebServiceMock3;
using Moq;

namespace ExchangeRateOffers.Tests.Unit.Services
{
    public class ExchangeRateServiceTests
    {
        private readonly Mock<IWebServiceConsultService<Mock1RequestDto, Mock1ResponseDto>> _mock1WebServiceMock;
        private readonly Mock<IWebServiceConsultService<Mock2RequestDto, Mock2ResponseDto>> _mock2WebServiceMock;
        private readonly Mock<IWebServiceConsultService<Mock3RequestDto, Mock3ResponseDto>> _mock3WebServiceMock;
        private readonly ExchangeRateService _exchangeRateService;

        public ExchangeRateServiceTests()
        {
            _mock1WebServiceMock = new Mock<IWebServiceConsultService<Mock1RequestDto, Mock1ResponseDto>>();
            _mock2WebServiceMock = new Mock<IWebServiceConsultService<Mock2RequestDto, Mock2ResponseDto>>();
            _mock3WebServiceMock = new Mock<IWebServiceConsultService<Mock3RequestDto, Mock3ResponseDto>>();

            _exchangeRateService = new ExchangeRateService(
                _mock1WebServiceMock.Object,
                _mock2WebServiceMock.Object,
                _mock3WebServiceMock.Object
            );
        }

        [Fact]
        public async Task GetBestOfferAsync_WhenAllProvidersSucceed_ShouldReturnBestOffer()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "DOP", 100);

            SetupMock1Success(55.5);
            SetupMock2Success(52.63);
            SetupMock3Success(58.82);

            // Act
            var result = await _exchangeRateService.GetBestOfferAsync(request);

            // Assert
            Assert.True(result.Successfull);
            Assert.Equal("Consulta Exitosa", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(WebServiceName.MOCK_WEB_WEBSERVICE_3, result.Data.BestOfferResponse.BestOffer.ProviderName);
            Assert.Equal(5882, result.Data.BestOfferResponse.BestOffer.ConvertedAmount);
            Assert.Equal(58.82, result.Data.BestOfferResponse.BestOffer.ExchangeRate);
            Assert.Equal(3, result.Data.BestOfferResponse.AllOffers.Count);
        }

        
        [Fact]
        public async Task GetBestOfferAsync_WhenAllProvidersFail_ShouldReturnError()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "EUR", 100);

            SetupMock1Failure();
            SetupMock2Failure();
            SetupMock3Failure();

            // Act
            var result = await _exchangeRateService.GetBestOfferAsync(request);

            // Assert
            Assert.False(result.Successfull);
            Assert.Equal("No se pudo obtener cotizaciones", result.Message);
            Assert.Equal("Todas las APIs fallaron", result.Error);
        }

        [Fact]
        public async Task GetBestOfferAsync_WhenMock1ThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "DOP", 100);
            _mock1WebServiceMock.Setup(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock1RequestDto>>()))
                .ThrowsAsync(new HttpRequestException("Connection timeout"));

            SetupMock2Success(52.63);
            SetupMock3Success(58.82);

            // Act
            var result = await _exchangeRateService.GetBestOfferAsync(request);

            // Assert
            Assert.True(result.Successfull);
            Assert.Equal(2, result.Data.BestOfferResponse.AllOffers.Count(x => x.IsSuccessful));
            Assert.Equal(WebServiceName.MOCK_WEB_WEBSERVICE_3, result.Data.BestOfferResponse.BestOffer.ProviderName);
        }

        [Theory]
        [InlineData("USD", "EUR", 1000, 0.85)]
        [InlineData("USD", "DOP", 100, 58.82)]
        [InlineData("EUR", "USD", 500, 1.18)]
        public async Task GetBestOfferAsync_WithDifferentCurrencies_ShouldCalculateCorrectly(
            string sourceCurrency, string targetCurrency, double amount, double expectedRate)
        {
            // Arrange
            var request = new ExchangeRequestDto(sourceCurrency, targetCurrency, amount);

            SetupMock1Success(expectedRate);
            SetupMock2Failure();
            SetupMock3Failure();

            // Act
            var result = await _exchangeRateService.GetBestOfferAsync(request);

            // Assert
            Assert.True(result.Successfull);
            Assert.Equal(expectedRate, result.Data.BestOfferResponse.BestOffer.ExchangeRate);
            Assert.Equal(amount * expectedRate, result.Data.BestOfferResponse.BestOffer.ConvertedAmount);
        }

        [Fact]
        public async Task GetBestOfferAsync_ShouldLogCorrectInformation()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "DOP", 100);

            SetupMock1Success(55.5);
            SetupMock2Success(52.63);
            SetupMock3Success(58.82);

            // Act
            var result = await _exchangeRateService.GetBestOfferAsync(request);

            // Assert
            Assert.True(result.Successfull);

            // Verificar que se llamaron todos los mocks
            _mock1WebServiceMock.Verify(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock1RequestDto>>()), Times.Once);
            _mock2WebServiceMock.Verify(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock2RequestDto>>()), Times.Once);
            _mock3WebServiceMock.Verify(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock3RequestDto>>()), Times.Once);
        }

        [Fact]
        public async Task GetBestOfferAsync_WhenProvidersReturnSameRate_ShouldReturnFirst()
        {
            // Arrange
            var request = new ExchangeRequestDto("USD", "DOP", 100);

            SetupMock1Success(55.0);
            SetupMock2Success(55.0);
            SetupMock3Success(55.0);

            // Act
            var result = await _exchangeRateService.GetBestOfferAsync(request);

            // Assert
            Assert.True(result.Successfull);
            // Debería devolver el primero en orden (MOCK_API_1)
            Assert.Equal(WebServiceName.MOCK_WEB_WEBSERVICE_1, result.Data.BestOfferResponse.BestOffer.ProviderName);
            Assert.Equal((double)5500m, result.Data.BestOfferResponse.BestOffer.ConvertedAmount);
        }

        private void SetupMock1Success(double rate)
        {
            var response = new PayloadResponse<Mock1ResponseDto>
            {
                Success = true,
                Response = new Mock1ResponseDto(rate),
                WebServiceName = WebServiceName.MOCK_WEB_WEBSERVICE_1
            };

            _mock1WebServiceMock.Setup(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock1RequestDto>>()))
                .ReturnsAsync(response);
        }

        private void SetupMock2Success(double rate)
        {
            var response = new PayloadResponse<Mock2ResponseDto>
            {
                Success = true,
                Response = new Mock2ResponseDto { Result = rate * 100 }, // Mock2 devuelve el resultado convertido
                WebServiceName = WebServiceName.MOCK_WEB_WEBSERVICE_2
            };

            _mock2WebServiceMock.Setup(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock2RequestDto>>()))
                .ReturnsAsync(response);
        }

        private void SetupMock3Success(double rate)
        {
            var response = new PayloadResponse<Mock3ResponseDto>
            {
                Success = true,
                Response = new Mock3ResponseDto(200, "Consulta", new Mock3DataDto(rate * 100)),
            };

            _mock3WebServiceMock.Setup(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock3RequestDto>>()))
                .ReturnsAsync(response);
        }

        private void SetupMock1Failure()
        {
            var response = new PayloadResponse<Mock1ResponseDto>
            {
                Success = false,
                ErrorMessage = "Mock1 service unavailable",
                WebServiceName = WebServiceName.MOCK_WEB_WEBSERVICE_1
            };

            _mock1WebServiceMock.Setup(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock1RequestDto>>()))
                .ReturnsAsync(response);
        }

        private void SetupMock2Failure()
        {
            var response = new PayloadResponse<Mock2ResponseDto>
            {
                Success = false,
                ErrorMessage = "Mock2 service unavailable",
                WebServiceName = WebServiceName.MOCK_WEB_WEBSERVICE_2
            };

            _mock2WebServiceMock.Setup(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock2RequestDto>>()))
                .ReturnsAsync(response);
        }

        private void SetupMock3Failure()
        {
            var response = new PayloadResponse<Mock3ResponseDto>
            {
                Success = false,
                ErrorMessage = "Mock3 service unavailable",
                WebServiceName = WebServiceName.MOCK_WEB_WEBSERVICE_3
            };

            _mock3WebServiceMock.Setup(x => x.SendPostAsync(It.IsAny<PayloadRequest<Mock3RequestDto>>()))
                .ReturnsAsync(response);
        }
    }
}