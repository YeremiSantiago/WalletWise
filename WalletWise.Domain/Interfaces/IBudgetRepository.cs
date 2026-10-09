using WalletWise.Domain.Entities;

namespace WalletWise.Domain.Interfaces
{
    public interface IBudgetRepository : IGenericRepository<Budget>
    {
        Task<Budget?> GetByIdForUserAsync(int id, string userId);
        Task<IEnumerable<Budget>> GetAllByUserIdAsync(string userId);
        Task<IEnumerable<Budget>> GetByWalletAsync(int walletId, string userId);
        Task<bool> HasOverlappingBudgetAsync(
            string userId, 
            int walletId, 
            int categoryId,
            DateTime startDate, 
            DateTime endDate,
            int? excludeBudgetId = null);
        Task<decimal> GetSpentAmountAsync(
            int walletId, 
            int categoryId, 
            DateTime startDate, 
            DateTime endDate, 
            string userId,
            int? excludeTransactionId = null);
        Task<Budget?> FindActiveBudgetAsync(
            int walletId, 
            int categoryId,
            DateTime transactionDate,
            string userId);
    }
}
