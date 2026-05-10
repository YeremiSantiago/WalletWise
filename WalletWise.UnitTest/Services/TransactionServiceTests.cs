using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Interfaces;
using WalletWise.Application.Mappings.EntityToDto;
using WalletWise.Application.Services;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using Xunit;

namespace WalletWise.Unit.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly TransactionService _transactionService;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IClock> _iClockMock;
        private readonly Mock<ILogger<Transaction>> _loggerMock;
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;

        private const string TestUserId = "1";

        public TransactionServiceTests()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _iClockMock = new Mock<IClock>();
            _loggerMock = new Mock<ILogger<Transaction>>();
            _walletRepoMock = new Mock<IWalletRepository>();
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _currentUserServiceMock.Setup(x => x.UserId).Returns(TestUserId);

            var config = new MapperConfiguration(
              c => { c.AddProfile<TransactionMappingProfile>(); },
              NullLoggerFactory.Instance);

            var mapper = config.CreateMapper();
            _transactionService = new TransactionService(
                _transactionRepoMock.Object, 
                _iClockMock.Object, 
                _loggerMock.Object, 
                mapper, 
                _walletRepoMock.Object, 
                _categoryRepoMock.Object,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_WhenGetAllTransactionsExisting_ReturnSuccessWithAllTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>()
            {
                new Transaction { Id = 1, Amount = 125, Date = DateTime.Parse("10/09/2026"), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = 1, WalletId = 1 },
                new Transaction { Id = 2, Amount = 500, Date = DateTime.Parse("24/02/2026"), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 2 },
                new Transaction { Id = 3, Amount = 7500, Date = DateTime.Parse("25/02/2026"), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 2 }
            };

            _transactionRepoMock.Setup(r => r.GetAllByUserAsync(TestUserId)).ReturnsAsync(transactions);

            // Act 
            var result = await _transactionService.GetAllTransactionsAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value?.Count());
        }


        [Fact]
        public async Task GetTransactionByIdAsync_WhenGetATransactionExisting_ReturnSuccessOperationWithValue()
        {
                                                                                                // Arrange
            var transaction = new Transaction { Id = 2, Amount = 500, Date = DateTime.Parse("24/02/2026"), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 2 };
            int id = 2;

            _transactionRepoMock.Setup(r => r.GetByIdForUserAsync(id, TestUserId)).ReturnsAsync(transaction);

            // Act
            var result = await _transactionService.GetTransactionByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(id, result.Value?.Id);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_WhenATransactionIsObtainedThatDoesNotExist_ReturnFailureOperationWithError()
        {
            // Arrange
            int id = 10;
            _transactionRepoMock.Setup(r => r.GetByIdForUserAsync(id, TestUserId)).ReturnsAsync((Transaction?)null);

            // Act
            var result = await _transactionService.GetTransactionByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.Null(result.Value);
            Assert.Equal($"La transaccion con el id {id} no existe", result.Error); 
        }

        [Fact]
        public async Task CreateTransactionAsync_WhenCreatedATransaction_ReturnOperationIsSuccessWithValue()
        {
            // Arrange
            var category = new Category {Id = 1, Name = "Comida", UserId = TestUserId, IsDeleted = false};
            var wallet = new Wallet { Id = 1, Name = "Maximo", UserId = TestUserId };

            var transactionDto = new CreateTransactionRequestDto
            {
                Amount = 15000,
                Date = DateTime.Parse("24/02/2026"),
                Type = TypeTransaction.Income,
                Comment = "Este es mi primer ingreso",
                CategoryId = 1,
                WalletId = 1
            };

            _iClockMock.Setup(c => c.UtcNow()).Returns(DateTime.Parse("10/03/2026"));
            
            _walletRepoMock.Setup(r => r.GetByIdForUserAsync(1, TestUserId)).ReturnsAsync(wallet);
            _categoryRepoMock.Setup(r => r.GetCategoryActiveByIdAsync(1, TestUserId)).ReturnsAsync(category);

            _transactionRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync((Transaction w) => w);

            // Act
            var result = await _transactionService.CreateTransactionAsync(transactionDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task UpdateTransactionAsync_WhenATransactionIsUpdated_ReturnOperationIsSuccessWithValue()
        {
            // Arrange
            var transaction = new Transaction { Id = 3, Amount = 7500, Date = DateTime.Parse("25/02/2026"), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 2 };

            var transactionDto = new UpdateTransactionRequestDto
            {
                Amount = 2000,
                Date = DateTime.Parse("25/02/2026"),
                Type = TypeTransaction.Income,
                Comment = "Se me olvido añadir algo",
                CategoryId = 1,
                WalletId = 1
            };

            int id = 3;

            _transactionRepoMock.Setup(r => r.GetByIdForUserAsync(id, TestUserId)).ReturnsAsync(transaction);
            _transactionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Transaction>()));

            // Act  
            var result = await _transactionService.UpdateTransactionAsync(id, transactionDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value?.Id);
        }

        [Fact]
        public async Task DeleteTransactionAsync_WhenTransactionIsDeleted_ReturnOperationIsSuccess()
        {
            // Arrange
            var transaction = new Transaction { Id = 2, Amount = 500, Date = DateTime.Parse("24/02/2026"), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 2 };
            int id = 2;

            _transactionRepoMock.Setup(r => r.GetByIdForUserAsync(id, TestUserId)).ReturnsAsync(transaction);
            _transactionRepoMock.Setup(r => r.RemoveAsync(id));

            // Act 
            var result = await _transactionService.DeleteTransactionAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task GetByDateRangeAsync_WhenTransactionsExistingInRangeDates_ReturnOperationSuccessWithTransactionsExisting()
        {
            // Arrange
            var transactions = new List<Transaction>()
            {
                new Transaction { Id = 1, Amount = 125, Date = DateTime.Parse("25/02/2026"), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = 1, WalletId = 1 },
                new Transaction { Id = 2, Amount = 500, Date = DateTime.Parse("26/02/2026"), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 3, WalletId = 2 }
            };

            DateTime start = DateTime.Parse("25/02/2026");
            DateTime end = DateTime.Parse("26/02/2026");

            _transactionRepoMock.Setup(r => r.GetByDateRangeAsync(TestUserId, start, end))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetByDateRangeAsync(start, end);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value?.Count());
        }

        [Fact]
        public async Task GetByTypeTransactionAsync_WhenYouGetAllExistingTransactionsOfATransactionType_ReturnOperationSuccessWithAllTransactionsOfAType()
        {
            // Arrange
            var transactions = new List<Transaction>()
            {
                new Transaction { Id = 1, Amount = 125, Date = DateTime.Parse("25/02/2026"), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = 1, WalletId = 1 },
                new Transaction { Id = 2, Amount = 500, Date = DateTime.Parse("26/02/2026"), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = 3, WalletId = 2 }
            };

            var type = TypeTransaction.Income;

            _transactionRepoMock.Setup(r => r.GetByTypeTransactionAsync(TestUserId, type))
                .ReturnsAsync(transactions);

            // Act 
            var result = await _transactionService.GetByTypeTransactionAsync(type);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.True(result.Value?.All(x => x.Type == type));
        }

        [Fact]
        public async Task GetAllTransactionsByCategoryAsync_WhenAllTransactionsExistingHasACategorySpecific_ReturnOperationSuccessAllTransactionesByCategory()
        {
            // Arrange
            var transactions = new List<Transaction>()
            {
                new Transaction { Id = 1, Amount = 125, Date = DateTime.Parse("10/09/2026"), Type = TypeTransaction.Income, UserId = TestUserId, CategoryId = 2, WalletId = 1 },
                new Transaction { Id = 2, Amount = 500, Date = DateTime.Parse("24/02/2026"), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 2, WalletId = 2 },
                new Transaction { Id = 3, Amount = 7500, Date = DateTime.Parse("25/02/2026"), Type = TypeTransaction.Expense, UserId = TestUserId, CategoryId = 2, WalletId = 2 }
            };

            int id = 2;

            _transactionRepoMock.Setup(r => r.GetAllTransactionsByCategoryAsync(TestUserId, id))
                .ReturnsAsync(transactions);

            // Act
            var result = await _transactionService.GetAllTransactionsByCategoryAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.True(result.Value?.All(x => x.CategoryId == id));
        }
    }
}

