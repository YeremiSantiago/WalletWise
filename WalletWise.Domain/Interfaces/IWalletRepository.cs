using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Entities;

namespace WalletWise.Domain.Interfaces
{
    public interface IWalletRepository : IGenericRepository<Wallet>
    {
        Task<Wallet?> GetByIdForUserAsync(int id, string userId);
        Task<IEnumerable<Wallet>> GetAllByUserAsync(string userId);
    }
}
