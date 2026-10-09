using WalletWise.Domain.Common.Enums;

namespace WalletWise.Application.Dtos.Budget
{
    public class BudgetResponseDto
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BudgetMode Mode { get; set; }

        // Valores calculados
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal UsagePercentage { get; set; }
        public bool IsExceeded { get; set; }

    }
}
