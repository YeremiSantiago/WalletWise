using Microsoft.Extensions.Logging;
using WalletWise.Application.Constants;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Services
{
    public class CategoryService : GenericService<Category>, ICategoryService 
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;
        
        public CategoryService(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, ILogger<Category> logger) : base(categoryRepository, logger) 
        {
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
        }

        public override async Task<Result<Category>> AddAsync(Category category)
        {
            var exist = await _categoryRepository.ExistsAsync(x => x.Name == category.Name); 
            
            if(exist == true)
            {
                return Result<Category>.Failure("Ya existe una Categoria con ese mismo nombre");
            }

            category.UserId = DefaultUser.Id;


            return Result<Category>.Success(await _categoryRepository.AddAsync(category));

        }

        public override async Task<Result<bool>> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if(category == null)
            {
                return Result<bool>.Failure($"La categoria con id {id} no pudo ser encontrada");
            }

            bool exists = await _transactionRepository.ExistsTransactionByCategoryAsync(category.Id);

            if (exists == true)
            {
                return Result<bool>.Failure("Esta categoria tiene transacciones asociadas");
            }

            category.IsDeleted = true;

            await _categoryRepository.UpdateAsync(category);

            return Result<bool>.Success(true);

        }




    }
}
