using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;


namespace WalletWise.Domain.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<bool> ExistsTransactionByCategoryAsync(int id);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Transaction>> GetByTypeTransactionAsync(TypeTransaction typeTransaction);
        Task<IEnumerable<Transaction>> GetAllTransactionsByCategoryAsync(int idCategory);

      
    }
}
