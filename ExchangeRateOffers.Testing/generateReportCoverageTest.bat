dotnet test --collect:"XPlat Code Coverage" --results-directory . -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="[ExchangeRateOffers.Presentation.Api.Controllers,ExchangeRateOffers.Core.Application.Services]*"
for /f "delims=" %%i in ('dir /b /ad-h /t:c /od') do set latestResultDir=%%i
reportgenerator -reports:%cd%\%latestResultDir%\coverage.cobertura.xml -targetdir:"coveragereport" -reporttypes:Html
rmdir /s /q %latestResultDir%
start %cd%\coveragereport\index.html