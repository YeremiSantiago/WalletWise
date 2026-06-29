using System;

namespace WalletWise.Application.Common
{
    public class ApiErrorResponse
    {
        public int Status { get; set; }
        public required string Error { get; set; }
        public required string Message { get; set; }
        public required string TraceId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
