using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gateway.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // This line passes the request to Ocelot.
                // If anything goes wrong during routing, it falls into the catch block.
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The API Gateway caught an unhandled exception.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 1. Force the response to be JSON
            context.Response.ContentType = "application/json";

            // 2. Set the standard 500 Internal Server Error code
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // 3. Create a standardized error object
            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An internal server error occurred while routing your request.",
                // NOTE: We expose the exception message here for your training environment.
                // In a real production app, you would hide the exception details from the client!
                DeveloperDetails = exception.Message
            };

            // 4. Serialize and return the JSON
            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}