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

        public async Task<bool> ExistsTransactionByCategoryAsync(int IdCategory)
        {
            return await _context.Transactions.
                AnyAsync(x => x.CategoryId == IdCategory);

        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsByCategoryAsync(int idCategory)
        {
            return await _context.Transactions.Where(x => x.CategoryId == idCategory)
                                                        .OrderByDescending(x => x.Date)
                                                        .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
             return await _context.Transactions.Where(x => x.Date >= start && x.Date <= end)
                .OrderByDescending(x => x.Date)
                .ToListAsync();    
        }

        public async Task<IEnumerable<Transaction>> GetByTypeTransactionAsync(TypeTransaction typeTransaction)
        {
            return await _context.Transactions.Where(x => x.Type == typeTransaction)
                .ToListAsync();
        }

       
    }
}
