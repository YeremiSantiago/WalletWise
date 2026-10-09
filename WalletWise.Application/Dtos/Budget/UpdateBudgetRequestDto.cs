using WalletWise.Domain.Common.Enums;

namespace WalletWise.Application.Dtos.Budget
{
    public class UpdateBudgetRequestDto
    {
        public decimal Amount { get; set; }
        public BudgetMode Mode { get; set; }
    }
}
