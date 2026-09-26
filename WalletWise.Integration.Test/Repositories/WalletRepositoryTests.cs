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
using WalletWise.Integration.Test.Infraestructure;

namespace WalletWise.Integration.Test.Repositories
{
    [Collection("Database")]
    public class WalletRepositoryTests : IAsyncLifetime
    {
        private IWalletRepository _walletRepository;
        private AppDbContext _context;
        private readonly DatabaseFixture _fixture;


        public WalletRepositoryTests(DatabaseFixture fixture)
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
            _walletRepository = new WalletRepository(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task GetWalletByIdAsync_WhenAWalletExist_ReturnWalletExisting()
        {
            // Arrange 
            var Wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "Trabajo",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta Credito",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta De Debito",
                    UserId = "1"
                }
            };

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();

            int id = Wallets[0].Id;

            // Act 

            var result = await _walletRepository.GetByIdAsync(id);

            // Assert

            Assert.Equal("Trabajo", result.Name);

        }

        [Fact]

        public async Task GetAllWallets_WhenWalletsExisting_ReturnAllWalletExistng()
        {
            // Arrange
            var Wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "Trabajo",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta Credito",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta De Debito",
                    UserId = "1"
                }
            };

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();

            int id = Wallets[1].Id;

            // Act 

            var Wallet = await _walletRepository.GetByIdAsync(id);

            // Assert

            Assert.NotNull(Wallet);
            Assert.Equal(Wallets[1].Name, Wallet.Name);

        }


        [Fact]
        public async Task AddWalletAsync_WhenCreatedAWallet_ReturnWallet()
        {
            // Arrange 

            var Wallet = new Wallet
            {
                Name = "Maximo",
                UserId = "1"
            };

            // Act

            var result = await _walletRepository.AddAsync(Wallet);

            // Assert

            Assert.NotNull(result);
            Assert.Equal(result.Name, Wallet.Name);

        }


        [Fact]
        public async Task UpdateWallet_WhenWalletIsUpdated_ShouldUpdatedCorrectly()
        {
            // Arrange

            var Wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "Trabajo",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta Credito",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta De Debito",
                    UserId = "1"
                }
            };

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();

            int id = Wallets[0].Id;

           var walletTrack = await _context.Wallets.FindAsync(id);

            walletTrack.Name = "Trabajoooo";
            

            // Act

            await _walletRepository.UpdateAsync(walletTrack);

            // Assert

            var Wallet = await _context.Wallets.FirstOrDefaultAsync(x => x.Id == id);

            Assert.NotNull(Wallet);
            Assert.Equal(Wallet.Name, walletTrack.Name);

        }

        [Fact]
        public async Task RemoveWalletAsync_WhenWalletIsDeleted_ShouldDeleteProperly()
        {
            // Arrange 

            var Wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "Trabajo",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta Credito",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta De Debito",
                    UserId = "1"
                }
            };

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();

            int id = Wallets[1].Id;

            // Act 

            await _walletRepository.RemoveAsync(id);

            // Assert

           bool exist = await _context.Wallets.AnyAsync(x => x.Name == "Tarjeta Credito");

            Assert.False(exist);

        }

        [Fact]
        public async Task ExistsAsync_WhenWalletExistsUnderCondition_ReturnReturnsTrueOrFalse()
        {
            // Arrange 

            var Wallets = new List<Wallet>()
            {
                new Wallet()
                {
                    Name = "Trabajo",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta Credito",
                    UserId = "1"
                },
                new Wallet()
                {
                    Name = "Tarjeta De Debito",
                    UserId = "1"
                }
            };

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();


            // Act

            var result = await _walletRepository.ExistsAsync(x => x.Name == "Trabajo");

            // Assert

            Assert.True(result);

        }


    }    
}

