using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using ExchangeRateOffers.Core.Application;
using ExchangeRateOffers.Infraestructure.Shared;
using ExchangeRateOffers.Infraestructure.Shared.Middleware;
using ExchangeRateOffers.Presentation.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddHttpContextAccessor();

builder.Services.AddSharedDependencies(builder.Configuration);
builder.Services.AddApplicationLayer(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddPresentationLayer();


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exchange Rate Offers API v1");
        c.RoutePrefix = "swagger";
    });
}
// Middleware de autenticación Firebase
app.UseMiddleware<FirebaseAuthMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
