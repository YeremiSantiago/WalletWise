using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;
using WalletWise.Application.Dtos.Budget;
using WalletWise.Application.Interfaces;
using WalletWise.Application.Mappings.EntityToDto;
using WalletWise.Application.Services;
using WalletWise.Domain.Common;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Unit.Tests.Services
{
    public class BudgetServiceTests
    {
        private readonly Mock<IBudgetRepository> _budgetRepository;
        private readonly Mock<ICurrentUserService> _currentUserService;
        private readonly Mock<IWalletRepository> _walletRepository;
        private readonly Mock<ICategoryRepository> _categoryRepository;
        private readonly Mock<IClock> _clock;
        private readonly BudgetService _budgetService;

        private const string TestUserId = "1";

        public BudgetServiceTests()
        {
            _budgetRepository = new Mock<IBudgetRepository>();
            _currentUserService = new Mock<ICurrentUserService>();
            _walletRepository = new Mock<IWalletRepository>();
            _categoryRepository = new Mock<ICategoryRepository>();
            _clock = new Mock<IClock>();

            _currentUserService.Setup(x => x.UserId).Returns(TestUserId);

            var config = new MapperConfiguration(
              cfg => cfg.AddProfile<BudgetMappingProfile>(), NullLoggerFactory.Instance
          );

            var mapper = config.CreateMapper();

            _budgetService = new BudgetService(
                _budgetRepository.Object,
                _currentUserService.Object,
                _walletRepository.Object,
                _categoryRepository.Object,
                _clock.Object,
                mapper
            );
        }

        #region Helpers

        private Category CreateExpenseCategory(int id = 1, string name = "Comida")
        {
            return new Category { Id = id, Name = name, UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense };
        }

        private Category CreateIncomeCategory(int id = 2, string name = "Salario")
        {
            return new Category { Id = id, Name = name, UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Income };
        }

        private Wallet CreateWallet(int id = 1, string name = "Personal")
        {
            return new Wallet { Id = id, Name = name, UserId = TestUserId };
        }

        private Budget CreateBudget(int id = 1, int walletId = 1, int categoryId = 1, decimal amount = 20000, BudgetMode mode = BudgetMode.Flexible)
        {
            return new Budget
            {
                Id = id,
                WalletId = walletId,
                CategoryId = categoryId,
                Amount = amount,
                StartDate = new DateTime(2026, 10, 1),
                EndDate = new DateTime(2026, 10, 31),
                Mode = mode,
                UserId = TestUserId
            };
        }

        private CreateBudgetRequestDto CreateBudgetRequest(decimal amount = 20000, BudgetMode mode = BudgetMode.Flexible)
        {
            return new CreateBudgetRequestDto
            {
                WalletId = 1,
                CategoryId = 1,
                Amount = amount,
                StartDate = new DateTime(2026, 10, 1),
                EndDate = new DateTime(2026, 10, 31),
                Mode = mode
            };
        }

        private void SetupDefaultMocks(decimal spentAmount = 0)
        {
            _clock.Setup(c => c.UtcNow()).Returns(new DateTime(2026, 10, 5));
            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(true);
            _categoryRepository.Setup(r => r.GetCategoryActiveByIdAsync(1, TestUserId)).ReturnsAsync(CreateExpenseCategory());
            _budgetRepository.Setup(r => r.HasOverlappingBudgetAsync(TestUserId, 1, 1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).ReturnsAsync(false);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), TestUserId, null)).ReturnsAsync(spentAmount);
            _budgetRepository.Setup(r => r.AddAsync(It.IsAny<Budget>())).ReturnsAsync((Budget b) => b);
        }

        #endregion

        // CreateBudgetAsync

        #region CreateBudgetAsync

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-500)]
        public async Task CreateBudgetAsync_WhenAmountIsInvalid_ReturnsFailureWithError(decimal amount)
        {
            // Arrange
            var request = CreateBudgetRequest(amount: amount);

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.Equal(DomainErrors.Budget.InvalidAmount.Code, result.Error?.Code);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenEndDateIsBeforeStartDate_ReturnsFailureWithError()
        {
            // Arrange
            var request = CreateBudgetRequest();
            request.StartDate = new DateTime(2026, 10, 31);
            request.EndDate = new DateTime(2026, 10, 1);

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.InvalidDateRange.Code, result.Error?.Code);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenPeriodIsInThePast_ReturnsFailureWithError()
        {
            // Arrange
            _clock.Setup(c => c.UtcNow()).Returns(new DateTime(2026, 11, 15));

            var request = CreateBudgetRequest();
            request.StartDate = new DateTime(2026, 10, 1);
            request.EndDate = new DateTime(2026, 10, 31);

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.PastPeriod.Code, result.Error?.Code);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenWalletDoesNotExist_ReturnsFailureWithError()
        {
            // Arrange
            _clock.Setup(c => c.UtcNow()).Returns(new DateTime(2026, 10, 5));
            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(false);

            var request = CreateBudgetRequest();

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Wallet.NotFound.Code, result.Error?.Code);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenCategoryDoesNotExist_ReturnsFailureWithError()
        {
            // Arrange
            _clock.Setup(c => c.UtcNow()).Returns(new DateTime(2026, 10, 5));
            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(true);
            _categoryRepository.Setup(r => r.GetCategoryActiveByIdAsync(1, TestUserId)).ReturnsAsync((Category?)null);

            var request = CreateBudgetRequest();

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Category.NotFound.Code, result.Error?.Code);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenCategoryIsNotExpense_ReturnsFailureWithError()
        {
            // Arrange
            _clock.Setup(c => c.UtcNow()).Returns(new DateTime(2026, 10, 5));
            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(true);
            _categoryRepository.Setup(r => r.GetCategoryActiveByIdAsync(1, TestUserId)).ReturnsAsync(CreateIncomeCategory(id: 1));

            var request = CreateBudgetRequest();

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.CategoryNotExpense.Code, result.Error?.Code);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenOverlappingBudgetExists_ReturnsFailureWithError()
        {
            // Arrange
            _clock.Setup(c => c.UtcNow()).Returns(new DateTime(2026, 10, 5));
            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(true);
            _categoryRepository.Setup(r => r.GetCategoryActiveByIdAsync(1, TestUserId)).ReturnsAsync(CreateExpenseCategory());
            _budgetRepository.Setup(r => r.HasOverlappingBudgetAsync(TestUserId, 1, 1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).ReturnsAsync(true);

            var request = CreateBudgetRequest();

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.OverlappingPeriod.Code, result.Error?.Code);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenStrictModeAndExistingExpensesExceedAmount_ReturnsFailureWithError()
        {
            // Arrange
            SetupDefaultMocks(spentAmount: 25000);

            var request = CreateBudgetRequest(amount: 20000, mode: BudgetMode.Strict);

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.ExceededOnCreation.Code, result.Error?.Code);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenStrictModeAndExistingExpensesEqualAmount_ReturnsSuccess()
        {
            // Arrange
            SetupDefaultMocks(spentAmount: 20000);

            var request = CreateBudgetRequest(amount: 20000, mode: BudgetMode.Strict);

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenStrictModeAndExistingExpensesBelowAmount_ReturnsSuccess()
        {
            // Arrange
            SetupDefaultMocks(spentAmount: 15000);

            var request = CreateBudgetRequest(amount: 20000, mode: BudgetMode.Strict);

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(5000, result.Value?.RemainingAmount);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenFlexibleModeAndExistingExpensesExceedAmount_ReturnsSuccessExceeded()
        {
            // Arrange
            SetupDefaultMocks(spentAmount: 25000);

            var request = CreateBudgetRequest(amount: 20000, mode: BudgetMode.Flexible);

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value?.IsExceeded);
            Assert.Equal(-5000, result.Value?.RemainingAmount);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenValidRequest_ReturnsSuccessWithCalculatedValues()
        {
            // Arrange
            SetupDefaultMocks(spentAmount: 8000);

            var request = CreateBudgetRequest(amount: 20000, mode: BudgetMode.Flexible);

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(20000, result.Value?.Amount);
            Assert.Equal(8000, result.Value?.SpentAmount);
            Assert.Equal(12000, result.Value?.RemainingAmount);
            Assert.Equal(40m, result.Value?.UsagePercentage);
            Assert.False(result.Value?.IsExceeded);
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenMidMonthCreation_ReturnsSuccessWithEndDateStillValid()
        {
            // Arrange - hoy es Oct 15, pero el presupuesto es Oct 01-31 (EndDate >= hoy)
            _clock.Setup(c => c.UtcNow()).Returns(new DateTime(2026, 10, 15));
            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(true);
            _categoryRepository.Setup(r => r.GetCategoryActiveByIdAsync(1, TestUserId)).ReturnsAsync(CreateExpenseCategory());
            _budgetRepository.Setup(r => r.HasOverlappingBudgetAsync(TestUserId, 1, 1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).ReturnsAsync(false);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, It.IsAny<DateTime>(), It.IsAny<DateTime>(), TestUserId, null)).ReturnsAsync(0);
            _budgetRepository.Setup(r => r.AddAsync(It.IsAny<Budget>())).ReturnsAsync((Budget b) => b);

            var request = CreateBudgetRequest();

            // Act
            var result = await _budgetService.CreateBudgetAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
        }

        #endregion

        // GetBudgetByIdAsync

        #region GetBudgetByIdAsync

        [Fact]
        public async Task GetBudgetByIdAsync_WhenBudgetExists_ReturnsSuccessWithCalculatedValues()
        {
            // Arrange
            var budget = CreateBudget();

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(5000);

            // Act
            var result = await _budgetService.GetBudgetByIdAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(1, result.Value?.Id);
            Assert.Equal(5000, result.Value?.SpentAmount);
            Assert.Equal(15000, result.Value?.RemainingAmount);
            Assert.Equal(25m, result.Value?.UsagePercentage);
            Assert.False(result.Value?.IsExceeded);
        }

        [Fact]
        public async Task GetBudgetByIdAsync_WhenBudgetDoesNotExist_ReturnsFailure()
        {
            // Arrange
            _budgetRepository.Setup(r => r.GetByIdForUserAsync(99, TestUserId)).ReturnsAsync((Budget?)null);

            // Act
            var result = await _budgetService.GetBudgetByIdAsync(99);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.NotFound.Code, result.Error?.Code);
        }

        [Fact]
        public async Task GetBudgetByIdAsync_WhenBudgetIsExceeded_ReturnsSuccessWithExceededTrue()
        {
            // Arrange
            var budget = CreateBudget(amount: 10000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(15000);

            // Act
            var result = await _budgetService.GetBudgetByIdAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value?.IsExceeded);
            Assert.Equal(-5000, result.Value?.RemainingAmount);
            Assert.Equal(150m, result.Value?.UsagePercentage);
        }

        #endregion

        // GetAllBudgetsAsync

        #region GetAllBudgetsAsync

        [Fact]
        public async Task GetAllBudgetsAsync_WhenBudgetsExist_ReturnsSuccessWithAllBudgets()
        {
            // Arrange
            var budgets = new List<Budget>
            {
                CreateBudget(id: 1, amount: 20000),
                CreateBudget(id: 2, walletId: 2, categoryId: 2, amount: 10000)
            };

            _budgetRepository.Setup(r => r.GetAllByUserIdAsync(TestUserId)).ReturnsAsync(budgets);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), TestUserId, null)).ReturnsAsync(0);

            // Act
            var result = await _budgetService.GetAllBudgetsAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value?.Count());
        }

        [Fact]
        public async Task GetAllBudgetsAsync_WhenNoBudgets_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _budgetRepository.Setup(r => r.GetAllByUserIdAsync(TestUserId)).ReturnsAsync(new List<Budget>());

            // Act
            var result = await _budgetService.GetAllBudgetsAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value!);
        }

        #endregion


        // GetBudgetsByWalletAsync
     

        #region GetBudgetsByWalletAsync

        [Fact]
        public async Task GetBudgetsByWalletAsync_WhenWalletDoesNotExist_ReturnsFailure()
        {
            // Arrange
            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(false);

            // Act
            var result = await _budgetService.GetBudgetsByWalletAsync(99);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Wallet.NotFound.Code, result.Error?.Code);
        }

        [Fact]
        public async Task GetBudgetsByWalletAsync_WhenWalletHasBudgets_ReturnsSuccessWithBudgets()
        {
            // Arrange
            var budgets = new List<Budget>
            {
                CreateBudget(id: 1),
                CreateBudget(id: 2, categoryId: 2)
            };

            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(true);
            _budgetRepository.Setup(r => r.GetByWalletAsync(1, TestUserId)).ReturnsAsync(budgets);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), TestUserId, null)).ReturnsAsync(0);

            // Act
            var result = await _budgetService.GetBudgetsByWalletAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value?.Count());
        }

        [Fact]
        public async Task GetBudgetsByWalletAsync_WhenWalletHasNoBudgets_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            _walletRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Wallet, bool>>>())).ReturnsAsync(true);
            _budgetRepository.Setup(r => r.GetByWalletAsync(1, TestUserId)).ReturnsAsync(new List<Budget>());

            // Act
            var result = await _budgetService.GetBudgetsByWalletAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value!);
        }

        #endregion

        #region UpdateBudgetAsync

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-1000)]
        public async Task UpdateBudgetAsync_WhenAmountIsInvalid_ReturnsFailure(decimal amount)
        {
            // Arrange
            var dto = new UpdateBudgetRequestDto { Amount = amount, Mode = BudgetMode.Flexible };

            // Act
            var result = await _budgetService.UpdateBudgetAsync(1, dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.InvalidAmount.Code, result.Error?.Code);
        }

        [Fact]
        public async Task UpdateBudgetAsync_WhenBudgetDoesNotExist_ReturnsFailure()
        {
            // Arrange
            _budgetRepository.Setup(r => r.GetByIdForUserAsync(99, TestUserId)).ReturnsAsync((Budget?)null);
            var dto = new UpdateBudgetRequestDto { Amount = 20000, Mode = BudgetMode.Flexible };

            // Act
            var result = await _budgetService.UpdateBudgetAsync(99, dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.NotFound.Code, result.Error?.Code);
        }

        [Fact]
        public async Task UpdateBudgetAsync_WhenStrictAndNewAmountBelowSpent_ReturnsFailure()
        {
            // Arrange
            var budget = CreateBudget(mode: BudgetMode.Strict, amount: 20000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(15000);

            var dto = new UpdateBudgetRequestDto { Amount = 10000, Mode = BudgetMode.Strict };

            // Act
            var result = await _budgetService.UpdateBudgetAsync(1, dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.AmountBelowSpent.Code, result.Error?.Code);
        }

        [Fact]
        public async Task UpdateBudgetAsync_WhenChangingFlexibleToStrictAndSpentExceedsAmount_ReturnsFailure()
        {
            // Arrange
            var budget = CreateBudget(mode: BudgetMode.Flexible, amount: 20000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(25000);

            var dto = new UpdateBudgetRequestDto { Amount = 30000, Mode = BudgetMode.Strict };

            // Act
            var result = await _budgetService.UpdateBudgetAsync(1, dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.ModeChangeExceeded.Code, result.Error?.Code);
        }

        [Fact]
        public async Task UpdateBudgetAsync_WhenValidAmountUpdate_ReturnsSuccessWithUpdatedValues()
        {
            // Arrange
            var budget = CreateBudget(mode: BudgetMode.Flexible, amount: 20000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(8000);
            _budgetRepository.Setup(r => r.UpdateAsync(It.IsAny<Budget>()));

            var dto = new UpdateBudgetRequestDto { Amount = 25000, Mode = BudgetMode.Flexible };

            // Act
            var result = await _budgetService.UpdateBudgetAsync(1, dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(25000, result.Value?.Amount);
            Assert.Equal(8000, result.Value?.SpentAmount);
            Assert.Equal(17000, result.Value?.RemainingAmount);
            Assert.False(result.Value?.IsExceeded);
        }

        [Fact]
        public async Task UpdateBudgetAsync_WhenChangingFlexibleToStrictWithValidSpent_ReturnsSuccess()
        {
            // Arrange
            var budget = CreateBudget(mode: BudgetMode.Flexible, amount: 20000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(10000);
            _budgetRepository.Setup(r => r.UpdateAsync(It.IsAny<Budget>()));

            var dto = new UpdateBudgetRequestDto { Amount = 20000, Mode = BudgetMode.Strict };

            // Act
            var result = await _budgetService.UpdateBudgetAsync(1, dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BudgetMode.Strict, result.Value?.Mode);
        }

        [Fact]
        public async Task UpdateBudgetAsync_WhenChangingStrictToFlexible_ReturnsSuccess()
        {
            // Arrange
            var budget = CreateBudget(mode: BudgetMode.Strict, amount: 20000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(20000);
            _budgetRepository.Setup(r => r.UpdateAsync(It.IsAny<Budget>()));

            var dto = new UpdateBudgetRequestDto { Amount = 20000, Mode = BudgetMode.Flexible };

            // Act
            var result = await _budgetService.UpdateBudgetAsync(1, dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(BudgetMode.Flexible, result.Value?.Mode);
        }

        #endregion

        #region DeleteBudgetAsync

        [Fact]
        public async Task DeleteBudgetAsync_WhenBudgetExists_ReturnsSuccess()
        {
            // Arrange
            var budget = CreateBudget();

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.RemoveAsync(1));

            // Act
            var result = await _budgetService.DeleteBudgetAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            _budgetRepository.Verify(r => r.RemoveAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteBudgetAsync_WhenBudgetDoesNotExist_ReturnsFailure()
        {
            // Arrange
            _budgetRepository.Setup(r => r.GetByIdForUserAsync(99, TestUserId)).ReturnsAsync((Budget?)null);

            // Act
            var result = await _budgetService.DeleteBudgetAsync(99);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(DomainErrors.Budget.NotFound.Code, result.Error?.Code);
            _budgetRepository.Verify(r => r.RemoveAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion


        #region Calculated Values

        [Fact]
        public async Task GetBudgetByIdAsync_WhenNoTransactions_ReturnsZeroSpentAndFullRemaining()
        {
            // Arrange
            var budget = CreateBudget(amount: 20000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(0);

            // Act
            var result = await _budgetService.GetBudgetByIdAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Value?.SpentAmount);
            Assert.Equal(20000, result.Value?.RemainingAmount);
            Assert.Equal(0m, result.Value?.UsagePercentage);
            Assert.False(result.Value?.IsExceeded);
        }

        [Fact]
        public async Task GetBudgetByIdAsync_WhenHalfSpent_ReturnsCorrectPercentage()
        {
            // Arrange
            var budget = CreateBudget(amount: 20000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(10000);

            // Act
            var result = await _budgetService.GetBudgetByIdAsync(1);

            // Assert
            Assert.Equal(50m, result.Value?.UsagePercentage);
        }

        [Fact]
        public async Task GetBudgetByIdAsync_WhenFullySpent_Returns100Percent()
        {
            // Arrange
            var budget = CreateBudget(amount: 20000);

            _budgetRepository.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(budget);
            _budgetRepository.Setup(r => r.GetSpentAmountAsync(1, 1, budget.StartDate, budget.EndDate, TestUserId, null)).ReturnsAsync(20000);

            // Act
            var result = await _budgetService.GetBudgetByIdAsync(1);

            // Assert
            Assert.Equal(100m, result.Value?.UsagePercentage);
            Assert.Equal(0, result.Value?.RemainingAmount);
            Assert.False(result.Value?.IsExceeded);
        }

        #endregion
    }
}
