using System.Net;
using System.Net.Http.Json;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Common.Pagination;
using WalletWise.Integration.Test.Infraestructure;

namespace WalletWise.Integration.Test.Controllers
{
    public class TransactionsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TransactionsControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetTransactionById_WhenTransactionExists_ReturnsTransaction()
        {
            var transaction = await CreateTransactionAsync(250, TypeTransaction.Income);

            var response = await _client.GetAsync($"api/transactions/{transaction.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<TransactionResponseDto>();

            Assert.NotNull(result);
            Assert.Equal(transaction.Id, result.Id);
        }

        [Fact]
        public async Task GetTransactionById_WhenTransactionDoesNotExist_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/transactions/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllTransactions_WhenTransactionsExist_ReturnsAllTransactions()
        {
            await CreateTransactionAsync(100, TypeTransaction.Expense);
            await CreateTransactionAsync(200, TypeTransaction.Income);
            await CreateTransactionAsync(300, TypeTransaction.Expense);

            var response = await _client.GetAsync("api/transactions");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<TransactionResponseDto>>();

            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Any());
            Assert.True(result.TotalRecords >= 3);
        }

        [Fact]
        public async Task CreateTransaction_WhenValid_ReturnsCreatedTransaction()
        {
            var category = await CreateCategoryAsync("Salud");
            var wallet = await CreateWalletAsync("Personal");

            var request = new CreateTransactionRequestDto
            {
                Amount = 500,
                Date = new DateTime(2026, 3, 1),
                Type = TypeTransaction.Expense,
                Comment = "Compra farmacia",
                CategoryId = category.Id,
                WalletId = wallet.Id
            };

            var response = await _client.PostAsJsonAsync("api/transactions", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<TransactionResponseDto>();

            Assert.NotNull(created);
            Assert.Equal(500, created.Amount);
            Assert.Equal(TypeTransaction.Expense, created.Type);
            Assert.Equal(category.Id, created.CategoryId);
            Assert.Equal(wallet.Id, created.WalletId);
        }

        [Fact]
        public async Task UpdateTransaction_WhenTransactionExists_ReturnsUpdatedTransaction()
        {
            var transaction = await CreateTransactionAsync(100, TypeTransaction.Income);

            var updateRequest = new UpdateTransactionRequestDto
            {
                Amount = 350,
                Date = new DateTime(2026, 3, 2),
                Type = TypeTransaction.Income,
                Comment = "Actualizado",
                CategoryId = transaction.CategoryId,
                WalletId = transaction.WalletId
            };

            var response = await _client.PutAsJsonAsync($"api/transactions/{transaction.Id}", updateRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<TransactionResponseDto>();

            Assert.NotNull(updated);
            Assert.Equal(350, updated.Amount);
            Assert.Equal("Actualizado", updated.Comment);
        }

        [Fact]
        public async Task DeleteTransaction_WhenTransactionExists_ReturnsNoContent()
        {
            var transaction = await CreateTransactionAsync(150, TypeTransaction.Expense);

            var response = await _client.DeleteAsync($"api/transactions/{transaction.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"api/transactions/{transaction.Id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteTransaction_WhenTransactionDoesNotExist_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("api/transactions/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<TransactionResponseDto> CreateTransactionAsync(decimal amount, TypeTransaction type)
        {
            var category = await CreateCategoryAsync($"Categoria-{Guid.NewGuid()}");
            var wallet = await CreateWalletAsync($"Wallet-{Guid.NewGuid()}");

            var request = new CreateTransactionRequestDto
            {
                Amount = amount,
                Date = new DateTime(2026, 3, 1),
                Type = type,
                Comment = "Transaccion prueba",
                CategoryId = category.Id,
                WalletId = wallet.Id
            };

            var response = await _client.PostAsJsonAsync("api/transactions", request);

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<TransactionResponseDto>();

            Assert.NotNull(created);

            return created;
        }

        private async Task<WalletResponseDto> CreateWalletAsync(string name)
        {
            var response = await _client.PostAsJsonAsync("api/wallets", new CreateWalletRequestDto { Name = name });

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<WalletResponseDto>();

            Assert.NotNull(created);

            return created;
        }

        private async Task<CategoryResponseDto> CreateCategoryAsync(string name)
        {
            var response = await _client.PostAsJsonAsync("api/categories", new CreateCategoryRequestDto { Name = name });

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<CategoryResponseDto>();

            Assert.NotNull(created);

            return created;
        }
    }
}
