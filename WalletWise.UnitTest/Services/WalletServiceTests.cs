using AutoMapper;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Application.Interfaces;
using WalletWise.Application.Mappings.EntityToDto;
using WalletWise.Application.Services;
using WalletWise.Domain.Common.Enums;
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

        public WalletServiceTests()
        {
            _repoMock = new Mock<IWalletRepository>();
            _loggerMock = new Mock<ILogger<Wallet>>();

            var options = new MapperConfiguration(
                c => c.AddProfile<WalletMappingProfile>(), NullLoggerFactory.Instance
                );

            var mapper = options.CreateMapper();

            _walletService = new WalletService(_repoMock.Object, _loggerMock.Object, mapper);
        }

        [Fact]
        public async Task GetAllWalletsAsync_WhenWalletsExist_ReturnsSuccessWithWallets()
        {
            // Arrange
            var wallets = new List<Wallet>()
            {
                new Wallet { Id = 1, Name = "Maximo", UserId = "1" },
                new Wallet { Id = 2, Name = "Pedro", UserId = "1" },
                new Wallet { Id = 3, Name = "Antonio", UserId = "1" }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(wallets);

            // Act
            var result = await _walletService.GetAllAsync();

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value.Count());
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetWalletByIdAsync_WhenWalletExist_ReturnsSuccessWithWallet()
        {
            // Arrange
            var wallets = new List<Wallet>()
            {
                new Wallet { Id = 1, Name = "Anuel", UserId = "1" },
                new Wallet { Id = 2, Name = "Pedra", UserId = "1"},
                new Wallet { Id = 3, Name = "Antonia", UserId = "1" }
            };

            _repoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(wallets[2]);

            // Act
            var result = await _walletService.GetByIdAsync(3);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Antonia", result.Value.Name);
        }

        [Fact]
        public async Task GetWalletByIdAsync_WhenWalletDoesnotExist_ReturnFailureWithNull()
        {
            // Arrange
            var wallets = new List<Wallet>()
            {
                new Wallet { Id = 1, Name = "Anuel", UserId = "1" },
                new Wallet { Id = 2, Name = "Pedra", UserId = "1" },
                new Wallet { Id = 3, Name = "Antonia", UserId = "1" },
                new Wallet {Id = 4, Name = "Juancito", UserId = "1"}
            };

            int id = 10;

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((default(Wallet)));

            // Act
            var result = await _walletService.GetByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal($"La entidad con el Id {id} no existe", result.Error);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task CreateWalletAsync_WhenaWalletIsCreated_ReturnSuccessWithValue()
        {
            // Arrange
            var walletDto = new CreateWalletRequestDto
            {
                Name = "Minimo Perazo"
            };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Wallet>())).ReturnsAsync((Wallet w) => w);

            // Act
            var result = await _walletService.CreateWalletAsync(walletDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Minimo Perazo", result.Value.Name);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task UpdateWalletAsync_WhenAnExistingWalletIsUpdated_ReturnSuccessWithValue()
        {
            // Arrange
            var wallets = new List<Wallet>()
            {
                new Wallet { Id = 1, Name = "Anuel", UserId = "1" },
                new Wallet { Id = 2, Name = "Pedra", UserId = "1" },
                new Wallet { Id = 3, Name = "Antonia", UserId = "1" },
                new Wallet {Id = 4, Name = "Juancito", UserId = "1"}
            };

            var walletDto = new UpdateWalletRequestDto
            {
                Name = "Maximo Supremo"
            };

            int id = 1;

            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(wallets[0]);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Wallet>())).Verifiable();

            // Act
            var result = await _walletService.UpdateWalletAsync(id, walletDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(walletDto.Name, result.Value.Name);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task DeleteWalletAsync_WhenWalletIsDeleted_ReturnSuccessWithValue()
        {
            // Arrange
            var wallets = new List<Wallet>()
            {
                new Wallet { Id = 1, Name = "Anuel", UserId = "1" },
                new Wallet { Id = 2, Name = "Pedra", UserId = "1" },
                new Wallet { Id = 3, Name = "Antonia", UserId = "1" },
                new Wallet {Id = 4, Name = "Juancito", UserId = "1"}
            };

            int id = 2;

            _repoMock.Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(wallets[1]);

            _repoMock.Setup(r => r.RemoveAsync(id)).
                Verifiable();

            // Act
            var result = await _walletService.DeleteWalletAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Error);
            Assert.True(result.Value);
        }
    }
}