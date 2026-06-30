using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Domain.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get;}
        /// <summary>
        /// siempre un código de BusinessErrorCodes, nunca texto libre.
        /// </summary>
        public string? Error { get;}
        public string? Message { get; }
        public T? Value { get; }

        private Result(bool isSuccess, string? error, string? message, T? value)
        {
            this.IsSuccess = isSuccess;
            this.Error = error;
            this.Message = message;
            this.Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new(true, null, null, value);
        }
        public static Result<T> Failure(string errorCode, string? message = null)
        {
            return new(false, errorCode, message, default);
        }

       
    }
}
