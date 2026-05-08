using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Mappings.EntityToDto;
using WalletWise.Application.Services;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;

namespace WalletWise.Unit.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly CategoryService _categoryService;
        private readonly Mock<ILogger<Category>> _loggerMock;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IClock> _clockMock;

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _clockMock = new Mock<IClock>();
            _loggerMock = new Mock<ILogger<Category>>();
            _transactionRepoMock = new Mock<ITransactionRepository>();

            var config = new MapperConfiguration(
            cfg => cfg.AddProfile<CategoryMappingProfile>(), NullLoggerFactory.Instance
            );
            var mapper = config.CreateMapper();

            _categoryService = new CategoryService(
                _categoryRepoMock.Object,
                _transactionRepoMock.Object,
                _loggerMock.Object,
                _clockMock.Object,
                mapper
            );

        }

        [Fact]
        public async Task GetAllCategoriesAsync_WhenGetALlTheCategoriesExisting_ReturnsSuccessWithValues()
        {
            // Arrange

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = "1", IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = "1", IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = "1", IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = "1", IsDeleted = false}
            };

            _categoryRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(categories);

            // Act 

            var result = await _categoryService.GetAllAsync();

            // Assert

            Assert.Equal(4, result.Value.Count());
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotEmpty(result.Value);


        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenGetCategoryExisting_ReturnCategoryWithValue()
        {
            // Assert 

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = "1", IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = "1", IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = "1", IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = "1", IsDeleted = false}
            };

            int id = 3;

            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.Is<int>(x => x == id)))
                .ReturnsAsync(categories[2]);

            // Act 

            var result = await _categoryService.GetByIdAsync(id);

            // Assert

            Assert.NotNull(result.Value);
            Assert.Equal("Transporte", result.Value.Name);
            Assert.Null(result.Error);
            Assert.True(result.IsSuccess);

        }

        [Fact]
        public async Task GetCategoryByIdAsync_whenACategoryDontExist_ReturnFailureWithError()
        {
            // Assert

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = "1", IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = "1", IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = "1", IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = "1", IsDeleted = false}
            };

            int id = 5;

            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.Is<int>(x => x == id)))
                .ReturnsAsync(default(Category));

            // Assert

            var result = await _categoryService.GetByIdAsync(id);

            // Assert

            Assert.Null(result.Value);
            Assert.NotEmpty(result.Error);
            Assert.False(result.IsSuccess);


        }

        [Fact]
        public async Task CreateCategoryAsync_WhenACategoryIsCreated_ReturnSuccessWithValue()
        {
            // Arrange

            var categoryDto = new CreateCategoryRequestDto()
            {
                Name = "Minimo Antonio"
            };

            _categoryRepoMock.Setup(r => r.AddAsync(It.Is<Category>(r => r.Name == categoryDto.Name)))
                .ReturnsAsync((Category w) => w);

            // Act

            var result = await _categoryService.CreateCategoryAsync(categoryDto);

            // Assert

            Assert.Equal(categoryDto.Name, result.Value.Name);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);

        }

        [Fact]
        public async Task UpdateCategoryAsync_WhenCategoryIsUpdated_ReturnWalletWithSuccess()
        {
            // Arrange

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = "1", IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = "1", IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = "1", IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = "1", IsDeleted = false}
            };



            var categoryDto = new UpdateCategoryRequestDto
            {
                Name = "Antonio Rodriguez"
            };

            int id = 1;

            _categoryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Category>()));
            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.Is<int>(x => x == id))).ReturnsAsync(categories[0]);

            // Act

            var result = await _categoryService.UpdateCategoryAsync(id, categoryDto);

            // Assert

            Assert.Equal(id, result.Value.Id);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.Equal(categoryDto.Name, result.Value.Name);

        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryIsDeleted_ReturnOperationIsSuccess()
        {
            // Arrange 

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = "1", IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = "1", IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = "1", IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = "1", IsDeleted = false}
            };

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("10/09/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = "1",
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
                    UserId = "1",
                    CategoryId = 3,
                    WalletId = 2
                }

            };

            var wallet = new List<Wallet>()
            {
                new Wallet
                {
                    Id = 1,
                    Name = "Sueldo",
                    UserId = "1"
                },
                new Wallet
                {
                    Id = 2,
                    Name = "Tarjeta de credito",
                    UserId = "1"
                }
            };

            int id = 2;

            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.Is<int>(x => x == id))).ReturnsAsync(categories[1]);
            _transactionRepoMock.Setup(r => r.ExistsTransactionByCategoryAsync(It.Is<int>(x => x == id))).ReturnsAsync(false);
            _categoryRepoMock.Setup(r => r.UpdateAsync(It.Is<Category>(r => r.Id.Equals(id))));

            // Act 

            var result = await _categoryService.DeleteCategoryAsync(id);

            // Assert

            Assert.True(result.Value);
            Assert.Null(result.Error);
            Assert.Equal(true, result.IsSuccess);
            Assert.NotNull(result.Value);


        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenACategoryHasATransaction_ReturnOperationIsFailure()
        {
            // Arrange 

            var categories = new List<Category>()
            {
                new Category {Id = 1, Name = "Comida", UserId = "1", IsDeleted = false},
                new Category {Id = 2, Name = "Servicios", UserId = "1", IsDeleted = false},
                new Category {Id = 3, Name = "Transporte", UserId = "1", IsDeleted = false},
                new Category {Id = 4, Name = "Comptras", UserId = "1", IsDeleted = false}
            };

            var transactions = new List<Transaction>()
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 125,
                    Date = DateTime.Parse("10/09/2026"),
                    Type = TypeTransaction.Income,
                    Comment = null,
                    UserId = "1",
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
                    UserId = "1",
                    CategoryId = 3,
                    WalletId = 2
                }
            };

            var wallet = new List<Wallet>()
            {
                new Wallet
                {
                    Id = 1,
                    Name = "Sueldo",
                    UserId = "1"
                },
                new Wallet
                {
                    Id = 2,
                    Name = "Tarjeta de credito",
                    UserId = "1"
                }
            };

            int id = 2;

            _categoryRepoMock.Setup(r => r.GetByIdAsync(It.Is<int>(x => x == id))).ReturnsAsync(categories[1]);
            _transactionRepoMock.Setup(r => r.ExistsTransactionByCategoryAsync(It.Is<int>(x => x == id))).ReturnsAsync(true);
            _categoryRepoMock.Setup(r => r.UpdateAsync(It.Is<Category>(r => r.Id.Equals(id))));

            // Act 

            var result = await _categoryService.DeleteCategoryAsync(id);

            // Assert

            Assert.False(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal(false, result.IsSuccess);
            Assert.Equal("Esta categoria tiene transacciones asociadas", result.Error);



        }


    }
}
