using System.Net;
using System.Net.Http.Json;
using WalletWise.Application.Dtos.Budget;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Domain.Common.Enums;
using WalletWise.Integration.Test.Infraestructure;

namespace WalletWise.Integration.Test.Controllers
{
    [Collection("WebApi")]
    public class BudgetControllerIntegrationTests : IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public BudgetControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public Task InitializeAsync()
        {
            return _factory.ResetDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenDataIsValid_CreatesBudgetSuccessfully()
        {
            // Arrange
            var wallet = await CreateWalletAsync("Billetera Principal");
            var category = await CreateCategoryAsync("Comida", TypeTransaction.Expense);

            var data = new CreateBudgetRequestDto
            {
                WalletId = wallet.Id,
                CategoryId = category.Id,
                Amount = 5000,
                StartDate = new DateTime(2026, 11, 1),
                EndDate = new DateTime(2026, 11, 30),
                Mode = BudgetMode.Strict
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/budgets", data);

            // Assert
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<BudgetResponseDto>();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(created);
            Assert.Equal(data.Amount, created.Amount);
            Assert.Equal(0, created.SpentAmount); // Starts empty
        }

        [Fact]
        public async Task CreateBudgetAsync_WhenCategoryIsIncome_ReturnsBadRequest()
        {
            // Arrange
            var wallet = await CreateWalletAsync("Billetera Secundaria");
            var category = await CreateCategoryAsync("Salario", TypeTransaction.Income);

            var data = new CreateBudgetRequestDto
            {
                WalletId = wallet.Id,
                CategoryId = category.Id,
                Amount = 5000,
                StartDate = new DateTime(2026, 11, 1),
                EndDate = new DateTime(2026, 11, 30),
                Mode = BudgetMode.Flexible
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/budgets", data);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetBudgetById_WhenBudgetExists_ReturnsBudget()
        {
            // Arrange
            var wallet = await CreateWalletAsync("Mi Billetera");
            var category = await CreateCategoryAsync("Transporte", TypeTransaction.Expense);
            var budget = await CreateBudgetHelperAsync(8000, new DateTime(2026, 12, 1), new DateTime(2026, 12, 31), BudgetMode.Flexible, wallet.Id, category.Id);

            // Act
            var response = await _client.GetAsync($"api/budgets/{budget.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<BudgetResponseDto>();
            Assert.NotNull(result);
            Assert.Equal(budget.Id, result.Id);
            Assert.Equal(8000, result.Amount);
        }

        [Fact]
        public async Task GetBudgetById_WhenBudgetDoesNotExist_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("api/budgets/9999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllBudgetsAsync_ReturnsBudgetsList()
        {
            // Arrange
            var wallet = await CreateWalletAsync("Billetera A");
            var category1 = await CreateCategoryAsync("Servicios", TypeTransaction.Expense);
            var category2 = await CreateCategoryAsync("Entretenimiento", TypeTransaction.Expense);

            await CreateBudgetHelperAsync(2000, new DateTime(2027, 1, 1), new DateTime(2027, 1, 31), BudgetMode.Flexible, wallet.Id, category1.Id);
            await CreateBudgetHelperAsync(3000, new DateTime(2027, 2, 1), new DateTime(2027, 2, 28), BudgetMode.Flexible, wallet.Id, category2.Id);

            // Act
            var response = await _client.GetAsync("api/budgets");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<BudgetResponseDto>>();
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public async Task UpdateBudgetAsync_WhenDataIsValid_UpdatesBudget()
        {
            // Arrange
            var wallet = await CreateWalletAsync("Billetera Update");
            var category = await CreateCategoryAsync("Educacion", TypeTransaction.Expense);
            var budget = await CreateBudgetHelperAsync(10000, new DateTime(2027, 3, 1), new DateTime(2027, 3, 31), BudgetMode.Flexible, wallet.Id, category.Id);

            var updateData = new UpdateBudgetRequestDto
            {
                Amount = 15000,
                Mode = BudgetMode.Strict
            };

            // Act
            var response = await _client.PutAsJsonAsync($"api/budgets/{budget.Id}", updateData);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<BudgetResponseDto>();
            Assert.NotNull(result);
            Assert.Equal(15000, result.Amount);
            Assert.Equal(BudgetMode.Strict, result.Mode);
        }

        [Fact]
        public async Task DeleteBudgetAsync_WhenBudgetExists_DeletesBudget()
        {
            // Arrange
            var wallet = await CreateWalletAsync("Billetera Delete");
            var category = await CreateCategoryAsync("Salud", TypeTransaction.Expense);
            var budget = await CreateBudgetHelperAsync(20000, new DateTime(2027, 4, 1), new DateTime(2027, 4, 30), BudgetMode.Strict, wallet.Id, category.Id);

            // Act
            var deleteResponse = await _client.DeleteAsync($"api/budgets/{budget.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Verify deletion
            var getResponse = await _client.GetAsync($"api/budgets/{budget.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        //  Helpers 

        private async Task<BudgetResponseDto> CreateBudgetHelperAsync(decimal amount, DateTime startDate, DateTime endDate, BudgetMode mode, int walletId, int categoryId)
        {
            var data = new CreateBudgetRequestDto
            {
                WalletId = walletId,
                CategoryId = categoryId,
                Amount = amount,
                StartDate = startDate,
                EndDate = endDate,
                Mode = mode
            };

            var response = await _client.PostAsJsonAsync("api/budgets", data);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<BudgetResponseDto>())!;
        }

        private async Task<WalletResponseDto> CreateWalletAsync(string name)
        {
            var response = await _client.PostAsJsonAsync("api/wallets", new CreateWalletRequestDto { Name = name });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<WalletResponseDto>())!;
        }

        private async Task<CategoryResponseDto> CreateCategoryAsync(string name, TypeTransaction type = TypeTransaction.Expense)
        {
            var response = await _client.PostAsJsonAsync("api/categories", new CreateCategoryRequestDto { Name = name, Type = type });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CategoryResponseDto>())!;
        }
    }
}
