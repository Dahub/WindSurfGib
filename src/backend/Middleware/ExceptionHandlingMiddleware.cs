using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WindSurfApi.Exceptions;

namespace WindSurfApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public class ErrorResponse
        {
            public string Message { get; set; }
            public string Type { get; set; }
        }

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogError(ex, "Une erreur est survenue lors du traitement de la requête");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            
            var errorResponse = new ErrorResponse
            {
                Type = exception.GetType().Name,
                Message = exception.Message
            };

            switch (exception)
            {
                case ResourceNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case InvalidBusinessDataException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case BusinessException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "Une erreur interne est survenue.";
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }
}
