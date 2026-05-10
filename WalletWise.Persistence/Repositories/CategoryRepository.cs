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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context ) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesActiveAsync(string userId)
        {
            return await _context.Categories.Where(x => x.IsDeleted == false && x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryActiveByIdAsync(int id, string userId)
        {
            return await _context.Categories.FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false && x.UserId == userId);
        }

        public async Task<bool> ExistsByNameAsync(string name, string userId)
        {
            return await _context.Categories.AnyAsync(x => x.Name == name && x.UserId == userId && x.IsDeleted == false);
        }

    }
}
