using System;

namespace WalletWise.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public Error? Error { get; }
        public T? Value { get; }

        private Result(bool isSuccess, Error? error, T? value)
        {
            IsSuccess = isSuccess;
            Error = error;
            Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new(true, null, value);
        }

        public static Result<T> Failure(Error error)
        {
            return new(false, error, default);
        }
    }
}
