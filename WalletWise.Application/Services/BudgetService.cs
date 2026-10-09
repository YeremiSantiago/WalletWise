using AutoMapper;
using WalletWise.Application.Dtos.Budget;
using WalletWise.Application.Interfaces;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Application.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWalletRepository _walletRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public BudgetService(IBudgetRepository budgetRepository, ICurrentUserService currentUserService, IWalletRepository walletRepository, ICategoryRepository categoryRepository, IClock clock, IMapper mapper)
        {
            _budgetRepository = budgetRepository;
            _currentUserService = currentUserService;
            _walletRepository = walletRepository;
            _categoryRepository = categoryRepository;
            _clock = clock;
            _mapper = mapper;
        }

        public async Task<Result<BudgetResponseDto>> CreateBudgetAsync(CreateBudgetRequestDto dto)
        {
            if (dto.Amount <= 0)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.InvalidAmount);

            if (dto.EndDate < dto.StartDate)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.InvalidDateRange);

            if (dto.EndDate.Date < _clock.UtcNow().Date)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.PastPeriod);

            var userId = _currentUserService.UserId!;

            bool walletExist = await _walletRepository.ExistsAsync(x => x.Id == dto.WalletId && x.UserId == userId);

            if (walletExist is false)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Wallet.NotFound);

            var category = await _categoryRepository.GetCategoryActiveByIdAsync(dto.CategoryId, userId);

            if (category is null)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Category.NotFound);

            if (category.Type != TypeTransaction.Expense)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.CategoryNotExpense);

            var overlapping = await _budgetRepository.HasOverlappingBudgetAsync(userId, dto.WalletId, category.Id, dto.StartDate, dto.EndDate);

            if (overlapping)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.OverlappingPeriod);

            var spentAmount = await _budgetRepository.GetSpentAmountAsync(dto.WalletId, category.Id, dto.StartDate, dto.EndDate, userId);

            if (dto.Mode == BudgetMode.Strict && spentAmount > dto.Amount)
            {
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.ExceededOnCreation);
            }

            var budget = _mapper.Map<Budget>(dto);
            budget.UserId = userId;

            await _budgetRepository.AddAsync(budget);

            var response = BuildBudgetResponse(budget, spentAmount);

            return Result<BudgetResponseDto>.Success(response);
        }

        public async Task<Result<BudgetResponseDto>> GetBudgetByIdAsync(int id)
        {
            var userId = _currentUserService.UserId!;

            var budget = await _budgetRepository.GetByIdForUserAsync(id, userId);

            if (budget is null)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.NotFound);

            return Result<BudgetResponseDto>.Success(await BuildBudgetResponseAsync(budget));
        }

        public async Task<Result<IEnumerable<BudgetResponseDto>>> GetAllBudgetsAsync()
        {
            var userId = _currentUserService.UserId!;

            var budgets = await _budgetRepository.GetAllByUserIdAsync(userId);

            var result = new List<BudgetResponseDto>();
            foreach (var budget in budgets)
            {
                result.Add(await BuildBudgetResponseAsync(budget));
            }

            return Result<IEnumerable<BudgetResponseDto>>.Success(result);
        }

        public async Task<Result<IEnumerable<BudgetResponseDto>>> GetBudgetsByWalletAsync(int walletId)
        {
            var userId = _currentUserService.UserId!;

            var walletExist = await _walletRepository.ExistsAsync(x => x.Id == walletId && x.UserId == userId);

            if (walletExist is false)
                return Result<IEnumerable<BudgetResponseDto>>.Failure(DomainErrors.Wallet.NotFound);

            var budgets = await _budgetRepository.GetByWalletAsync(walletId, userId);

            var result = new List<BudgetResponseDto>();

            foreach (var budget in budgets)
            {
                result.Add(await BuildBudgetResponseAsync(budget));
            }

            return Result<IEnumerable<BudgetResponseDto>>.Success(result);
        }

        public async Task<Result<BudgetResponseDto>> UpdateBudgetAsync(int id, UpdateBudgetRequestDto dto)
        {
            var userId = _currentUserService.UserId!;

            if (dto.Amount <= 0)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.InvalidAmount);

            var budget = await _budgetRepository.GetByIdForUserAsync(id, userId);

            if (budget is null)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.NotFound);

            var spentAmount = await _budgetRepository.GetSpentAmountAsync(
                budget.WalletId, budget.CategoryId,
                budget.StartDate, budget.EndDate, userId);

            // Si el nuevo modo es Strict, el nuevo monto no puede ser menor al gasto acumulado
            if (dto.Mode == BudgetMode.Strict && spentAmount > dto.Amount)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.AmountBelowSpent);

            // Si cambia de Flexible a Strict, validar que el gasto actual no supere el monto
            if (budget.Mode == BudgetMode.Flexible && dto.Mode == BudgetMode.Strict && spentAmount > budget.Amount)
                return Result<BudgetResponseDto>.Failure(DomainErrors.Budget.ModeChangeExceeded);

            budget.Amount = dto.Amount;
            budget.Mode = dto.Mode;

            await _budgetRepository.UpdateAsync(budget);

            var response = BuildBudgetResponse(budget, spentAmount);

            return Result<BudgetResponseDto>.Success(response);
        }
      
        public async Task<Result<bool>> DeleteBudgetAsync(int id)
        {
            var userId = _currentUserService.UserId!;

            var budget = await _budgetRepository.GetByIdForUserAsync(id, userId);

            if (budget is null)
                return Result<bool>.Failure(DomainErrors.Budget.NotFound);

            await _budgetRepository.RemoveAsync(id);

            return Result<bool>.Success(true);
        }

        /// <summary>
        /// Construye el BudgetResponseDto con valores calculados a partir del gasto acumulado.
        /// Versión async que consulta el gasto desde el repositorio.
        /// </summary>
        private async Task<BudgetResponseDto> BuildBudgetResponseAsync(Budget budget)
        {
            var spentAmount = await _budgetRepository.GetSpentAmountAsync(
                budget.WalletId, budget.CategoryId,
                budget.StartDate, budget.EndDate, budget.UserId);

            return BuildBudgetResponse(budget, spentAmount);
        }

        /// <summary>
        /// Construye el BudgetResponseDto con valores calculados cuando ya se tiene el spentAmount.
        /// Evita una consulta adicional cuando el gasto ya fue calculado previamente.
        /// </summary>
        private BudgetResponseDto BuildBudgetResponse(Budget budget, decimal spentAmount)
        {
            var response = _mapper.Map<BudgetResponseDto>(budget);
            response.SpentAmount = spentAmount;
            response.RemainingAmount = budget.Amount - spentAmount;
            response.UsagePercentage = budget.Amount > 0 ? Math.Round((spentAmount / budget.Amount) * 100, 2) : 0;
            response.IsExceeded = spentAmount > budget.Amount;
            return response;
        }
    }
}
