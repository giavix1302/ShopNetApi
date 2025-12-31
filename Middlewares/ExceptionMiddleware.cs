using ShopNetApi.Exceptions;
using System.Net;
using System.Text.Json;

namespace ShopNetApi.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                await HandleException(context, ex.StatusCode, ex.Message);
            }
            catch (Exception)
            {
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
