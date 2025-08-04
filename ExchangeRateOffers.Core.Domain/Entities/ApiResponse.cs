namespace ExchangeRateOffers.Core.Domain.Entities
{
    /// <summary>
    /// Respuesta estándar
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Successfull { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public T? Data { get; set; }

        private ApiResponse(bool successfull, T? data = default, string? message = null, string? error = null)
        {
            Successfull = successfull;
            Data = data;
            Message = message;
            Error = error;
        }

        /// <summary>
        /// Respuesta exitosa con datos
        /// </summary>
        public static ApiResponse<T> Success(T data, string? message = null)
        {
            return new ApiResponse<T>(true, data, message);
        }

        /// <summary>
        /// Respuesta fallida por error del cliente (validación, datos incorrectos)
        /// </summary>
        public static ApiResponse<T> Fail(string error, string? message = null)
        {
            return new ApiResponse<T>(false, default, message, error);
        }

        /// <summary>
        /// Respuesta de error del servidor
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, string? error = null)
        {
            return new ApiResponse<T>(false, default, message, error);
        }
    }
}
