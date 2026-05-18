using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Dtos.Reports;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Domain.Common.Enums;
using WalletWise.Integration.Test.Infraestructure;

namespace WalletWise.Integration.Test.Controllers
{
    public class ReportsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        private static int ReportYear => DateTime.UtcNow.Year - 1;

        public ReportsControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        public async Task GetSummary_WhenTransactionsExisting_ReturnSummarySuccessfully()
        {
            // Arrange
            await CreateTransactionAsync(12000, TypeTransaction.Income);
            await CreateTransactionAsync(1000, TypeTransaction.Income);
            await CreateTransactionAsync(10000, TypeTransaction.Expense);
            await CreateTransactionAsync(500, TypeTransaction.Expense);

            // Act
            var response = await _client.GetAsync("api/reports/summary");

            // Act
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<ReportSummaryDto>();

            Assert.NotNull(result);
            Assert.Equal(result.Balance, 2500);
        }

        [Fact]
        public async Task GetMonthly_WhenTransactionsExisting_ReturnsMonthlySummaryList()
        {
            // Arrange
            var category = await CreateCategoryAsync("Categoria-Monthly");
            var wallet = await CreateWalletAsync("Wallet-Monthly");
            var year = ReportYear;

            await CreateTransactionAsync(1000, TypeTransaction.Income, new DateTime(year, 1, 15), category.Id, wallet.Id);
            await CreateTransactionAsync(400, TypeTransaction.Expense, new DateTime(year, 1, 16), category.Id, wallet.Id);
            await CreateTransactionAsync(2000, TypeTransaction.Income, new DateTime(year, 2, 20), category.Id, wallet.Id);

            // Act
            var response = await _client.GetAsync($"api/reports/monthly?year={year}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<MonthlySummaryDto>>();

            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.Contains(result, x => x.Month == 1);
            Assert.Contains(result, x => x.Month == 2);
        }

        [Fact]
        public async Task GetByCategory_WhenTransactionsExisting_ReturnsGroupedResults()
        {
            // Arrange
            var category = await CreateCategoryAsync("Comida");
            var wallet = await CreateWalletAsync("Principal");
            var year = ReportYear;

            await CreateTransactionAsync(500, TypeTransaction.Expense, new DateTime(year, 3, 10), category.Id, wallet.Id);
            await CreateTransactionAsync(250, TypeTransaction.Expense, new DateTime(year, 3, 11), category.Id, wallet.Id);

            // Act
            var response = await _client.GetAsync($"api/reports/by-category?start={year}-03-01&end={year}-03-31&type=Expense");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryReportItemDto>>();

            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.Contains(result, x => x.CategoryId == category.Id && x.TransactionsCount == 2 && x.TotalAmount == 750);
        }

        [Fact]
        public async Task GetByCategory_WhenNoTransactions_ReturnsMessage()
        {
            // Act
            var response = await _client.GetAsync("api/reports/by-category?start=1900-01-01&end=1900-01-31&type=Expense");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(payload.TryGetProperty("message", out _));
        }

        [Fact]
        public async Task GetByCategory_WhenStartAfterEnd_ReturnsUnprocessableEntity()
        {
            // Act
            var year = ReportYear;
            var response = await _client.GetAsync($"api/reports/by-category?start={year}-03-31&end={year}-03-01&type=Expense");

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetTopCategories_WhenTransactionsExisting_ReturnsTopCategories()
        {
            // Arrange
            var categoryA = await CreateCategoryAsync("A");
            var categoryB = await CreateCategoryAsync("B");
            var categoryC = await CreateCategoryAsync("C");
            var wallet = await CreateWalletAsync("TopWallet");
            var year = ReportYear;

            await CreateTransactionAsync(500, TypeTransaction.Expense, new DateTime(year, 4, 5), categoryA.Id, wallet.Id);
            await CreateTransactionAsync(300, TypeTransaction.Expense, new DateTime(year, 4, 6), categoryB.Id, wallet.Id);
            await CreateTransactionAsync(100, TypeTransaction.Expense, new DateTime(year, 4, 7), categoryC.Id, wallet.Id);

            // Act
            var response = await _client.GetAsync($"api/reports/top-categories?start={year}-04-01&end={year}-04-30&top=2");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<TopCategoryReportItemDto>>();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, x => x.CategoryId == categoryA.Id);
            Assert.Contains(result, x => x.CategoryId == categoryB.Id);
        }

        [Fact]
        public async Task GetTopCategories_WhenNoData_ReturnsMessage()
        {
            // Act
            var response = await _client.GetAsync("api/reports/top-categories?start=1900-01-01&end=1900-01-31&top=5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(payload.TryGetProperty("message", out _));
        }

        [Fact]
        public async Task GetTopCategories_WhenTopIsInvalid_ReturnsUnprocessableEntity()
        {
            // Act
            var year = ReportYear;
            var response = await _client.GetAsync($"api/reports/top-categories?start={year}-04-01&end={year}-04-30&top=0");

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task GetComparison_WhenValidPeriods_ReturnsComparison()
        {
            // Arrange
            var category = await CreateCategoryAsync("Comparacion");
            var wallet = await CreateWalletAsync("Comparacion");
            var year = ReportYear;

            await CreateTransactionAsync(1000, TypeTransaction.Income, new DateTime(year, 1, 10), category.Id, wallet.Id);
            await CreateTransactionAsync(200, TypeTransaction.Expense, new DateTime(year, 1, 11), category.Id, wallet.Id);

            await CreateTransactionAsync(2000, TypeTransaction.Income, new DateTime(year, 2, 10), category.Id, wallet.Id);
            await CreateTransactionAsync(500, TypeTransaction.Expense, new DateTime(year, 2, 11), category.Id, wallet.Id);

            // Act
            var response = await _client.GetAsync($"api/reports/comparison?startA={year}-01-01&endA={year}-01-31&startB={year}-02-01&endB={year}-02-28");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<ComparisonReportDto>();

            Assert.NotNull(result);
            Assert.Equal(800, result.Period1Balance);
            Assert.Equal(1500, result.Period2Balance);
            Assert.Equal(700, result.BalanceDifference);
        }

        [Fact]
        public async Task GetComparison_WhenPeriodsOverlap_ReturnsUnprocessableEntity()
        {
            // Act
            var year = ReportYear;
            var response = await _client.GetAsync($"api/reports/comparison?startA={year}-01-01&endA={year}-01-31&startB={year}-01-15&endB={year}-02-15");

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Fact]
        public async Task Export_WhenFiltersMatch_ReturnsTransactions()
        {
            // Arrange
            var category = await CreateCategoryAsync("Export");
            var wallet = await CreateWalletAsync("Export");
            var year = ReportYear;

            await CreateTransactionAsync(700, TypeTransaction.Expense, new DateTime(year, 5, 10), category.Id, wallet.Id, "Pago Luz");

            // Act
            var response = await _client.GetAsync($"api/reports/export?start={year}-05-01&end={year}-05-31&type=Expense&categoryId={category.Id}&search=Luz");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<TransactionResponseDto>>();

            Assert.NotNull(result);
            Assert.True(result.Any(x => x.Comment == "Pago Luz"));
        }

        [Fact]
        public async Task Export_WhenNoData_ReturnsMessage()
        {
            // Act
            var response = await _client.GetAsync("api/reports/export?search=NO_MATCH_2033");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.True(payload.TryGetProperty("message", out _));
        }

        private async Task<TransactionResponseDto> CreateTransactionAsync(
            decimal amount,
            TypeTransaction type,
            DateTime? date = null,
            int? categoryId = null,
            int? walletId = null,
            string? comment = null)
        {
            var resolvedCategoryId = categoryId ?? (await CreateCategoryAsync($"Categoria-{Guid.NewGuid()}")).Id;
            var resolvedWalletId = walletId ?? (await CreateWalletAsync($"Wallet-{Guid.NewGuid()}")).Id;

            var request = new CreateTransactionRequestDto
            {
                Amount = amount,
                Date = date ?? DateTime.UtcNow.Date.AddDays(-1),
                Type = type,
                Comment = comment ?? "Transaccion prueba",
                CategoryId = resolvedCategoryId,
                WalletId = resolvedWalletId
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
