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
            { BusinessErrorCodes.ERR_USER_ALREADY_EXISTS, StatusCodes.Status409Conflict },
            { BusinessErrorCodes.ERR_INVALID_AMOUNT, StatusCodes.Status400BadRequest },
            { BusinessErrorCodes.ERR_FUTURE_DATE_NOT_ALLOWED, StatusCodes.Status400BadRequest },
            { BusinessErrorCodes.ERR_WALLET_NOT_FOUND, StatusCodes.Status404NotFound },
            { BusinessErrorCodes.ERR_CATEGORY_NOT_FOUND, StatusCodes.Status404NotFound },
            { BusinessErrorCodes.ERR_CATEGORY_HAS_TRANSACTIONS, StatusCodes.Status409Conflict },
            { BusinessErrorCodes.ERR_INVALID_DATE_RANGE, StatusCodes.Status422UnprocessableEntity },
            { BusinessErrorCodes.ERR_INVALID_LIMIT, StatusCodes.Status422UnprocessableEntity },
            { BusinessErrorCodes.ERR_OVERLAPPING_PERIODS, StatusCodes.Status422UnprocessableEntity },
            { BusinessErrorCodes.ERR_WALLET_NAME_EXISTS, StatusCodes.Status409Conflict },
            { BusinessErrorCodes.ERR_CATEGORY_LIST_FAILED, StatusCodes.Status500InternalServerError }
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

            return StatusCodes.Status422UnprocessableEntity;
        }
    }
}
