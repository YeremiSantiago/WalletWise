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
        public const string ERR_INVALID_AMOUNT = "ERR_INVALID_AMOUNT";
        public const string ERR_FUTURE_DATE_NOT_ALLOWED = "ERR_FUTURE_DATE_NOT_ALLOWED";
        public const string ERR_WALLET_NOT_FOUND = "ERR_WALLET_NOT_FOUND";
        public const string ERR_CATEGORY_NOT_FOUND = "ERR_CATEGORY_NOT_FOUND";
        public const string ERR_CATEGORY_HAS_TRANSACTIONS = "ERR_CATEGORY_HAS_TRANSACTIONS";
        public const string ERR_INVALID_DATE_RANGE = "ERR_INVALID_DATE_RANGE";
        public const string ERR_INVALID_LIMIT = "ERR_INVALID_LIMIT";
        public const string ERR_OVERLAPPING_PERIODS = "ERR_OVERLAPPING_PERIODS";
        public const string ERR_WALLET_NAME_EXISTS = "ERR_WALLET_NAME_EXISTS";
        public const string ERR_CATEGORY_LIST_FAILED = "ERR_CATEGORY_LIST_FAILED";
    }
}
