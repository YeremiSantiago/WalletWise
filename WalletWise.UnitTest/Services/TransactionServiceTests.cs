using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Transaction;
using WalletWise.Application.Mappings.EntityToDto;
using WalletWise.Application.Services;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Unit.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly TransactionService _transactionService;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IClock> _iClockMock;
        private readonly Mock<ILogger<Transaction>> _loggerMock;

        public TransactionServiceTests()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _iClockMock = new Mock<IClock>();
            _loggerMock = new Mock<ILogger<Transaction>>();

            var config = new MapperConfiguration(
                c => c.AddProfile<TransactionMappingProfile>(),
                NullLoggerFactory.Instance);

            var mapper = config.CreateMapper();
            _transactionService = new TransactionService(_transactionRepoMock.Object, _iClockMock.Object, _loggerMock.Object, mapper);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_WhenGetAllTransactionsExisting_ReturnSuccessWithAllTransactions()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("10/09/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 500,
                    Date = DateTime.Parse("24/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                },
                new Transaction
                {
                    Id = 3,
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                }

            };

            _transactionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(transactions);

            // Act 

            var result = await _transactionService.GetAllAsync();

            // Assert

            Assert.Equal(3, result.Value.Count());
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotEmpty(result.Value);

        }


        [Fact]
        public async Task GetTransactionByIdAsync_WhenGetATransactionExisting_ReturnSuccessOperationWithValue()
        {
            // Arrrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("10/09/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 500,
                    Date = DateTime.Parse("24/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                },
                new Transaction
                {
                    Id = 3,
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                }
            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };



            int id = 2;


            _transactionRepoMock.Setup(r => r.GetByIdAsync(It.Is<int>(x => x == id)))
                .ReturnsAsync(transactions[1]);

            // Act

            var result = await _transactionService.GetByIdAsync(id);

            // Assert

            Assert.Equal(result.Value.Id, id);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);

        }

        [Fact]
        public async Task GetTransactionByIdAsync_WhenATransactionIsObtainedThatDoesNotExist_ReturnFailureOperationWithError()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("10/09/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 500,
                    Date = DateTime.Parse("24/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                },
                new Transaction
                {
                    Id = 3,
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                }
            };

            int id = 10;

            _transactionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(default(Transaction));

            // Act

            var result = await _transactionService.GetByIdAsync(id);

            // Assert

            Assert.NotNull(result.Error);
            Assert.Null(result.Value);
            Assert.False(result.IsSuccess);
            Assert.Equal($"La entidad con el Id {id} no pudo ser encontrada", result.Error);

        }

        [Fact]
        public async Task CreateTransactionAsync_WhenCreatedATransaction_ReturnOperationIsSuccessWithValue()
        {
            // Arrange

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };

            var wallets = new List<Wallet>()
            {
                new Wallet { Id = 1, Name = "Maximo", UserId = 1 },
                new Wallet { Id = 2, Name = "Pedro", UserId = 1 },
                new Wallet { Id = 3, Name = "Antonio", UserId = 1 }
            };

            var transactionDto = new CreateTransactionRequestDto
            {
                Amount = 15000,
                Date = DateTime.Parse("25/02/2026"),
                Type = TypeTransaction.Income,
                Comment = "Este es mi primer gasto",
                CategoryId = 1,
                WalletId = 1
            };


            _transactionRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction w) => w);

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

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("10/09/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 500,
                    Date = DateTime.Parse("24/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                },
                new Transaction
                {
                    Id = 3,
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                }
            };

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

            _transactionRepoMock.Setup(r => r.UpdateAsync(It.Is<Transaction>(x => x.Id == id)));
            _transactionRepoMock.Setup(r => r.GetByIdAsync(It.Is<int>(x => x == id))).ReturnsAsync(transactions[2]);

            // Act  

            var result = await _transactionService.UpdateTransactionAsync(id, transactionDto);

            // Assert

            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.Equal(3, result.Value.Id);


        }

        [Fact]
        public async Task DeleteTransactionAsync_WhenTransactionIsDeleted_ReturnOperationIsSuccess()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("10/09/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 500,
                    Date = DateTime.Parse("24/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                },
                new Transaction
                {
                    Id = 3,
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                }

            };

            int id = 2;

            _transactionRepoMock.Setup(r => r.RemoveAsync(It.Is<int>(x => x == id)));
            _transactionRepoMock.Setup(r => r.GetByIdAsync(It.Is<int>(x => x == id))).ReturnsAsync(transactions[1]);

            // Act 

            var result = await _transactionService.DeleteTransactionAsync(id);

            // Assert

            Assert.Equal(true, result.Value);
            Assert.Null(result.Error);
            Assert.True(result.IsSuccess);

        }

        [Fact]
        public async Task GetByDateRangeAsync_WhenTransactionsExistingInRangeDates_ReturnOperationSuccessWithTransactionsExisting()
        {
            // Arrange
            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 500,
                    Date = DateTime.Parse("26/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                }

            };

            DateTime start = DateTime.Parse("25/02/2026");
            DateTime end = DateTime.Parse("26/02/2026");

            _transactionRepoMock.Setup(r => r.GetByDateRangeAsync(It.Is<DateTime>(x => x == start), It.Is<DateTime>(x => x == end)))
                .ReturnsAsync(transactions);

            // Act

            var result = await _transactionService.GetByDateRangeAsync(start, end);

            // Assert

            Assert.Equal(2, result.Value.Count());
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value);
            Assert.Null(result.Error);

        }

        [Fact]
        public async Task GetByTypeTransactionAsync_WhenYouGetAllExistingTransactionsOfATransactionType_ReturnOperationSuccessWithAllTransactionsOfAType()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 500,
                    Date = DateTime.Parse("26/02/2026"),
                    Type = TypeTransaction.Income,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 2
                }
            };

            var type = TypeTransaction.Income;

            _transactionRepoMock.Setup(r => r.GetByTypeTransactionAsync(It.Is<TypeTransaction>(x => x == type)))
                .ReturnsAsync(transactions);

            // Act 

            var result = await _transactionService.GetByTypeTransactionAsync(type);

            // Assert

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.All(x => x.Type == type));
            Assert.Null(result.Error);
            Assert.NotEmpty(result.Value);


        }

        [Fact]
        public async Task GetAllTransactionsByCategoryAsync_WhenAllTransactionsExistingHasACategorySpecific_ReturnOperationSuccessAllTransactionesByCategory()
        {
            // Assert

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("10/09/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 2,
                    WalletId = 1
                },
                new Transaction
                {
                    Id = 2,
                    Amount = 500,
                    Date = DateTime.Parse("24/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 2,
                    WalletId = 2
                },
                new Transaction
                {
                    Id = 3,
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 2,
                    WalletId = 2
                }

            };

            int id = 2;

            _transactionRepoMock.Setup(r => r.GetAllTransactionsByCategoryAsync(It.Is<int>(x => x == id)))
                .ReturnsAsync(transactions);

            // Act

            var result = await _transactionService.GetAllTransactionsByCategoryAsync(id);

            // Assert

            Assert.True(result.Value.All(x => x.CategoryId == id));
            Assert.Null(result.Error);
            Assert.NotEmpty(result.Value);
            Assert.True(result.IsSuccess);

        }



    }

}

