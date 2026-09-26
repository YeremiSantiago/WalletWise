using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Infrastructure.Context;
using WalletWise.Infrastructure.Repositories;
using WalletWise.Domain.Common.Enums;
using WalletWise.Integration.Test.Infraestructure;

namespace WalletWise.Integration.Test.Repositories
{
    [Collection("Database")]
    public class CategoryRepositoryTests : IAsyncLifetime
    {
        private ICategoryRepository _categoryRepository;
        private AppDbContext _context;
        private readonly DatabaseFixture _fixture;

        public CategoryRepositoryTests(DatabaseFixture fixture)
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
            _categoryRepository = new CategoryRepository(_context);
        }
        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenAtCategoryExist_ReturntCategoryExisting()
        {
            // Arrange 
            var Categories = new List<Category>()
            {
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Comida",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Compras",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Servicios",
                    UserId = "1"
                }
            };

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();

            int id = Categories[0].Id;

            // Act 

            var result = await _categoryRepository.GetByIdAsync(id);

            // Assert

            Assert.Equal("Comida", result.Name);

        }

        [Fact]

        public async Task GetAllCategory_WhenCategoriesExisting_ReturnAllCategoriesExistingg()
        {
            // Arrange
            var Categories = new List<Category>()
            {
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Ropa",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Telecomunicaciones",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Servicios",
                    UserId = "1"
                }
            };

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();

            int id = Categories[1].Id;

            // Act 

            var results = await _categoryRepository.GetByIdAsync(id);

            // Assert

            Assert.NotNull(results);
            Assert.Equal(Categories[1].Name, results.Name);

        }


        [Fact]
        public async Task AddCategoryAsync_WhenCreatedACategory_ReturnCategories()
        {
            // Arrange 

            var Category = new Category 
            {
                Type = TypeTransaction.Expense,
                Name = "Antonio",
                UserId = "1"
            };

            // Act

            var result = await _categoryRepository.AddAsync(Category);

            // AssertS

            Assert.NotNull(result);
            Assert.Equal(result.Name, Category.Name);

        }


        [Fact]
        public async Task UpdateCategory_WhenCategoryIsUpdated_ShouldUpdatedCorrectly()
        {
            // Arrange

            var Categories = new List<Category>()
            {
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Comida",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Ropa",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "traje",
                    UserId = "1"
                }
            };

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();

            int id = Categories[0].Id;

            var categoryTrack = await _context.Categories.FindAsync(id);

            categoryTrack.Name = "Comidaaaaaaaaaaaaaaaaaaaa";


            // Act

            await _categoryRepository.UpdateAsync(categoryTrack);

            // Assert

            var Category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            Assert.NotNull(Category);
            Assert.Equal(Category.Name, categoryTrack.Name);

        }

        [Fact]
        public async Task RemoveCategoryAsync_WhenCategoryIsDeleted_ShouldCategoryProperly()
        {
            // Arrange 

            var Categories = new List<Category>()
            {
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Ropa",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Comida",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Ahorro",
                    UserId = "1"
                }
            };

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();

            int id = Categories[1].Id;

            // Act 

            await _categoryRepository.RemoveAsync(id);

            // Assert

            bool exist = await _context.Categories.AnyAsync(x => x.Name == "Comida");

            Assert.False(exist);

        }

        [Fact]
        public async Task ExistsAsync_WhenCategoryExistsUnderCondition_ReturnReturnsTrueOrFalse()
        {
            // Arrange 

            var Categories = new List<Category>()
            {
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Comida",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Ropa",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Name = "Traje",
                    UserId = "1"
                }
            };

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();


            // Act

            var result = await _categoryRepository.ExistsAsync(x => x.Name == "Comida");

            // Assert

            Assert.True(result);

        }


    }
}

