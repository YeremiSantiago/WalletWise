using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Entities;

namespace WalletWise.Domain.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllCategoriesActiveAsync(string userId);
        Task<Category?> GetCategoryActiveByIdAsync(int id, string userId);
        Task<bool> ExistsByNameAsync(string name, string usrId);
        
    }
}
