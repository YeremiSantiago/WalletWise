using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Budget;
using WalletWise.Domain.Common;

namespace WalletWise.Application.Interfaces
{
    public interface IBudgetService
    {
        Task<Result<BudgetResponseDto>> CreateBudgetAsync(CreateBudgetRequestDto dto);
        Task<Result<BudgetResponseDto>> GetBudgetByIdAsync(int id);
        Task<Result<IEnumerable<BudgetResponseDto>>> GetAllBudgetsAsync();
        Task<Result<IEnumerable<BudgetResponseDto>>> GetBudgetsByWalletAsync(int walletId);
        Task<Result<BudgetResponseDto>> UpdateBudgetAsync(int id, UpdateBudgetRequestDto dto);
        Task<Result<bool>> DeleteBudgetAsync(int id);
    }
}
