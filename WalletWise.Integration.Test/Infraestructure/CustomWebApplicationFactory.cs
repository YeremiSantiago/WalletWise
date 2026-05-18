using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using WalletWise.Persistence.Context;
using WalletWise.WebApi;

namespace WalletWise.Integration.Test.Infraestructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private SqliteConnection _connection = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection = CreateOpenConnection();

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
                    options.UseSqlite(_connection));

                services.AddDbContext<IdentityAppDbContext>(options =>
                options.UseSqlite(_connection));

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
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        public new async Task DisposeAsync()
        {
            await _connection.DisposeAsync();
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

        private static SqliteConnection CreateOpenConnection()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            return connection;
        }
    }
}
