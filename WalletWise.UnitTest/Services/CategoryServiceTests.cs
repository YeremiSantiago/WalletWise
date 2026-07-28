using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Category;
using WalletWise.Application.Interfaces;
using WalletWise.Application.Mappings.EntityToDto;
using WalletWise.Application.Services;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using Xunit;

namespace WalletWise.Unit.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<ILogger<Category>> _loggerMock;
        private readonly Mock<IClock> _clockMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly CategoryService _categoryService;

        private const string TestUserId = "1";

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _loggerMock = new Mock<ILogger<Category>>();
            _clockMock = new Mock<IClock>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            
            _currentUserServiceMock.Setup(x => x.UserId).Returns(TestUserId);

            var config = new MapperConfiguration(
                cfg => cfg.AddProfile<CategoryMappingProfile>(), NullLoggerFactory.Instance
            );
            var mapper = config.CreateMapper();

            _categoryService = new CategoryService(
                _categoryRepoMock.Object,
                _transactionRepoMock.Object,
                _loggerMock.Object,
                _clockMock.Object,
                mapper,
                _currentUserServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllCategoriesAsync_WhenGetAllTheCategoriesExisting_ReturnsSuccessWithValues()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category {Id = 1, Name = "Comida", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Id = 2, Name = "Servicios", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Id = 3, Name = "Transporte", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense},
                new Category {Id = 4, Name = "Compras", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense}
            };

            _categoryRepoMock.Setup(r => r.GetAllCategoriesActiveAsync(TestUserId))
                .ReturnsAsync(categories);

            // Act 
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(4, result.Value?.Count());
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenGetCategoryExisting_ReturnCategoryWithValue()
        {
            // Arrange 
            var category = new Category { Id = 3, Name = "Transporte", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense };
            int id = 3;

            _categoryRepoMock.Setup(r => r.GetCategoryActiveByIdAsync(id, TestUserId))
                .ReturnsAsync(category);

            // Act 
            var result = await _categoryService.GetCategoryByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal("Transporte", result.Value?.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenCategoryDontExist_ReturnFailureWithError()
        {
            // Arrange
            int id = 5;

            _categoryRepoMock.Setup(r => r.GetCategoryActiveByIdAsync(id, TestUserId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<WalletWise.Application.Exceptions.NotFoundException>(() => _categoryService.GetCategoryByIdAsync(id));
        }

        [Fact]
        public async Task CreateCategoryAsync_WhenACategoryIsCreated_ReturnSuccessWithValue()
        {
            // Arrange
            var categoryDto = new CreateCategoryRequestDto
            {
                Name = "Nueva Categoria"
            };

            _categoryRepoMock.Setup(r => r.ExistsByNameAsync(categoryDto.Name, TestUserId))
                .ReturnsAsync(false);

            _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category c) => c);

            // Act
            var result = await _categoryService.CreateCategoryAsync(categoryDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(categoryDto.Name, result.Value?.Name);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WhenCategoryIsUpdated_ReturnSuccess()
        {
            // Arrange
            int id = 1;
            var existingCategory = new Category { Id = 1, Name = "Comida", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense };
            var categoryDto = new UpdateCategoryRequestDto
            {
                Name = "Gasto Actualizado"
            };

            _categoryRepoMock.Setup(r => r.GetCategoryActiveByIdAsync(id, TestUserId))
                .ReturnsAsync(existingCategory);

            _categoryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Category>()));

            // Act
            var result = await _categoryService.UpdateCategoryAsync(id, categoryDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal("Gasto Actualizado", result.Value?.Name);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryIsDeleted_ReturnOperationIsSuccess()
        {
            // Arrange 
            int id = 2;
            var category = new Category { Id = 2, Name = "Servicios", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense };

            _categoryRepoMock.Setup(r => r.GetCategoryActiveByIdAsync(id, TestUserId))
                .ReturnsAsync(category);

            _transactionRepoMock.Setup(r => r.ExistsTransactionByCategoryAsync(id, TestUserId))
                .ReturnsAsync(false);

            _categoryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Category>()));

            // Act 
            var result = await _categoryService.DeleteCategoryAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryHasATransaction_ReturnOperationIsFailure()
        {
            // Arrange 
            int id = 2;
            var category = new Category { Id = 2, Name = "Servicios", UserId = TestUserId, IsDeleted = false, Type = TypeTransaction.Expense };

            _categoryRepoMock.Setup(r => r.GetCategoryActiveByIdAsync(id, TestUserId))
                .ReturnsAsync(category);

            // Simular que existen transacciones para esta categoría
            _transactionRepoMock.Setup(r => r.ExistsTransactionByCategoryAsync(id, TestUserId))
                .ReturnsAsync(true);

            // Act 
            var result = await _categoryService.DeleteCategoryAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.Equal(WalletWise.Application.Common.BusinessErrorCodes.ERR_CATEGORY_HAS_TRANSACTIONS, result.Error);
            Assert.False(result.Value);
        }
    }
}
