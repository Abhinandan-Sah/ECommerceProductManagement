using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gateway.API.Middleware
{
    /// <summary>
    /// Converts unhandled gateway exceptions into a consistent JSON response.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Creates the gateway exception middleware with the next pipeline delegate and logging services.
        /// </summary>
        /// <param name="next">Next middleware in the ASP.NET Core request pipeline.</param>
        /// <param name="logger">Logger used to record unhandled gateway exceptions.</param>
        /// <param name="environment">Hosting environment used to decide whether diagnostic detail is returned.</param>
        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next, 
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Runs the next middleware and handles any unhandled exception with a gateway response.
        /// </summary>
        /// <param name="context">HTTP context for the current gateway request.</param>
        /// <returns>A task that completes when request processing finishes.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The API Gateway caught an unhandled exception.");
                await HandleExceptionAsync(context, ex, _environment);
            }
        }

        /// <summary>
        /// Writes the gateway error response without leaking downstream exception details in production.
        /// </summary>
        /// <param name="context">HTTP context whose response should be written.</param>
        /// <param name="exception">Unhandled exception caught by the gateway.</param>
        /// <param name="environment">Hosting environment used to control diagnostic detail.</param>
        /// <returns>A task that completes when the JSON response has been written.</returns>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment environment)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // The gateway should not leak downstream details in production, but local debugging still needs a hint.
            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An internal server error occurred while routing your request.",
                Detail = environment.IsDevelopment() ? exception.Message : null
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
