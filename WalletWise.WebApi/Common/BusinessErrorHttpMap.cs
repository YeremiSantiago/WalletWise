using System.Net;
using WalletWise.Application.Common;

namespace WalletWise.WebApi.Common
{
    public static class BusinessErrorHttpMap
    {
        private static readonly Dictionary<string, int> _map = new()
        {
            { BusinessErrorCodes.ERR_NOT_FOUND, StatusCodes.Status404NotFound },
            { BusinessErrorCodes.ERR_FORBIDDEN, StatusCodes.Status403Forbidden },
            { BusinessErrorCodes.ERR_VALIDATION, StatusCodes.Status400BadRequest },
            { BusinessErrorCodes.ERR_UNEXPECTED, StatusCodes.Status500InternalServerError },
            { BusinessErrorCodes.ERR_UNAUTHORIZED, StatusCodes.Status401Unauthorized },
            { BusinessErrorCodes.ERR_CATEGORY_TYPE_MISMATCH, StatusCodes.Status422UnprocessableEntity },
            { BusinessErrorCodes.ERR_CATEGORY_NAME_EXISTS, StatusCodes.Status409Conflict },
            { BusinessErrorCodes.ERR_INVALID_CREDENTIALS, StatusCodes.Status401Unauthorized },
            { BusinessErrorCodes.ERR_USER_ALREADY_EXISTS, StatusCodes.Status409Conflict }
        };

        public static int GetStatusCode(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                return StatusCodes.Status500InternalServerError;
            }

            if (_map.TryGetValue(errorCode, out var statusCode))
            {
                return statusCode;
            }

            // Default to UnprocessableEntity for unknown business rules
            return StatusCodes.Status422UnprocessableEntity;
        }
    }
}
