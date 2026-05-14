using System.Net;
using System.Text.Json;
using Catalog.API.Domain.Exceptions;

namespace Catalog.API.Infrastructure.Middleware
{
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
            // Domain exceptions are safe to show to clients; unexpected exceptions get a generic message.
            var (statusCode, message) = exception switch
            {
                NotFoundException         => (HttpStatusCode.NotFound,            exception.Message),
                BadRequestException       => (HttpStatusCode.BadRequest,          exception.Message),
                ConflictException         => (HttpStatusCode.Conflict,            exception.Message),
                ForbiddenException        => (HttpStatusCode.Forbidden,           exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,      "Unauthorized access."),
                _                         => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            // Client-side problems should be visible in logs, but only server faults need full stack traces.
            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Unhandled exception occurred");
            else
                _logger.LogWarning("Handled exception: {Message}", exception.Message);

            // Keep the error shape consistent across controllers and clients.
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
