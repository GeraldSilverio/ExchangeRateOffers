using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using ExchangeRateOffers.Mock1.Middleware;
using ExchangeRateOffers.Mock1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add Firebase Authentication Middleware
app.UseMiddleware<FirebaseAuthMiddleware>();

app.MapControllers();

app.Run();