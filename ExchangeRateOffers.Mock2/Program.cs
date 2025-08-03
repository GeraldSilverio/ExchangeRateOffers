using ExchangeRateOffers.Mock2.Services;
using ExchangeRateOffers.Mock2.Middleware;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddXmlSerializerFormatters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Exchange Rate Mock API 2 (XML)", Version = "v1" });
});

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// Register services
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

// Initialize Firebase
var firebaseConfigPath = builder.Configuration["Firebase:ServiceAccountKeyPath"];
if (!string.IsNullOrEmpty(firebaseConfigPath) && File.Exists(firebaseConfigPath))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(firebaseConfigPath)
    });
}
else
{
    // For development, you might want to use default credentials
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.GetApplicationDefault()
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exchange Rate Mock API 2 (XML) v1");
    });
}

app.UseHttpsRedirection();

// Add Firebase Authentication Middleware
app.UseMiddleware<FirebaseAuthMiddleware>();

app.MapControllers();

app.Run();