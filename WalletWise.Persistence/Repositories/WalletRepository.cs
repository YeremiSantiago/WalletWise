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
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Wallet>> GetAllByUserAsync(string userId)
        {
            return await _context.Wallets.AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToListAsync();          
        }

        public async Task<Wallet?> GetByIdForUserAsync(int id, string userId)
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }
    }
}
