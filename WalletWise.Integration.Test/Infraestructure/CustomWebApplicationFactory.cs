using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using WalletWise.Infrastructure.Context;
using WalletWise.WebApi;

namespace WalletWise.Integration.Test.Infraestructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithCreateParameterModifier(parameters =>
            {
                parameters.HostConfig.ShmSize = 1_073_741_824; 
            })
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var toRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType.FullName.Contains("DbContext") ||
                            d.ServiceType.FullName.Contains("SqlServer") ||
                            d.ServiceType.FullName.Contains("EntityFramework")))
                .ToList();

                foreach (var d in toRemove)
                    services.Remove(d);

                services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));

                services.AddDbContext<IdentityAppDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

                services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new AuthorizationPolicyBuilder(TestAuthHandler.SchemeName)
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var identityDb = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();

            await db.Database.MigrateAsync();
            await identityDb.Database.MigrateAsync();

        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Transactions.RemoveRange(db.Transactions);
            db.Categories.RemoveRange(db.Categories);
            db.Wallets.RemoveRange(db.Wallets);

            await db.SaveChangesAsync();
        }

       
    }
}

