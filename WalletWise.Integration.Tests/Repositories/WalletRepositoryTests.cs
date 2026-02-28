using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletWise.Persistence.Context;

namespace WalletWise.Integration.Tests.Repositories
{
    public class WalletRepositoryTests
    {

        private AppDbContext getConnection()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
                
        }


        // Arrange
        // Act
        // Assert
    }
}
