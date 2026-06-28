using Microsoft.AspNetCore.Mvc;
using WalletWise.Application.Common;
using WalletWise.Domain.Common;

namespace WalletWise.WebApi.Common
{
    public static class ResultExtensions
    {
        public static ActionResult ToOkResult<T>(this Result<T> result, HttpContext httpContext)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(result.Value);
            }

            return CreateErrorResponse(result, httpContext);
        }

        public static ActionResult ToCreatedResult<T>(this Result<T> result, HttpContext httpContext, string actionName, object routeValues)
        {
            if (result.IsSuccess)
            {
                if (string.IsNullOrEmpty(actionName))
                {
                    return new CreatedResult(string.Empty, result.Value);
                }
                return new CreatedAtActionResult(actionName, null, routeValues, result.Value);
            }

            return CreateErrorResponse(result, httpContext);
        }

        public static ActionResult ToNoContentResult<T>(this Result<T> result, HttpContext httpContext)
        {
            if (result.IsSuccess)
            {
                return new NoContentResult();
            }

            return CreateErrorResponse(result, httpContext);
        }

        private static ActionResult CreateErrorResponse<T>(Result<T> result, HttpContext httpContext)
        {
            var statusCode = BusinessErrorHttpMap.GetStatusCode(result.Error!);

            var response = new ApiErrorResponse
            {
                Status = statusCode,
                Error = result.Error!,
                // Assuming result.Error is the business error code, we need a friendly message.
                // For now, if the original codebase used result.Error as a message, we might need a mapping for messages too.
                // Wait! In the prompt, the user specified that result.Error should be the stable code. But currently, result.Error is often a Spanish string like "No se pudo obtener...".
                // I'll need to use result.Error as the code, and maybe result.Message if it existed, but Result<T> only has Error.
                // The prompt says: "El campo 'error' lleva el código estable... nunca texto libre. El campo 'message' es el texto legible en español".
                // Let's modify Result<T> to contain both Code and Message if it doesn't already? Or just map it here?
                // The prompt says: "construye el ApiErrorResponse usando BusinessErrorHttpMap para resolver el status a partir de result.Error".
                // If Result<T>.Error holds the code, where does the message come from? Maybe we should map standard messages based on the code?
                // Let's map a generic message here if we don't have a specific one, or let's assume we'll fix the Result.Failure calls to pass a code, and we can map messages.
                Message = GetFriendlyMessage(result.Error!),
                TraceId = httpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }

        private static string GetFriendlyMessage(string errorCode)
        {
            return errorCode switch
            {
                BusinessErrorCodes.ERR_NOT_FOUND => "El recurso solicitado no fue encontrado.",
                BusinessErrorCodes.ERR_FORBIDDEN => "No tienes permisos para acceder a este recurso.",
                BusinessErrorCodes.ERR_VALIDATION => "Errores de validación encontrados.",
                BusinessErrorCodes.ERR_UNEXPECTED => "Ha ocurrido un error inesperado.",
                BusinessErrorCodes.ERR_CATEGORY_TYPE_MISMATCH => "El tipo de la categoría no coincide con la operación.",
                BusinessErrorCodes.ERR_CATEGORY_NAME_EXISTS => "Ya existe una categoría con ese nombre.",
                BusinessErrorCodes.ERR_INVALID_CREDENTIALS => "Credenciales inválidas.",
                BusinessErrorCodes.ERR_USER_ALREADY_EXISTS => "El usuario ya existe.",
                _ => "Se produjo un error de validación de negocio."
            };
        }
    }
}
