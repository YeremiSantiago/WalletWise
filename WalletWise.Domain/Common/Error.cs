namespace WalletWise.Domain.Common
{
    public record Error(string Code, string Message, int StatusCode)
    {
        public static readonly Error None = new(string.Empty, string.Empty, 200);
        public static readonly Error NullValue = new("Error.NullValue", "Se proporcionó un valor nulo", 400);

        public Error WithMessage(string message) => this with { Message = message };
    }
}
