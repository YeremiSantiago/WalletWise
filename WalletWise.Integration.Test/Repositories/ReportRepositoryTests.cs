using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Infrastructure.Context;
using WalletWise.Infrastructure.Repositories;
using WalletWise.Integration.Test.Infraestructure;
using Xunit;

namespace WalletWise.Integration.Test.Repositories
{
    [Collection("Database")]
    public class ReportRepositoryTests : IAsyncLifetime
    {
        private IReportRepository _reportRepository;
        private AppDbContext _context;
        private const string TestUserId = "1";
        private readonly DatabaseFixture _fixture;

        public ReportRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _context = _fixture.CreateDbContext();
            await _context.Database.EnsureCreatedAsync();


            _context.Transactions.RemoveRange(_context.Transactions);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Wallets.RemoveRange(_context.Wallets);
            await _context.SaveChangesAsync();
            _reportRepository = new ReportRepository(_context);
        }
        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task GetSummaryAsync_WhenTransactionsExist_ReturnsTotals()
        {
            await SeedSummaryDataAsync();

            var result = await _reportRepository.GetSummaryAsync(TestUserId);

            Assert.Equal(1500, result.TotalIncome);
            Assert.Equal(300, result.TotalExpense);
            Assert.Equal(1200, result.Balance);
        }

        [Fact]
        public async Task GetSummaryAsync_WhenNoTransactions_ReturnsZeros()
        {
            await ClearDataAsync();

            var result = await _reportRepository.GetSummaryAsync(TestUserId);

            Assert.Equal(0, result.TotalIncome);
            Assert.Equal(0, result.TotalExpense);
            Assert.Equal(0, result.Balance);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_WhenTransactionsExist_ReturnsMonthlyTotals()
        {
            await SeedMonthlyDataAsync();
            int year = 2026;

            var result = (await _reportRepository.GetMonthlySummaryAsync(TestUserId, year)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Month);
            Assert.Equal(2, result[1].Month);
            Assert.Equal(1000, result[0].TotalIncome);
            Assert.Equal(200, result[1].TotalExpense);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_WhenNoTransactions_ReturnsEmpty()
        {
            await ClearDataAsync();
            int year = 2026;

            var result = (await _reportRepository.GetMonthlySummaryAsync(TestUserId, year)).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByCategoryAsync_WhenTransactionsExist_ReturnsGroupedData()
        {
            await SeedCategoryDataAsync();
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 31);

            var result = (await _reportRepository.GetByCategoryAsync(TestUserId, start, end, TypeTransaction.Expense)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.CategoryName == "Comida" && x.TotalAmount == 300);
            Assert.Contains(result, x => x.CategoryName == "Servicios" && x.TransactionsCount == 1);
        }

        [Fact]
        public async Task GetComparisonAsync_WhenPeriodsExist_ReturnsComparison()
        {
            await SeedComparisonDataAsync();

            var result = await _reportRepository.GetComparisonAsync(
                TestUserId,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 31),
                new DateTime(2026, 2, 1),
                new DateTime(2026, 2, 28));

            Assert.Equal(1000, result.Period1Income);
            Assert.Equal(200, result.Period1Expense);
            Assert.Equal(500, result.Period2Income);
            Assert.Equal(700, result.Period2Expense);
            Assert.Equal(-500, result.IncomeDifference);
            Assert.Equal(500, result.ExpenseDifference);
            Assert.Equal(-1000, result.BalanceDifference);
        }

        [Fact]
        public async Task GetExportAsync_WhenNoFilters_ReturnsOrderedByDateDesc()
        {
            await SeedExportDataAsync();

            var result = (await _reportRepository.GetExportAsync(TestUserId, null, null, null, null, null)).ToList();

            Assert.Equal(3, result.Count);
            Assert.True(result[0].Date >= result[1].Date);
            Assert.True(result[1].Date >= result[2].Date);
        }

        private async Task SeedSummaryDataAsync()
        {
            await ClearDataAsync();

            var wallet = new Wallet { Name = "Principal", UserId = TestUserId };
            var category = new Category { Name = "General", UserId = TestUserId, Type = TypeTransaction.Expense };

            await _context.Wallets.AddAsync(wallet);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var transactions = new List<Transaction>
            {
                new Transaction { Amount = 1000, Date = new DateTime(2026, 1, 1), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id },
                new Transaction { Amount = 500, Date = new DateTime(2026, 1, 10), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id },
                new Transaction { Amount = 300, Date = new DateTime(2026, 1, 15), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id },
                new Transaction { Amount = 999, Date = new DateTime(2026, 1, 20), Type = TypeTransaction.Income, UserId = "2", CategoryId = category.Id, WalletId = wallet.Id }
            };

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedMonthlyDataAsync()
        {
            await ClearDataAsync();

            var wallet = new Wallet { Name = "Principal", UserId = TestUserId };
            var category = new Category { Name = "General", UserId = TestUserId, Type = TypeTransaction.Expense };

            await _context.Wallets.AddAsync(wallet);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var transactions = new List<Transaction>
            {
                new Transaction { Amount = 1000, Date = new DateTime(2026, 1, 5), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id },
                new Transaction { Amount = 100, Date = new DateTime(2026, 1, 15), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id },
                new Transaction { Amount = 200, Date = new DateTime(2026, 2, 8), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id }
            };

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCategoryDataAsync()
        {
            await ClearDataAsync();

            var categories = new List<Category>
            {
                new Category { Name = "Comida", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense },
                new Category { Name = "Servicios", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense },
                new Category { Name = "Oculta", UserId = TestUserId, IsDeleted = true, Type = TypeTransaction.Expense }
            };

            var wallet = new Wallet { Name = "Principal", UserId = TestUserId };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            var transactions = new List<Transaction>
            {
                new Transaction { Amount = 100, Date = new DateTime(2026, 1, 10), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[0].Id, WalletId = wallet.Id },
                new Transaction { Amount = 200, Date = new DateTime(2026, 1, 12), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[0].Id, WalletId = wallet.Id },
                new Transaction { Amount = 150, Date = new DateTime(2026, 1, 15), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[1].Id, WalletId = wallet.Id },
                new Transaction { Amount = 999, Date = new DateTime(2026, 1, 20), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[2].Id, WalletId = wallet.Id }
            };

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedComparisonDataAsync()
        {
            await ClearDataAsync();

            var wallet = new Wallet { Name = "Principal", UserId = TestUserId };
            var category = new Category { Name = "General", UserId = TestUserId, Type = TypeTransaction.Expense };

            await _context.Wallets.AddAsync(wallet);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var transactions = new List<Transaction>
            {
                new Transaction { Amount = 1000, Date = new DateTime(2026, 1, 10), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id },
                new Transaction { Amount = 200, Date = new DateTime(2026, 1, 15), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id },
                new Transaction { Amount = 500, Date = new DateTime(2026, 2, 10), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id },
                new Transaction { Amount = 700, Date = new DateTime(2026, 2, 12), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = category.Id, WalletId = wallet.Id }
            };

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedExportDataAsync()
        {
            await ClearDataAsync();

            var categories = new List<Category>
            {
                new Category { Name = "General", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense },
                new Category { Name = "Cafe", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense }
            };

            var wallet = new Wallet { Name = "Principal", UserId = TestUserId };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            var transactions = new List<Transaction>
            {
                new Transaction { Amount = 50, Date = new DateTime(2026, 3, 1), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = categories[0].Id, WalletId = wallet.Id, Comment = "Ingreso" },
                new Transaction { Amount = 25, Date = new DateTime(2026, 3, 10), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[1].Id, WalletId = wallet.Id, Comment = "Cafe del dia" },
                new Transaction { Amount = 70, Date = new DateTime(2026, 3, 20), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[0].Id, WalletId = wallet.Id, Comment = "Supermercado" }
            };

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task ClearDataAsync()
        {
            _context.Transactions.RemoveRange(_context.Transactions);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Wallets.RemoveRange(_context.Wallets);
            await _context.SaveChangesAsync();
        }
    }
}

