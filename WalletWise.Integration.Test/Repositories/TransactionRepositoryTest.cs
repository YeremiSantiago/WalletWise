using Microsoft.EntityFrameworkCore;
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
    public class TransactionRepositoryTest : IAsyncLifetime
    {
        private  ITransactionRepository _transactionRepository;
        private  AppDbContext _context;
        private const string TestUserId = "1";
        private readonly DatabaseFixture _fixture;

        public TransactionRepositoryTest(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _context = _fixture.CreateDbContext();
            await _context.Database.MigrateAsync();

            _context.Transactions.RemoveRange(_context.Transactions);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Wallets.RemoveRange(_context.Wallets);
            await _context.SaveChangesAsync();
            _transactionRepository = new TransactionRepository(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
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
            var transaction = await _context.Transactions.FirstAsync();
            int id = transaction.Id;

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
            var wallet = new Wallet { Name = "Trabajo", UserId = TestUserId };

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
                CategoryId = category.Id,
                WalletId = wallet.Id
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
            var transaction = await _context.Transactions.FirstAsync();

            transaction!.Amount = 30000;
            transaction.Comment = "Modificado";

            // Act
            await _transactionRepository.UpdateAsync(transaction);

            // Assert
            bool exist = await _context.Transactions.AnyAsync(x => x.Amount == 30000 && x.Comment == "Modificado" && x.Id == transaction.Id);
            Assert.True(exist);
        }

        [Fact]
        public async Task RemoveTransactionAsync_WhenTransactionIsRemoved_TransactionShouldBeDeleted()
        {
            // Arrange
            await SeedDataAsync();
            var transactionToDelete = await _context.Transactions.FirstAsync(t => t.Amount == 500);
            int id = transactionToDelete.Id;

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
            var (categories, _, _) = await SeedSearchDataAsync();
            int categoryId = categories[1].Id;

            // Act
            var result = await _transactionRepository.GetAllTransactionsByCategoryAsync(TestUserId, categoryId);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task ExistsTransactionByCategoryAsync_WhenCategoryHasTransactions_ShouldReturnTrue()
        {
            // Arrange
            var (categories, _, _) = await SeedSearchDataAsync();
            int categoryId = categories[2].Id;

            // Act
            bool result = await _transactionRepository.ExistsTransactionByCategoryAsync(categoryId, TestUserId);

            // Assert
            Assert.True(result);
        }

        

        private async Task SeedDataAsync()
        {
            var categories = new List<Category>
            {
                new Category {Name = "Comida", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Name = "Servicios", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Name = "Transporte", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense}
            };

            var wallets = new List<Wallet>
            {
                new Wallet { Name = "Trabajo", UserId = TestUserId },
                new Wallet { Name = "Tarjeta Credito", UserId = TestUserId },
                new Wallet { Name = "Tarjeta De Debito", UserId = TestUserId }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.SaveChangesAsync();

            var transactions = new List<Transaction>
            {
                new Transaction { Amount = 125, Date = new DateTime(2026, 9, 10), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = categories[0].Id, WalletId = wallets[0].Id },
                new Transaction { Amount = 500, Date = new DateTime(2026, 2, 24), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[1].Id, WalletId = wallets[1].Id },
                new Transaction { Amount = 7500, Date = new DateTime(2026, 2, 25), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[2].Id, WalletId = wallets[2].Id }
            };

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();
        }

        private async Task<(List<Category>, List<Wallet>, List<Transaction>)> SeedSearchDataAsync()
        {
            var categories = new List<Category>
            {
                new Category {Name = "Comida", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Name = "Servicios", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Name = "Transporte", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense}
            };

            var wallets = new List<Wallet>
            {
                new Wallet { Name = "Trabajo", UserId = TestUserId },
                new Wallet { Name = "Tarjeta Credito", UserId = TestUserId },
                new Wallet { Name = "Tarjeta De Debito", UserId = TestUserId }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.SaveChangesAsync();

            var transactions = new List<Transaction>
            {
                new Transaction { Amount = 125, Date = new DateTime(2026, 2, 26), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = categories[0].Id, WalletId = wallets[0].Id },
                new Transaction { Amount = 500, Date = new DateTime(2026, 2, 27), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[1].Id, WalletId = wallets[1].Id },
                new Transaction { Amount = 7500, Date = new DateTime(2026, 2, 28), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[2].Id, WalletId = wallets[2].Id },
                new Transaction { Amount = 8500, Date = new DateTime(2026, 2, 28), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = categories[2].Id, WalletId = wallets[2].Id }
            };

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            return (categories, wallets, transactions);
        }
    }
}

