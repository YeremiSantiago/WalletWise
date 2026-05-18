using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;
using WalletWise.Application.Dtos.Wallet;
using WalletWise.Integration.Test.Infraestructure;

namespace WalletWise.Integration.Test.Controllers
{
    public class WalletsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public WalletsControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }


        [Fact]
        public async Task GetWalletById_WhenAWalletExisting_ReturnWalletFound()
        {
            // Arrange
            var request = await CreateWalletAsync("Inversiones");

            // Act
            var response = await _client.GetAsync($"api/wallets/{request.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<WalletResponseDto>();

            Assert.NotNull(result);
            Assert.Equal(result.Name, "Inversiones");

        }


        [Fact]
        public async Task CreateWallet_WhenValid_ReturnsCreateWallet()
        {
            var request = new CreateWalletRequestDto { Name = "Trabajo" };

            var response = await _client.PostAsJsonAsync("api/wallets", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<WalletResponseDto>();

            Assert.NotNull(created);
            Assert.Equal("Trabajo", created.Name);
            Assert.Equal("test-user-id", created.UserId);
        }

        [Fact]
        public async Task UpdateWallet_WhenExists_ReturnsUpdatedWallet()
        {
            var created = await CreateWalletAsync("Inicial");

            var update = new UpdateWalletRequestDto { Name = "Actualizada" };

            var response = await _client.PutAsJsonAsync($"api/wallets/{created.Id}", update);

            response.EnsureSuccessStatusCode();

            var updated = await response.Content.ReadFromJsonAsync<WalletResponseDto>();

            Assert.NotNull(updated);
            Assert.Equal("Actualizada", updated.Name);
        }

        [Fact]
        public async Task DeleteWallet_WhenExists_ReturnsNoContent()
        {
            var created = await CreateWalletAsync("Existente");

            var response = await _client.DeleteAsync($"api/wallets/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"api/wallets/{created.Id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        private async Task<WalletResponseDto> CreateWalletAsync(string name)
        {
            var response = await _client.PostAsJsonAsync("api/wallets", new CreateWalletRequestDto { Name = name });

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<WalletResponseDto>();

            Assert.NotNull(created);

            return created;
        }

    }
}
