using System.Net;
using Microsoft.AspNetCore.Mvc;
using FootballPredictionAPI.Exceptions;

namespace FootballPredictionAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
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

        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            int statusCode = exception switch
            {
                UnauthorizedAccessException =>
                    StatusCodes.Status401Unauthorized,

                ConflictException =>
                    StatusCodes.Status409Conflict,

                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode == 500)
            {
                _logger.LogError(exception,
                    "An unexpected error occurred.");
            }
            else
            {
                _logger.LogWarning(
                    "Request failed with status {StatusCode}: {Message}",
                    statusCode,
                    exception.Message);
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    401 => "Unauthorized",
                    409 => "Conflict",
                    _ => "Internal Server Error"
                },
                Detail = statusCode == 500
                    ? "An unexpected server error occurred."
                    : exception.Message
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}