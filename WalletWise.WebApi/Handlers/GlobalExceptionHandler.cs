using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WalletWise.Application.Exceptions;
using WalletWise.Domain.Common;
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
            var error = exception switch
            {
                NotFoundException => DomainErrors.General.NotFound,
                ForbiddenAccessException => DomainErrors.General.Forbidden,
                UniqueConstraintViolationException => new Error("General.Conflict", "Hubo un conflicto con los datos proporcionados.", StatusCodes.Status409Conflict),
                _ => DomainErrors.General.Unexpected
            };

            var statusCode = error.StatusCode;
            var errorCode = error.Code;
            var message = exception is NotFoundException ? (exception.Message ?? error.Message) : error.Message;

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
