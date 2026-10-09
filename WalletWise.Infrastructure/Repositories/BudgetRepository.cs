using Microsoft.EntityFrameworkCore;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Infrastructure.Context;

namespace WalletWise.Infrastructure.Repositories
{
    public class BudgetRepository : GenericRepository<Budget>, IBudgetRepository
    {
        public BudgetRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<Budget?> FindActiveBudgetAsync(int walletId, int categoryId, DateTime transactionDate, string userId)
        {
            return await _context.Budgets.Where(x => x.WalletId == walletId
            && x.CategoryId == categoryId
            && x.UserId == userId
            && transactionDate >= x.StartDate
            && transactionDate <= x.EndDate)
                 .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Budget>> GetAllByUserIdAsync(string userId)
        {
            return await _context.Budgets.Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<Budget?> GetByIdForUserAsync(int id, string userId)
        {

            return await _context.Budgets.Where(x => x.Id == id && x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Budget>> GetByWalletAsync(int walletId, string userId)
        {
            return await _context.Budgets.Where(x => x.WalletId == walletId && x.UserId == userId)
                .ToListAsync();
        }

        public async Task<decimal> GetSpentAmountAsync(int walletId, int categoryId, DateTime startDate, DateTime endDate, string userId, int? excludeTransactionId = null)
        {
            var query = _context.Transactions
                .Where(x => x.UserId == userId
                && x.WalletId == walletId
                && x.CategoryId == categoryId
                && x.Type == TypeTransaction.Expense
                && x.Date >= startDate
                && x.Date <= endDate);

            return await query.SumAsync(x => x.Amount);
        }

        public async Task<bool> HasOverlappingBudgetAsync(string userId, int walletId, int categoryId, DateTime startDate, DateTime endDate, int? excludeBudgetId = null)
        {
            var query = _context.Budgets
                .Where(x => x.UserId == userId
                && x.WalletId == walletId
                && x.CategoryId == categoryId);

            if (excludeBudgetId.HasValue)
            {
                query = query.Where(x => x.Id != excludeBudgetId.Value);
            }

            return await query.AnyAsync(x =>
                x.StartDate <= endDate && x.EndDate >= startDate);
        }
    }
}
