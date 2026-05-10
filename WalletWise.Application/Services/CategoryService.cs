using Microsoft.Extensions.Logging;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using AutoMapper;

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
            try
            {
                var userId = _currentUserService.UserId!;
                var categories = await _categoryRepository.GetAllCategoriesActiveAsync(userId);

                return Result<IEnumerable<CategoryResponseDto>>.Success(_mapper.Map<IEnumerable<CategoryResponseDto>>(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A ocurrido un fallo inesperado al obtener todas las categories");
                return Result<IEnumerable<CategoryResponseDto>>.Failure("No se ha podido obtener todas las categorias");
            }
        }

        public async Task<Result<CategoryResponseDto?>> GetCategoryByIdAsync(int id)
        {
            try
            {
                var userId = _currentUserService.UserId!;
                var result = await _categoryRepository.GetCategoryActiveByIdAsync(id, userId);

                if (result is null)
                {
                    return Result<CategoryResponseDto?>.Failure($"La category con el id {id} no existe");
                }

                return Result<CategoryResponseDto?>.Success(_mapper.Map<CategoryResponseDto>(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo inesperado al obtener la categoria con el id {Id} ", id);
                return Result<CategoryResponseDto?>.Failure($"No se ha podido obtener la categoria con el id {id}");
            }
        }

        public async Task<Result<CategoryResponseDto>> CreateCategoryAsync(CreateCategoryRequestDto categoryDto)
        {
            try
            {
                var userId = _currentUserService.UserId!;
                var category = _mapper.Map<Category>(categoryDto);

                category.UserId = userId;
                category.CreatedAt = _clock.UtcNow();

                var exist = await _categoryRepository.ExistsByNameAsync(category.Name, userId);

                if (exist)
                {
                    return Result<CategoryResponseDto>.Failure("Ya existe una Categoria con ese mismo nombre");
                }

                var response = await _categoryRepository.AddAsync(category);

                return Result<CategoryResponseDto>.Success(_mapper.Map<CategoryResponseDto>(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo al crear la categoria");
                return Result<CategoryResponseDto>.Failure("No se ha podido crear la categoria");
            }
        }

        public async Task<Result<CategoryResponseDto>> UpdateCategoryAsync(int id, UpdateCategoryRequestDto categoryDto)
        {
            try
            {
                var userId = _currentUserService.UserId!;
                var exist = await _categoryRepository.GetCategoryActiveByIdAsync(id, userId);

                if (exist == null)
                {
                    return Result<CategoryResponseDto>.Failure($"La Categoria con el id {id} no existe");
                }

                _mapper.Map(categoryDto, exist);

                exist.UpdatedAt = _clock.UtcNow();

                await _categoryRepository.UpdateAsync(exist);

                return Result<CategoryResponseDto>.Success(_mapper.Map<CategoryResponseDto>(exist));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un error inesperado al actualizar la categoria {Id}", id);
                return Result<CategoryResponseDto>.Failure("No se ha podido actualizar la categoria");
            }
        }

        public async Task<Result<bool>> DeleteCategoryAsync(int id)
        {
            try
            {
                var userId = _currentUserService.UserId!;
                var category = await _categoryRepository.GetCategoryActiveByIdAsync(id, userId);

                if (category == null)
                {
                    return Result<bool>.Failure($"La categoria con id {id} no pudo ser encontrada");
                }

                var exists = await _transactionRepository.ExistsTransactionByCategoryAsync(category.Id, userId);

                if (exists)
                {
                    return Result<bool>.Failure("Esta categoria tiene transacciones asociadas");
                }

                category.UpdatedAt = _clock.UtcNow();
                category.IsDeleted = true;

                await _categoryRepository.UpdateAsync(category);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ha ocurrido un fallo al borrar la categoria id {Id}", id);
                return Result<bool>.Failure("No se ha podido eliminar la categoria");
            }
        }
    }
}
