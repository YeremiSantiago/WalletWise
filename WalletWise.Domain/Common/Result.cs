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
        public string? Error { get;}
        public T? Value { get; }

        private Result(bool isSuccess, string? error, T? value)
        {
            this.IsSuccess = isSuccess;
            this.Error = error;
            this.Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new(true, null, value);
        }
        public static Result<T> Failure(string error)
        {
            return new(false, error, default);

        }

       
    }
}
