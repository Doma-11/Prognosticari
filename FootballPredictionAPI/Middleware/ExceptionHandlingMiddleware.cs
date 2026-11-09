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
                ArgumentException =>
                    StatusCodes.Status400BadRequest,

                KeyNotFoundException =>
                    StatusCodes.Status404NotFound,

                ForbiddenException =>
                    StatusCodes.Status403Forbidden,

                ConflictException =>
                    StatusCodes.Status409Conflict,

                InvalidOperationException =>
                    StatusCodes.Status409Conflict,

                UnauthorizedAccessException =>
                    StatusCodes.Status401Unauthorized,

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
                    400 => "Bad Request",
                    401 => "Unauthorized",
                    403 => "Forbidden",
                    404 => "Not Found",
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