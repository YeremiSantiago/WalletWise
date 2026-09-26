using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using WalletWise.Infrastructure.Context;

namespace WalletWise.Integration.Test.Infraestructure
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithCreateParameterModifier(parameters =>
            {
                parameters.HostConfig.ShmSize = 1_073_741_824; 
            })
            .Build();

        public string ConnectionString => _dbContainer.GetConnectionString();

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }

        public AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            return new AppDbContext(options);
        }
    }
}

