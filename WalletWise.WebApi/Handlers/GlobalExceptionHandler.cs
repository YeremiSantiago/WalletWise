using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WalletWise.Application.Common;
using WalletWise.Application.Exceptions;
using WalletWise.WebApi.Common;

namespace WalletWise.WebApi.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }


        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, errorCode, message) = exception switch
            {
                NotFoundException => 
                    (StatusCodes.Status404NotFound, BusinessErrorCodes.ERR_NOT_FOUND, exception.Message ?? "El recurso solicitado no fue encontrado."),
                ForbiddenAccessException => 
                    (StatusCodes.Status403Forbidden, BusinessErrorCodes.ERR_FORBIDDEN, "No tienes permisos para acceder a este recurso."),
                _ => 
                    (StatusCodes.Status500InternalServerError,BusinessErrorCodes.ERR_UNEXPECTED, "Ha ocurrido un error inesperado.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred");
            }
            else
            {
                _logger.LogWarning(exception, "Domain exception occurred");
            }

            var apiErrorResponse = new ApiErrorResponse
            {
                Status = statusCode,
                Error = errorCode,
                Message = message,
                TraceId = httpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };   

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(apiErrorResponse, cancellationToken);

            return true;
        }

        
    }
}
