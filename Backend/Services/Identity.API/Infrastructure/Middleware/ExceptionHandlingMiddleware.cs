using System.Net;
using System.Text.Json;
using Identity.API.Domain.Exceptions;

namespace Identity.API.Infrastructure.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Catches all unhandled exceptions and returns a consistent JSON error response.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Map exception type to HTTP status code
            var (statusCode, message) = exception switch
            {
                NotFoundException      => (HttpStatusCode.NotFound,            exception.Message),
                BadRequestException    => (HttpStatusCode.BadRequest,          exception.Message),
                ConflictException      => (HttpStatusCode.Conflict,            exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,   "Unauthorized access."),
                _                      => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            // Log: Warning for 4xx client errors, Error for 5xx server errors
            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Unhandled exception occurred");
            else
                _logger.LogWarning("Handled exception: {Message}", exception.Message);

            // Return a clean, consistent JSON response
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                StatusCode = (int)statusCode,
                Message = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
