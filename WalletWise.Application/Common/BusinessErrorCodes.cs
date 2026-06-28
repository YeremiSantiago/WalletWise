namespace WalletWise.Application.Common
{
    public static class BusinessErrorCodes
    {
        public const string ERR_NOT_FOUND = "ERR_NOT_FOUND";
        public const string ERR_FORBIDDEN = "ERR_FORBIDDEN";
        public const string ERR_VALIDATION = "ERR_VALIDATION";
        public const string ERR_UNEXPECTED = "ERR_UNEXPECTED";
        
        // Custom business errors
        public const string ERR_CATEGORY_TYPE_MISMATCH = "ERR_CATEGORY_TYPE_MISMATCH";
        public const string ERR_CATEGORY_NAME_EXISTS = "ERR_CATEGORY_NAME_EXISTS";
        public const string ERR_INVALID_CREDENTIALS = "ERR_INVALID_CREDENTIALS";
        public const string ERR_USER_ALREADY_EXISTS = "ERR_USER_ALREADY_EXISTS";
        public const string ERR_UNAUTHORIZED = "ERR_UNAUTHORIZED";
    }
}
