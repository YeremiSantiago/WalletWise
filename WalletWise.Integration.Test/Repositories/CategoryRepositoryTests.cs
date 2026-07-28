using Microsoft.Data.Sqlite;
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

namespace WalletWise.Integration.Test.Repositories
{
    public class CategoryRepositoryTests
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly AppDbContext _context;

        public CategoryRepositoryTests()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            _context = new AppDbContext(options);

            _context.Database.EnsureCreated();

            _categoryRepository = new CategoryRepository(_context);
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
                    Id = 1,
                    Name = "Comida",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 2,
                    Name = "Compras",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 3,
                    Name = "Servicios",
                    UserId = "1"
                }
            };

            int id = 1;

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();


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
                    Id = 1,
                    Name = "Ropa",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 2,
                    Name = "Telecomunicaciones",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 3,
                    Name = "Servicios",
                    UserId = "1"
                }
            };

            int id = 2;

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();

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
                    Id = 1,
                    Name = "Comida",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 2,
                    Name = "Ropa",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 3,
                    Name = "traje",
                    UserId = "1"
                }
            };

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();

            int id = 1;

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
                    Id = 1,
                    Name = "Ropa",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 2,
                    Name = "Comida",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 3,
                    Name = "Ahorro",
                    UserId = "1"
                }
            };

            await _context.Categories.AddRangeAsync(Categories);

            await _context.SaveChangesAsync();

            int id = 2;

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
                    Id = 1,
                    Name = "Comida",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 2,
                    Name = "Ropa",
                    UserId = "1"
                },
                new Category 
                {
                    Type = TypeTransaction.Expense,
                    Id = 3,
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

