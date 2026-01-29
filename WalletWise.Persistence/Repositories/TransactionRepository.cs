using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var resultBool = await _context.Transactions.
                AnyAsync(x => x.CategoryId == IdCategory);


            return resultBool;

        }


    }
}
