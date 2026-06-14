using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Common.Pagination;
using WalletWise.Domain.Entities;


namespace WalletWise.Domain.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<Transaction?> GetByIdForUserAsync(int id, string userId);
        Task<IEnumerable<Transaction>> GetAllByUserAsync(string userId);
        Task<bool> ExistsTransactionByCategoryAsync(int id, string userId);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(string userId, DateTime start, DateTime end);
        Task<IEnumerable<Transaction>> GetByTypeTransactionAsync(string userId, TypeTransaction typeTransaction);
        Task<IEnumerable<Transaction>> GetAllTransactionsByCategoryAsync(string userId, int idCategory);
        Task<PagedResult<Transaction>> GetPagedTransactionsAsync(string userId, TransactionFilterParams filterParams);


    }
}
