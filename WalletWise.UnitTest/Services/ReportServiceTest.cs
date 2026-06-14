using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Reports;
using WalletWise.Application.Interfaces;
using WalletWise.Application.Mappings.EntityToDto;
using WalletWise.Application.Services;
using WalletWise.Domain.Common.Enums;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Domain.Reports;
using Xunit;

namespace WalletWise.Unit.Tests.Services
{
    public class ReportServiceTest
    {
        private readonly Mock<IReportRepository> _reportRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ILogger<ReportService>> _loggerMock;
        private readonly IReportService _reportService;

        private const string TestUserId = "1";

        public ReportServiceTest()
        {

            
            _reportRepositoryMock = new Mock<IReportRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _loggerMock = new Mock<ILogger<ReportService>>();

            _currentUserServiceMock.Setup(x => x.UserId).Returns(TestUserId);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryMappingProfile>();
                cfg.AddProfile<TransactionMappingProfile>();
                cfg.AddProfile<ReportMappingProfile>();
            }, NullLoggerFactory.Instance);

            var mapper = config.CreateMapper();

            _reportService = new ReportService(
                _reportRepositoryMock.Object,
                _currentUserServiceMock.Object,
                mapper,
                _loggerMock.Object
            );
        }

        [Fact]
        public void Constructor_WhenDependenciesProvided_CreatesInstance()
        {

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryMappingProfile>();
                cfg.AddProfile<TransactionMappingProfile>();
                cfg.AddProfile<ReportMappingProfile>();
            }, NullLoggerFactory.Instance);

            var mapper = config.CreateMapper();

            var service = new ReportService(
                _reportRepositoryMock.Object,
                _currentUserServiceMock.Object,
                mapper,
                _loggerMock.Object
            );

            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetSummaryAsync_WhenUserNotAuthenticated_ReturnsFailure()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);

            // Act
            var result = await _reportService.GetSummaryAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Usuario no autenticado", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetSummaryAsync_WhenRepositoryReturnsSummary_ReturnsSuccessWithDto()
        {
            var summary = new ReportSummary
            {
                TotalIncome = 100,
                TotalExpense = 40,
                Balance = 60
            };

            _reportRepositoryMock.Setup(r => r.GetSummaryAsync(TestUserId))
                .ReturnsAsync(summary);

            var result = await _reportService.GetSummaryAsync();

            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(100, result.Value?.TotalIncome);
            Assert.Equal(40, result.Value?.TotalExpense);
            Assert.Equal(60, result.Value?.Balance);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_WhenUserNotAuthenticated_ReturnsFailure()
        {
            _currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);

            var result = await _reportService.GetMonthlySummaryAsync(2025);

            Assert.False(result.IsSuccess);
            Assert.Equal("Usuario no autenticado", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_WhenRepositoryReturnsData_ReturnsMappedDtos()
        {
            var data = new List<MonthlySummary>
            {
                new MonthlySummary { Year = 2025, Month = 1, TotalIncome = 200, TotalExpense = 50, Balance = 150 },
                new MonthlySummary { Year = 2025, Month = 2, TotalIncome = 100, TotalExpense = 40, Balance = 60 }
            };

            _reportRepositoryMock.Setup(r => r.GetMonthlySummaryAsync(TestUserId, 2025))
                .ReturnsAsync(data);

            var result = await _reportService.GetMonthlySummaryAsync(2025);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value?.Count());
            Assert.Contains(result.Value!, x => x.Month == 1 && x.TotalIncome == 200);
            Assert.Contains(result.Value!, x => x.Month == 2 && x.Balance == 60);
        }

        [Fact]
        public async Task GetByCategoryAsync_WhenStartGreaterThanEnd_ReturnsFailure()
        {
            var start = new DateTime(2025, 2, 1);
            var end = new DateTime(2025, 1, 1);

            var result = await _reportService.GetByCategoryAsync(start, end, TypeTransaction.Expense);

            Assert.False(result.IsSuccess);
            Assert.Equal("La fecha inicial no puede ser posterior a la fecha final", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetByCategoryAsync_WhenUserNotAuthenticated_ReturnsFailure()
        {
            _currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);

            var result = await _reportService.GetByCategoryAsync(DateTime.Today, DateTime.Today, null);

            Assert.False(result.IsSuccess);
            Assert.Equal("Usuario no autenticado", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetByCategoryAsync_WhenRepositoryReturnsData_ReturnsMappedDtos()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 1, 31);

            var data = new List<CategoryReportItem>
            {
                new CategoryReportItem { CategoryId = 1, CategoryName = "Comida", Type = TypeTransaction.Expense, TransactionsCount = 3, TotalAmount = 120 },
                new CategoryReportItem { CategoryId = 2, CategoryName = "Salario", Type = TypeTransaction.Income, TransactionsCount = 1, TotalAmount = 1000 }
            };

            _reportRepositoryMock.Setup(r => r.GetByCategoryAsync(TestUserId, start, end, TypeTransaction.Expense))
                .ReturnsAsync(data);

            var result = await _reportService.GetByCategoryAsync(start, end, TypeTransaction.Expense);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value?.Count());
            Assert.Contains(result.Value!, x => x.CategoryName == "Comida" && x.TotalAmount == 120);
        }

        [Fact]
        public async Task GetTopCategoriesAsync_WhenStartGreaterThanEnd_ReturnsFailure()
        {
            var start = new DateTime(2025, 2, 1);
            var end = new DateTime(2025, 1, 1);

            var result = await _reportService.GetTopCategoriesAsync(start, end, 5);

            Assert.False(result.IsSuccess);
            Assert.Equal("La fecha inicial no puede ser posterior a la fecha final", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetTopCategoriesAsync_WhenTopIsInvalid_ReturnsFailure()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 1, 31);

            var result = await _reportService.GetTopCategoriesAsync(start, end, 0);

            Assert.False(result.IsSuccess);
            Assert.Equal("El límite debe ser mayor a 0", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetTopCategoriesAsync_WhenRepositoryReturnsData_ReturnsMappedDtos()
        {
            var start = new DateTime(2025, 1, 1);
            var end = new DateTime(2025, 1, 31);

            var data = new List<TopCategoryReportItem>
            {
                new TopCategoryReportItem { CategoryId = 1, CategoryName = "Comida", TotalAmount = 300 },
                new TopCategoryReportItem { CategoryId = 2, CategoryName = "Servicios", TotalAmount = 200 }
            };

            _reportRepositoryMock.Setup(r => r.GetTopCategoriesAsync(TestUserId, start, end, 2))
                .ReturnsAsync(data);

            var result = await _reportService.GetTopCategoriesAsync(start, end, 2);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value?.Count());
            Assert.Contains(result.Value!, x => x.CategoryName == "Comida" && x.TotalAmount == 300);
        }

        [Fact]
        public async Task GetComparisonAsync_WhenDatesInvalid_ReturnsFailure()
        {
            var result = await _reportService.GetComparisonAsync(
                new DateTime(2025, 2, 1),
                new DateTime(2025, 1, 1),
                new DateTime(2025, 3, 1),
                new DateTime(2025, 3, 31)
            );

            Assert.False(result.IsSuccess);
            Assert.Equal("Las fechas iniciales no pueden ser posteriores a las finales", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetComparisonAsync_WhenPeriodsOverlap_ReturnsFailure()
        {
            var result = await _reportService.GetComparisonAsync(
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 31),
                new DateTime(2025, 1, 15),
                new DateTime(2025, 2, 15)
            );

            Assert.False(result.IsSuccess);
            Assert.Equal("Los periodos a comparar no deben superponerse", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetComparisonAsync_WhenUserNotAuthenticated_ReturnsFailure()
        {
            _currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);

            var result = await _reportService.GetComparisonAsync(
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 31),
                new DateTime(2025, 2, 1),
                new DateTime(2025, 2, 28)
            );

            Assert.False(result.IsSuccess);
            Assert.Equal("Usuario no autenticado", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task GetComparisonAsync_WhenRepositoryReturnsData_ReturnsMappedDto()
        {
            var data = new ComparisonReport
            {
                Period1Start = new DateTime(2025, 1, 1),
                Period1End = new DateTime(2025, 1, 31),
                Period2Start = new DateTime(2025, 2, 1),
                Period2End = new DateTime(2025, 2, 28),
                Period1Income = 1000,
                Period1Expense = 400,
                Period1Balance = 600,
                Period2Income = 1200,
                Period2Expense = 500,
                Period2Balance = 700,
                IncomeDifference = 200,
                ExpenseDifference = 100,
                BalanceDifference = 100
            };

            _reportRepositoryMock.Setup(r => r.GetComparisonAsync(
                    TestUserId,
                    data.Period1Start,
                    data.Period1End,
                    data.Period2Start,
                    data.Period2End))
                .ReturnsAsync(data);

            var result = await _reportService.GetComparisonAsync(
                data.Period1Start,
                data.Period1End,
                data.Period2Start,
                data.Period2End
            );

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(1000, result.Value?.Period1Income);
            Assert.Equal(700, result.Value?.Period2Balance);
            Assert.Equal(200, result.Value?.IncomeDifference);
        }

        [Fact]
        public async Task ExportAsync_WhenStartAfterEnd_ReturnsFailure()
        {
            var start = new DateTime(2025, 2, 1);
            var end = new DateTime(2025, 1, 1);

            var result = await _reportService.ExportAsync(start, end, null, null, null);

            Assert.False(result.IsSuccess);
            Assert.Equal("Rango de fechas invalido", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task ExportAsync_WhenUserNotAuthenticated_ReturnsFailure()
        {
            _currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);

            var result = await _reportService.ExportAsync(null, null, null, null, null);

            Assert.False(result.IsSuccess);
            Assert.Equal("Usuario no autenticado", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task ExportAsync_WhenRepositoryReturnsData_ReturnsMappedDtos()
        {
            var data = new List<Transaction>
            {
                new Transaction
                {
                    Id = 1,
                    Amount = 150,
                    Date = new DateTime(2025, 1, 10),
                    Type = TypeTransaction.Expense,
                    Comment = "Compra",
                    UserId = TestUserId,
                    CategoryId = 2,
                    WalletId = 3
                }
            };

            _reportRepositoryMock.Setup(r => r.GetExportAsync(TestUserId, null, null, null, null, null))
                .ReturnsAsync(data);

            var result = await _reportService.ExportAsync(null, null, null, null, null);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value!);
            Assert.Equal(1, result.Value!.First().Id);
            Assert.Equal(150, result.Value!.First().Amount);
        }
    }
}
