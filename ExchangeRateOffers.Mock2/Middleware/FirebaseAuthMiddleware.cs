using FirebaseAdmin.Auth;
using System.Xml.Serialization;
using ExchangeRateOffers.Mock2.Dtos;

namespace ExchangeRateOffers.Mock2.Middleware
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
            // Skip authentication for health checks and other non-API endpoints
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
            context.Response.ContentType = "application/xml";

            var errorResponse = new ErrorResult
            {
                Code = statusCode,
                Message = message
            };

            var xml = SerializeToXml(errorResponse);
            await context.Response.WriteAsync(xml);
        }

        private string SerializeToXml<T>(T obj)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var writer = new StringWriter();
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serializing error response to XML");
                return "<Error><Code>500</Code><Message>Serialization error</Message></Error>";
            }
        }
    }
}