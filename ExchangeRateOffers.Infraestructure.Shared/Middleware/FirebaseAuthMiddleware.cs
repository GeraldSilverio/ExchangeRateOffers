using ExchangeRateOffers.Core.Domain.Entities;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace ExchangeRateOffers.Infraestructure.Shared.Middleware
{
    [ExcludeFromCodeCoverage]

    /// <summary>
    /// Middleware para autenticación con Firebase en la API principal
    /// </summary>
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
            try
            {
                var token = ExtractTokenFromHeader(context.Request);

                if (string.IsNullOrEmpty(token))
                {
                    await WriteUnauthorizedResponse(context, "Token de autorización requerido");
                    return;
                }

                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

                // Agregar información del usuario al contexto
                context.Items["FirebaseUser"] = decodedToken;
                context.Items["Token"] = token;
                context.Items["UserId"] = decodedToken.Uid;
                context.Items["UserEmail"] = decodedToken.Claims.GetValueOrDefault("email")?.ToString();

                _logger.LogDebug("Usuario autenticado exitosamente: {UserId} ({Email})",
                    decodedToken.Uid, decodedToken.Claims.GetValueOrDefault("email"));

                await _next(context);
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogWarning("Autenticación Firebase fallida: {Message}", ex.Message);
                await WriteUnauthorizedResponse(context, "Token de autenticación inválido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante la autenticación");
                await WriteErrorResponse(context, 500, "Error interno del servidor");
            }
        }
        private static string? ExtractTokenFromHeader(HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"].FirstOrDefault();

            if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return authHeader;
        }

        private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
        {
            await WriteErrorResponse(context, 401, message);
        }

        private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var errorResponse =  ApiResponse<string>.Fail(GetErrorType(statusCode), message);

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        private static string GetErrorType(int statusCode)
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
