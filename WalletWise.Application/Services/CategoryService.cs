using Microsoft.Extensions.Logging;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using AutoMapper;
using WalletWise.Application.Common;

namespace WalletWise.Application.Services
{
    public class CategoryService : GenericService<Category, CategoryResponseDto, CreateCategoryRequestDto, UpdateCategoryRequestDto>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IClock _clock;
        private readonly ICurrentUserService _currentUserService;

        public CategoryService(
            ICategoryRepository categoryRepository,
            ITransactionRepository transactionRepository,
            ILogger<Category> logger,
            IClock clock,
            IMapper mapper,
            ICurrentUserService currentUserService)
            : base(categoryRepository, logger, mapper)
        {
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
            _clock = clock;
            _currentUserService = currentUserService;
        }

        public async Task<Result<IEnumerable<CategoryResponseDto>>> GetAllCategoriesAsync()
        {
            var userId = _currentUserService.UserId!;
            var categories = await _categoryRepository.GetAllCategoriesActiveAsync(userId);

            return Result<IEnumerable<CategoryResponseDto>>.Success(_mapper.Map<IEnumerable<CategoryResponseDto>>(categories));
        }

        public async Task<Result<CategoryResponseDto?>> GetCategoryByIdAsync(int id)
        {
            var userId = _currentUserService.UserId!;
            var result = await _categoryRepository.GetCategoryActiveByIdAsync(id, userId);

            if (result is null)
            {
                throw new Exceptions.NotFoundException($"La category con el id {id} no existe");
            }

            return Result<CategoryResponseDto?>.Success(_mapper.Map<CategoryResponseDto>(result));
        }

        public async Task<Result<CategoryResponseDto>> CreateCategoryAsync(CreateCategoryRequestDto categoryDto)
        {
            var userId = _currentUserService.UserId!;
            var category = _mapper.Map<Category>(categoryDto);

            category.UserId = userId;
            category.CreatedAt = _clock.UtcNow();

            var exist = await _categoryRepository.ExistsByNameAsync(category.Name, userId);

            if (exist)
            {
                return Result<CategoryResponseDto>.Failure(BusinessErrorCodes.ERR_CATEGORY_NAME_EXISTS, "Ya existe una Categoria con ese mismo nombre");
            }

            try
            {
                var response = await _categoryRepository.AddAsync(category);
                return Result<CategoryResponseDto>.Success(_mapper.Map<CategoryResponseDto>(response));
            }
            catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
            {
                return Result<CategoryResponseDto>.Failure(BusinessErrorCodes.ERR_CATEGORY_NAME_EXISTS, "Ya existe una categoría con ese mismo nombre");
            }
        }

        public async Task<Result<CategoryResponseDto>> UpdateCategoryAsync(int id, UpdateCategoryRequestDto categoryDto)
        {
            var userId = _currentUserService.UserId!;
            var exist = await _categoryRepository.GetCategoryActiveByIdAsync(id, userId);

            if (exist == null)
            {
                throw new Exceptions.NotFoundException($"La Categoria con el id {id} no existe");
            }

            _mapper.Map(categoryDto, exist);

            exist.UpdatedAt = _clock.UtcNow();

            await _categoryRepository.UpdateAsync(exist);

            return Result<CategoryResponseDto>.Success(_mapper.Map<CategoryResponseDto>(exist));
        }

        public async Task<Result<bool>> DeleteCategoryAsync(int id)
        {
            var userId = _currentUserService.UserId!;
            var category = await _categoryRepository.GetCategoryActiveByIdAsync(id, userId);

            if (category == null)
            {
                throw new Exceptions.NotFoundException($"La categoria con id {id} no pudo ser encontrada");
            }

            var exists = await _transactionRepository.ExistsTransactionByCategoryAsync(category.Id, userId);

            if (exists)
            {
                return Result<bool>.Failure(BusinessErrorCodes.ERR_CATEGORY_HAS_TRANSACTIONS, "Esta categoria tiene transacciones asociadas");
            }

            category.UpdatedAt = _clock.UtcNow();
            category.IsDeleted = true;

            await _categoryRepository.UpdateAsync(category);

            return Result<bool>.Success(true);
        }
    }
}
