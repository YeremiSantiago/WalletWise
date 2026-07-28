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
                Message = result.Message ?? GetFriendlyMessage(result.Error!),
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
                BusinessErrorCodes.ERR_INVALID_AMOUNT => "El monto no es válido.",
                BusinessErrorCodes.ERR_FUTURE_DATE_NOT_ALLOWED => "No se permiten fechas futuras.",
                BusinessErrorCodes.ERR_WALLET_NOT_FOUND => "La billetera no fue encontrada.",
                BusinessErrorCodes.ERR_CATEGORY_NOT_FOUND => "La categoría no fue encontrada.",
                BusinessErrorCodes.ERR_CATEGORY_HAS_TRANSACTIONS => "La categoría tiene transacciones asociadas.",
                BusinessErrorCodes.ERR_INVALID_DATE_RANGE => "El rango de fechas no es válido.",
                BusinessErrorCodes.ERR_INVALID_LIMIT => "El límite provisto no es válido.",
                BusinessErrorCodes.ERR_OVERLAPPING_PERIODS => "Los periodos se superponen.",
                BusinessErrorCodes.ERR_WALLET_NAME_EXISTS => "Ya existe una billetera con ese nombre.",
                BusinessErrorCodes.ERR_CATEGORY_LIST_FAILED => "Error al listar las categorías.",
                _ => "Se produjo un error de validación de negocio."
            };
        }
    }
}
