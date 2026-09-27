using Microsoft.AspNetCore.Mvc;
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
            var error = result.Error ?? DomainErrors.General.Unexpected;
            var statusCode = error.StatusCode;

            var response = new ApiErrorResponse
            {
                Status = statusCode,
                Error = error.Code,
                Message = error.Message,
                TraceId = httpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }
    }
}
