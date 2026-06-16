using System;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using WalletWise.Application.Dtos.Auth;
using WalletWise.Application.Dtos.Users;
using WalletWise.Integration.Test.Infraestructure;
using WalletWise.Infrastructure.Context;

namespace WalletWise.Integration.Test.Controllers
{
    public class UsersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public UsersControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
            await ResetIdentityAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetCurrentUserProfile_WhenUserExists_ReturnsProfile()
        {
            await SeedTestUserAsync("Perfil Test");

            var response = await _client.GetAsync("api/users/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<UserProfileResponseDto>();

            Assert.NotNull(result);
            Assert.Equal(result.Id, "test-user-id");
            Assert.Equal(result.Name, "Perfil Test");
        }

        [Fact]
        public async Task GetCurrentUserProfile_WhenUserMissing_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/users/me");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProfile_WhenValid_ReturnsUpdatedProfile()
        {
            await SeedTestUserAsync("Nombre Antiguo");

            var response = await _client.PutAsJsonAsync("api/users/me", new UpdateUserProfileRequestDto
            {
                Name = "Nombre Nuevo"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<UserProfileResponseDto>();

            Assert.NotNull(result);
            Assert.Equal(result.Name, "Nombre Nuevo");
        }

        [Fact]
        public async Task ChangePassword_WhenValid_ReturnsNoContent()
        {
            await SeedTestUserAsync("Perfil Test");

            var response = await _client.PutAsJsonAsync("api/users/me/password", new ChangePasswordRequestDto
            {
                CurrentPassword = "Passw0rd1",
                NewPassword = "NewPassw0rd2"
            });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WhenInvalid_ReturnsBadRequest()
        {
            await SeedTestUserAsync("Perfil Test");

            var response = await _client.PutAsJsonAsync("api/users/me/password", new ChangePasswordRequestDto
            {
                CurrentPassword = "WrongPass1",
                NewPassword = "NewPassw0rd2"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private async Task SeedTestUserAsync(string name)
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var existing = await userManager.FindByIdAsync("test-user-id");
            if (existing != null)
            {
                return;
            }

            var user = new IdentityUser
            {
                Id = "test-user-id",
                Email = "test-user@walletwise.local",
                UserName = "test-user@walletwise.local"
            };

            var createResult = await userManager.CreateAsync(user, "Passw0rd1");

            Assert.True(createResult.Succeeded);

            var claims = new[]
            {
                new Claim("profile_name", name),
                new Claim("registered_at", DateTime.UtcNow.ToString("O"))
            };

            var claimResult = await userManager.AddClaimsAsync(user, claims);

            Assert.True(claimResult.Succeeded);
        }

        private async Task ResetIdentityAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();

           
            try
            {
                var databaseCreator = db.GetService<IRelationalDatabaseCreator>();
                await databaseCreator.CreateTablesAsync();
            }
            catch
            {
                
            }

            db.UserClaims.RemoveRange(db.UserClaims);
            db.UserLogins.RemoveRange(db.UserLogins);
            db.UserRoles.RemoveRange(db.UserRoles);
            db.UserTokens.RemoveRange(db.UserTokens);
            db.RoleClaims.RemoveRange(db.RoleClaims);
            db.Roles.RemoveRange(db.Roles);
            db.Users.RemoveRange(db.Users);

            await db.SaveChangesAsync();
        }
    }
}
