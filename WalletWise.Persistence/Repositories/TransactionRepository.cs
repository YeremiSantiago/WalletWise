using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Persistence.Context;
namespace WalletWise.Persistence.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context ) : base(context)
        {
        }

        public async Task<bool> ExistsTransactionByCategoryAsync(int IdCategory, string userId)
        {
            return await _context.Transactions.
                AnyAsync(x => x.CategoryId == IdCategory && x.UserId == userId);

        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsByCategoryAsync(string userId, int idCategory)
        {
            return await _context.Transactions.Where(x => x.CategoryId == idCategory && x.UserId == userId)
                                                        .AsNoTracking()
                                                        .OrderByDescending(x => x.Date)
                                                        .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(string userId,DateTime start, DateTime end)
        {
             return await _context.Transactions.Where(x => x.Date >= start && x.Date <= end && x.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(x => x.Date)
                .ToListAsync();    
        }

        public async Task<IEnumerable<Transaction>> GetByTypeTransactionAsync(string userId, TypeTransaction typeTransaction)
        {
            return await _context.Transactions.Where(x => x.Type == typeTransaction && x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdForUserAsync(int id, string userId)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }

        public async Task<IEnumerable<Transaction>> GetAllByUserAsync(string userId)
        {
            return await _context.Transactions
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
