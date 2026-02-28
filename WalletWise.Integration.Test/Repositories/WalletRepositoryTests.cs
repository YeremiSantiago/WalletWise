using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Domain.Entities;
using WalletWise.Domain.Interfaces;
using WalletWise.Persistence.Context;
using WalletWise.Persistence.Repositories;

namespace WalletWise.Integration.Test.Repositories
{
    public class WalletRepositoryTests
    {
        private readonly IWalletRepository _walletRepository;
        private readonly AppDbContext _context;

        public WalletRepositoryTests()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _walletRepository = new WalletRepository(_context);
        }

        [Fact]
        public async Task GetWalletByIdAsync_WhenAWalletExist_ReturnWalletExisting()
        {
            // Arrange 
            var Wallets = new List<Wallet>()
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

            int id = 1;

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();


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

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();

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
                UserId = 1
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

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();

            int id = 1;

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

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();

            int id = 2;

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

            await _context.Wallets.AddRangeAsync(Wallets);

            await _context.SaveChangesAsync();


            // Act

            var result = await _walletRepository.ExistsAsync(x => x.Name == "Trabajo");

            // Assert

            Assert.True(result);

        }


    }    
}
