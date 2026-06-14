using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using WalletWise.Application.Dtos.Auth;
using WalletWise.Integration.Test.Infraestructure;
using WalletWise.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace WalletWise.Integration.Test.Controllers
{
    public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        public async Task Register_WhenValid_ReturnsCreated()
        {
            var request = CreateRegisterRequest();

            var response = await _client.PostAsJsonAsync("api/auth/register", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            Assert.NotNull(result);
            Assert.Equal(result.Email, request.Email);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Fact]
        public async Task Register_WhenEmailAlreadyExists_ReturnsBadRequest()
        {
            var request = CreateRegisterRequest();

            await RegisterUserAsync(request);

            var response = await _client.PostAsJsonAsync("api/auth/register", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WhenValid_ReturnsOk()
        {
            var request = CreateRegisterRequest();

            await RegisterUserAsync(request);

            var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequestDto
            {
                Email = request.Email,
                Password = request.Password
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            Assert.NotNull(result);
            Assert.Equal(result.Email, request.Email);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Fact]
        public async Task Login_WhenInvalid_ReturnsUnauthorized()
        {
            var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequestDto
            {
                Email = $"invalid-{Guid.NewGuid()}@test.com",
                Password = "Passw0rd1"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Logout_WhenUserExists_ReturnsNoContent()
        {
            await SeedTestUserAsync();

            var response = await _client.PostAsync("api/auth/logout", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WhenValid_ReturnsNoContent()
        {
            await SeedTestUserAsync();

            var response = await _client.PutAsJsonAsync("api/auth/me/password", new ChangePasswordRequestDto
            {
                CurrentPassword = "Passw0rd1",
                NewPassword = "NewPassw0rd2"
            });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private static RegisterRequestDto CreateRegisterRequest()
        {
            return new RegisterRequestDto
            {
                Name = "Test User",
                Email = $"user-{Guid.NewGuid()}@test.com",
                Password = "Passw0rd1",
                ConfirmPassword = "Passw0rd1"
            };
        }

        private async Task<LoginResponseDto> RegisterUserAsync(RegisterRequestDto request)
        {
            var response = await _client.PostAsJsonAsync("api/auth/register", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            Assert.NotNull(result);

            return result;
        }

        private async Task SeedTestUserAsync()
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

            var result = await userManager.CreateAsync(user, "Passw0rd1");

            Assert.True(result.Succeeded);
        }

        private async Task ResetIdentityAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();

            
            try
            {
                await db.Database.EnsureCreatedAsync();
               
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

            // Generame una imprimir

        }
    }
}