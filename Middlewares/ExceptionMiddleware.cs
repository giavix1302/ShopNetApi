using ShopNetApi.Exceptions;
using System.Net;
using System.Text.Json;

namespace ShopNetApi.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex,
                    "Handled application exception. Path={Path}",
                    context.Request.Path);

                await HandleException(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex,
                    "Unhandled exception. Path={Path}",
                    context.Request.Path);

                await HandleException(
                    context,
                    (int)HttpStatusCode.InternalServerError,
                    "Internal server error"
                );
            }
        }

        private static async Task HandleException(
            HttpContext context,
            int statusCode,
            string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                success = false,
                message,
                statusCode
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );
        }
    }
}
