
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;

namespace WalletWise.Application.Interfaces
{
    public interface ITransactionService : IGenericService<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Transaction>> GetByTypeTransactionAsync(TypeTransaction typeTransaction);
        Task<IEnumerable<Transaction>> GetByCategoryAsync(int idCategory);

    }
}
