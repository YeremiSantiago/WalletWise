namespace WalletWise.Domain.Common
{
    public static class DomainErrors
    {
        public static class General
        {
            public static readonly Error NotFound = new("General.NotFound", "El recurso solicitado no fue encontrado.", 404);
            public static readonly Error Forbidden = new("General.Forbidden", "No tienes permisos para acceder a este recurso.", 403);
            public static readonly Error Validation = new("General.Validation", "Errores de validación encontrados.", 400);
            public static readonly Error Unexpected = new("General.Unexpected", "Ha ocurrido un error inesperado.", 500);
            public static readonly Error Unauthorized = new("General.Unauthorized", "No estás autorizado.", 401);
        }

        public static class Category
        {
            public static readonly Error TypeMismatch = new("Category.TypeMismatch", "El tipo de la categoría no coincide con la operación.", 422);
            public static readonly Error NameExists = new("Category.NameExists", "Ya existe una categoría con ese nombre.", 409);
            public static readonly Error NotFound = new("Category.NotFound", "La categoría no fue encontrada.", 404);
            public static readonly Error HasTransactions = new("Category.HasTransactions", "La categoría tiene transacciones asociadas.", 409);
            public static readonly Error ListFailed = new("Category.ListFailed", "Error al listar las categorías.", 500);
        }

        public static class Wallet
        {
            public static readonly Error NotFound = new("Wallet.NotFound", "La billetera no fue encontrada.", 404);
            public static readonly Error NameExists = new("Wallet.NameExists", "Ya existe una billetera con ese nombre.", 409);
        }

        public static class Transaction
        {
            public static readonly Error InvalidAmount = new("Transaction.InvalidAmount", "El monto no es válido.", 400);
            public static readonly Error FutureDateNotAllowed = new("Transaction.FutureDateNotAllowed", "No se permiten fechas futuras.", 400);
            public static readonly Error NotFound = new("Transaction.NotFound", "La transacción no fue encontrada.", 404);
        }

        public static class Auth
        {
            public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Credenciales inválidas.", 401);
            public static readonly Error UserAlreadyExists = new("Auth.UserAlreadyExists", "El usuario ya existe.", 409);
            public static readonly Error UserNotFound = new("Auth.UserNotFound", "El usuario no fue encontrado.", 404);
        }

        public static class Report
        {
            public static readonly Error InvalidDateRange = new("Report.InvalidDateRange", "El rango de fechas no es válido.", 422);
            public static readonly Error InvalidLimit = new("Report.InvalidLimit", "El límite provisto no es válido.", 422);
            public static readonly Error OverlappingPeriods = new("Report.OverlappingPeriods", "Los periodos se superponen.", 422);
        }
    }
}
