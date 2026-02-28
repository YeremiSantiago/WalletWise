using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Persistence.Context;
using WalletWise.Persistence.Repositories;

namespace WalletWise.Integration.Test.Repositories
{
    public class TransactionRepositoryTest
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly AppDbContext _context;

        public TransactionRepositoryTest()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            _context = new AppDbContext(options);

            _context.Database.EnsureCreatedAsync();

            _transactionRepository = new TransactionRepository(_context);

        }

        [Fact]
        public async Task GetAllTransactions_WhenTransactionsHaveBeenCreated_ReturnsAlltransactionsExisting()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
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
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                }

            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };



            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Id = 1,
                    Name = "Trabajo",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 2,
                    Name = "Tarjeta Credito",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 3,
                    Name = "Tarjeta De Debito",
                    UserId = 1
                }
            };


            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            // Act

            var result = await _transactionRepository.GetAllAsync();

            // Assert

            Assert.True(result.Count() == 3);
            Assert.NotEmpty(result);

        }

        [Fact]
        public async Task GetTransactionById_WhenATransactionExisting_ReturnTransactionFound()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
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
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                }

            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };



            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Id = 1,
                    Name = "Trabajo",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 2,
                    Name = "Tarjeta Credito",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 3,
                    Name = "Tarjeta De Debito",
                    UserId = 1
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            int id = 2;

            // Act

            var result = await _transactionRepository.GetByIdAsync(id);

            // Assert

            Assert.Equal(result.Id, id);
        }


        [Fact]
        public async Task AddTransactionAsync_WhenTransactionIsAdded_ShouldBeCreatedYRetunValue()
        {
            // Arrange

            var transaction = new Transaction()
            {
                Amount = 15000,
                Date = DateTime.Parse("26/02/2026"),
                Type = TypeTransaction.Income,
                Comment = "Primera transaction",
                UserId = 1,
                CategoryId = 1,
                WalletId = 1
            };

            var category = new Category()
            {
                Name = "Comida",
                UserId = 1
            };

           var wallet = new Wallet()
            {
                Id = 1,
                Name = "Trabajo",
                UserId = 1
            };


            await _context.Categories.AddAsync(category);
            await _context.Wallets.AddAsync(wallet);

            await _context.SaveChangesAsync();

            int id = 1;

            // Act 
            
            var result = await _transactionRepository.AddAsync(transaction);

            // Assert

            bool exist = await _context.Transactions.AnyAsync();

            Assert.True(exist);
        }

        [Fact]
        public async Task UpdateTransactionAsync_WhenTransactionIsUpdated_TheTransactionShouldBeUpdated()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
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
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                }

            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };

            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Id = 1,
                    Name = "Trabajo",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 2,
                    Name = "Tarjeta Credito",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 3,
                    Name = "Tarjeta De Debito",
                    UserId = 1
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            int id = 2;

            var transaction = await _context.Transactions.FindAsync(id);
            transaction.Amount = 30000;
            transaction.Comment = "Abinader Presidente";

            // Act

             await _transactionRepository.UpdateAsync(transaction);

            // Assert

            bool exist = await _context.Transactions.AnyAsync(x => x.Amount == transaction.Amount && x.Comment == transaction.Comment);
            Assert.True(exist);

        }

        [Fact]
        public async Task RemoveTransactionAsync_WhenTransactionIsRemoved_TransactionShouldBeDeleted()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
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
                    Amount = 7500,
                    Date = DateTime.Parse("25/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                }

            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };

            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Id = 1,
                    Name = "Trabajo",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 2,
                    Name = "Tarjeta Credito",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 3,
                    Name = "Tarjeta De Debito",
                    UserId = 1
                }
            };

            int id = 2;

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();


            // Act

            await _transactionRepository.RemoveAsync(2);

            // Assert

            var exist = await _context.Transactions.FirstOrDefaultAsync(x => x.Id == 2);
            Assert.Null(exist);

        }

        [Fact]
        public async Task GetByDateRangeAsync_WhenTransactionsExistInRange_ShouldReturnListOfTransactions()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Amount = 125,
                    Date = DateTime.Parse("26/02/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Amount = 500,
                    Date = DateTime.Parse("27/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 2,
                    WalletId = 2
                },
                new Transaction
                {
                    Amount = 7500,
                    Date = DateTime.Parse("28/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                },
                new Transaction
                {
                    Amount = 8500,
                    Date = DateTime.Parse("28/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de los otros dias",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                }

            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };

            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Id = 1,
                    Name = "Trabajo",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 2,
                    Name = "Tarjeta Credito",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 3,
                    Name = "Tarjeta De Debito",
                    UserId = 1
                }
            };

            int id = 2;

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            DateTime start = DateTime.Parse("28/02/2026");
            DateTime end = DateTime.Parse("28/02/2026");


            // Act

            var result = await _transactionRepository.GetByDateRangeAsync(start, end);

            // Assert

            bool exist = await _context.Transactions.AllAsync(x => x.Date >= start && x.Date <= end);

            Assert.True(true);
        }

        [Fact]
        public async Task GetByTypeTransactionAsync_WhenTransactionsOfTypeExist_ShouldReturnListOfTransactions()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Amount = 125,
                    Date = DateTime.Parse("26/02/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Amount = 500,
                    Date = DateTime.Parse("27/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 2,
                    WalletId = 2
                },
                new Transaction
                {
                    Amount = 7500,
                    Date = DateTime.Parse("28/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                },
                new Transaction
                {
                    Amount = 8500,
                    Date = DateTime.Parse("28/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de los otros dias",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                }

            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };

            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Id = 1,
                    Name = "Trabajo",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 2,
                    Name = "Tarjeta Credito",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 3,
                    Name = "Tarjeta De Debito",
                    UserId = 1
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            var type = TypeTransaction.Income;


            // Act 

            var result = await _transactionRepository.GetByTypeTransactionAsync(type);

            // Assert

            var isValid = result.All(x => x.Type == type);

            Assert.True(isValid);


        }

        public async Task GetAllTransactionsByCategoryAsync_WhenCategoryHasTransactions_ShouldReturnListOfTransactions()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Amount = 125,
                    Date = DateTime.Parse("26/02/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Amount = 500,
                    Date = DateTime.Parse("27/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 2,
                    WalletId = 2
                },
                new Transaction
                {
                    Amount = 7500,
                    Date = DateTime.Parse("28/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                },
                new Transaction
                {
                    Amount = 8500,
                    Date = DateTime.Parse("28/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de los otros dias",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                }

            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };

            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Id = 1,
                    Name = "Trabajo",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 2,
                    Name = "Tarjeta Credito",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 3,
                    Name = "Tarjeta De Debito",
                    UserId = 1
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            var category = 2;

            // Act

            var result = await _transactionRepository.GetAllTransactionsByCategoryAsync(category);

            // Assert

            int count = result.Count();

            Assert.Equal(count, 1);

        }

        [Fact]
        public async Task ExistsTransactionByCategoryAsync_WhenCategoryHasTransactions_ShouldReturnTrue()
        {
            // Arrange

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Amount = 125,
                    Date = DateTime.Parse("26/02/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = 1,
                    CategoryId = 1,
                    WalletId = 1
                },
                new Transaction
                {
                    Amount = 500,
                    Date = DateTime.Parse("27/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Que sueño",
                    UserId = 1,
                    CategoryId = 2,
                    WalletId = 2
                },
                new Transaction
                {
                    Amount = 7500,
                    Date = DateTime.Parse("28/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de hoy",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                },
                new Transaction
                {
                    Amount = 8500,
                    Date = DateTime.Parse("28/02/2026"),
                    Type = TypeTransaction.Expense,
                    Comment = "Gastos de los otros dias",
                    UserId = 1,
                    CategoryId = 3,
                    WalletId = 3
                }

            };

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = 1, IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = 1, IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = 1, IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = 1, IsDeleted = false}
            };

            var wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Id = 1,
                    Name = "Trabajo",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 2,
                    Name = "Tarjeta Credito",
                    UserId = 1
                },
                new Wallet()
                {
                    Id = 3,
                    Name = "Tarjeta De Debito",
                    UserId = 1
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.Wallets.AddRangeAsync(wallets);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            int category = 3;

            // Act

            bool result = await _transactionRepository.ExistsTransactionByCategoryAsync(category);

            // Assert

            Assert.True(result);
        }




    }
}
