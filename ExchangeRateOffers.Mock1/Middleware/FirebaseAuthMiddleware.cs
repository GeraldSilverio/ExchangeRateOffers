using ExchangeRateOffers.Mock1.Dtos;
using FirebaseAdmin.Auth;
using System.Text.Json;

namespace ExchangeRateOffers.Mock1.Middleware
{
    public class FirebaseAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FirebaseAuthMiddleware> _logger;

        public FirebaseAuthMiddleware(RequestDelegate next, ILogger<FirebaseAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            try
            {
                var token = ExtractTokenFromHeader(context.Request);

                if (string.IsNullOrEmpty(token))
                {
                    await WriteErrorResponse(context, 401, "Missing authorization token");
                    return;
                }

                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

                // Add user info to context for later use
                context.Items["FirebaseUser"] = decodedToken;
                context.Items["UserId"] = decodedToken.Uid;

                _logger.LogDebug("Successfully authenticated user: {UserId}", decodedToken.Uid);

                await _next(context);
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogWarning("Firebase authentication failed: {Message}", ex.Message);
                await WriteErrorResponse(context, 401, "Invalid authentication token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authentication");
                await WriteErrorResponse(context, 500, "Internal server error");
            }
        }

        private string ExtractTokenFromHeader(HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"].FirstOrDefault();

            if (authHeader?.StartsWith("Bearer ") == true)
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }

        private async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = new ErrorResponse(GetErrorType(statusCode), message, statusCode);
    
            var json = JsonSerializer.Serialize(errorResponse);

            await context.Response.WriteAsync(json);
        }

        private string GetErrorType(int statusCode)
        {
            return statusCode switch
            {
                401 => "Unauthorized",
                403 => "Forbidden",
                500 => "InternalServerError",
                _ => "Error"
            };
        }
    }
}
