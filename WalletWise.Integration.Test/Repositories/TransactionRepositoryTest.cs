using Microsoft.Data.Sqlite;
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
using Xunit;

namespace WalletWise.Integration.Test.Repositories
{
    public class TransactionRepositoryTest
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly AppDbContext _context;
        private const string TestUserId = "1";

        public TransactionRepositoryTest()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _transactionRepository = new TransactionRepository(_context);
        }

        [Fact]
        public async Task GetAllTransactions_WhenTransactionsHaveBeenCreated_ReturnsAlltransactionsExisting()
        {
            // Arrange
            await SeedDataAsync();

            // Act
            var result = await _transactionRepository.GetAllByUserAsync(TestUserId);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetTransactionById_WhenATransactionExisting_ReturnTransactionFound()
        {
            // Arrange
            await SeedDataAsync();
            int id = 2;

            // Act
            var result = await _transactionRepository.GetByIdForUserAsync(id, TestUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task AddTransactionAsync_WhenTransactionIsAdded_ShouldBeCreatedYRetunValue()
        {
            // Arrange
            var category = new Category { Name = "Comida", UserId = TestUserId, Type = TypeTransaction.Expense };
            var wallet = new Wallet { Id = 1, Name = "Trabajo", UserId = TestUserId };

            await _context.Categories.AddAsync(category);
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            var transaction = new Transaction
            {
                Amount = 15000,
                Date = new DateTime(2026, 2, 26),
                Type = TypeTransaction.Income,
                Comment = "Primera transaction",
                UserId = TestUserId,
                CategoryId = 1,
                WalletId = 1
            };

            // Act 
            var result = await _transactionRepository.AddAsync(transaction);

            // Assert
            bool exist = await _context.Transactions.AnyAsync(x => x.Id == result.Id);
            Assert.True(exist);
        }

        [Fact]
        public async Task UpdateTransactionAsync_WhenTransactionIsUpdated_TheTransactionShouldBeUpdated()
        {
            // Arrange
            await SeedDataAsync();
            int id = 2;

            var transaction = await _context.Transactions.FindAsync(id);
            transaction!.Amount = 30000;
            transaction.Comment = "Modificado";

            // Act
            await _transactionRepository.UpdateAsync(transaction);

            // Assert
            bool exist = await _context.Transactions.AnyAsync(x => x.Amount == 30000 && x.Comment == "Modificado" && x.Id == id);
            Assert.True(exist);
        }

        [Fact]
        public async Task RemoveTransactionAsync_WhenTransactionIsRemoved_TransactionShouldBeDeleted()
        {
            // Arrange
            await SeedDataAsync();
            int id = 2;

            // Act
            await _transactionRepository.RemoveAsync(id);

            // Assert
            var exist = await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);
            Assert.Null(exist);
        }

        [Fact]
        public async Task GetByDateRangeAsync_WhenTransactionsExistInRange_ShouldReturnListOfTransactions()
        {
            // Arrange
            await SeedSearchDataAsync();
            DateTime start = new DateTime(2026, 2, 28);
            DateTime end = new DateTime(2026, 2, 28);

            // Act
            var result = await _transactionRepository.GetByDateRangeAsync(TestUserId, start, end);

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.All(x => x.Date >= start && x.Date <= end));
        }

        [Fact]
        public async Task GetByTypeTransactionAsync_WhenTransactionsOfTypeExist_ShouldReturnListOfTransactions()
        {
            // Arrange
            await SeedSearchDataAsync();
            var type = TypeTransaction.Income;

            // Act 
            var result = await _transactionRepository.GetByTypeTransactionAsync(TestUserId, type);

            // Assert
            Assert.NotEmpty(result);
            Assert.True(result.All(x => x.Type == type));
        }

        [Fact]
        public async Task GetAllTransactionsByCategoryAsync_WhenCategoryHasTransactions_ShouldReturnListOfTransactions()
        {
            // Arrange
            await SeedSearchDataAsync();
            int categoryId = 2;

            // Act
            var result = await _transactionRepository.GetAllTransactionsByCategoryAsync(TestUserId, categoryId);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task ExistsTransactionByCategoryAsync_WhenCategoryHasTransactions_ShouldReturnTrue()
        {
            // Arrange
            await SeedSearchDataAsync();
            int categoryId = 3;

            // Act
            bool result = await _transactionRepository.ExistsTransactionByCategoryAsync(categoryId, TestUserId);

            // Assert
            Assert.True(result);
        }

        

        private async Task SeedDataAsync()
        {
            var categories = new List<Category>
            {
                new Category {Id = 1, Name = "Comida", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Id = 2, Name = "Servicios", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Id = 3, Name = "Transporte", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense}
            };

            var wallets = new List<Wallet>
            {
                new Wallet { Id = 1, Name = "Trabajo", UserId = TestUserId },
                new Wallet { Id = 2, Name = "Tarjeta Credito", UserId = TestUserId },
                new Wallet { Id = 3, Name = "Tarjeta De Debito", UserId = TestUserId }
            };

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, Amount = 125, Date = new DateTime(2026, 9, 10), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = 1, WalletId = 1 },
                new Transaction { Id = 2, Amount = 500, Date = new DateTime(2026, 2, 24), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 2, WalletId = 2 },
                new Transaction { Id = 3, Amount = 7500, Date = new DateTime(2026, 2, 25), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 3 }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedSearchDataAsync()
        {
            var categories = new List<Category>
            {
                new Category {Id = 1, Name = "Comida", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Id = 2, Name = "Servicios", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Id = 3, Name = "Transporte", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense}
            };

            var wallets = new List<Wallet>
            {
                new Wallet { Id = 1, Name = "Trabajo", UserId = TestUserId },
                new Wallet { Id = 2, Name = "Tarjeta Credito", UserId = TestUserId },
                new Wallet { Id = 3, Name = "Tarjeta De Debito", UserId = TestUserId }
            };

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, Amount = 125, Date = new DateTime(2026, 2, 26), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = 1, WalletId = 1 },
                new Transaction { Id = 2, Amount = 500, Date = new DateTime(2026, 2, 27), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 2, WalletId = 2 },
                new Transaction { Id = 3, Amount = 7500, Date = new DateTime(2026, 2, 28), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 3 },
                new Transaction { Id = 4, Amount = 8500, Date = new DateTime(2026, 2, 28), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 3 }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }
    }
}

