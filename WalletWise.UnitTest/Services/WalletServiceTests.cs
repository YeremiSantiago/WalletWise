using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Application.Interfaces;
using WalletWise.Application.Mappings.EntityToDto;
using WalletWise.Application.Services;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using Xunit;

namespace WalletWise.Unit.Tests.Services
{
    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _repoMock;
        private readonly WalletService _walletService;
        private readonly Mock<ILogger<Wallet>> _loggerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;

        private const string TestUserId = "1";
                                                                                    
        public WalletServiceTests()
        {
            _repoMock = new Mock<IWalletRepository>();
            _loggerMock = new Mock<ILogger<Wallet>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _currentUserServiceMock.Setup(x => x.UserId).Returns(TestUserId);

            var options = new MapperConfiguration(
                c => c.AddProfile<WalletMappingProfile>(), NullLoggerFactory.Instance
                );

            var mapper = options.CreateMapper();

            _walletService = new WalletService(_repoMock.Object, _loggerMock.Object, mapper, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetAllWalletsAsync_WhenWalletsExist_ReturnsSuccessWithWallets()
        {
            // Arrange
            var wallets = new List<Wallet>()
            {
                new Wallet { Id = 1, Name = "Maximo", UserId = TestUserId },
                new Wallet { Id = 2, Name = "Pedro", UserId = TestUserId },
                new Wallet { Id = 3, Name = "Antonio", UserId = TestUserId }
            };

            _repoMock.Setup(r => r.GetAllByUserAsync(TestUserId)).ReturnsAsync(wallets);

            // Act
            var result = await _walletService.GetAllWalletsAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value?.Count());
        }

        [Fact]
        public async Task GetWalletByIdAsync_WhenWalletExist_ReturnsSuccessWithWallet()
        {
            // Arrange
            var wallet = new Wallet { Id = 3, Name = "Antonia", UserId = TestUserId };
            int id = 3;

            _repoMock.Setup(r => r.GetByIdForUserAsync(id, TestUserId)).ReturnsAsync(wallet);

            // Act
            var result = await _walletService.GetWalletByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal("Antonia", result.Value?.Name);
        }

        [Fact]
        public async Task GetWalletByIdAsync_WhenWalletDoesnotExist_ReturnFailureWithNull()
        {
            // Arrange
            int id = 10;
            _repoMock.Setup(r => r.GetByIdForUserAsync(id, TestUserId)).ReturnsAsync((Wallet?)null);

            // Act
            var result = await _walletService.GetWalletByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.Null(result.Value);
            Assert.Equal($"La category con el id {id} no existe", result.Error);
        }

        [Fact]
        public async Task CreateWalletAsync_WhenaWalletIsCreated_ReturnSuccessWithValue()
        {
            // Arrange
            var walletDto = new CreateWalletRequestDto { Name = "Minimo Perazo" };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Wallet>())).ReturnsAsync((Wallet w) => w);

            // Act
            var result = await _walletService.CreateWalletAsync(walletDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal("Minimo Perazo", result.Value?.Name);
        }

        [Fact]
        public async Task UpdateWalletAsync_WhenAnExistingWalletIsUpdated_ReturnSuccessWithValue()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, Name = "Anuel", UserId = TestUserId };
            var walletDto = new UpdateWalletRequestDto { Name = "Maximo Supremo" };
            int id = 1;

            _repoMock.Setup(r => r.GetByIdForUserAsync(id, TestUserId)).ReturnsAsync(wallet);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Wallet>())).Verifiable();

            // Act
            var result = await _walletService.UpdateWalletAsync(id, walletDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.NotNull(result.Value);
            Assert.Equal(walletDto.Name, result.Value?.Name);
        }

        [Fact]
        public async Task DeleteWalletAsync_WhenWalletIsDeleted_ReturnSuccessWithValue()
        {
            // Arrange
            var wallet = new Wallet { Id = 2, Name = "Pedra", UserId = TestUserId };
            int id = 2;

            _repoMock.Setup(r => r.GetByIdForUserAsync(id, TestUserId)).ReturnsAsync(wallet);
            _repoMock.Setup(r => r.RemoveAsync(id)).Verifiable();

            // Act
            var result = await _walletService.DeleteWalletAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.True(result.Value);
        }
    }
}