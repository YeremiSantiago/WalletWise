using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WalletWise.Application.Dtos.Category;
using WalletWise.Integration.Test.Infraestructure;

namespace WalletWise.Integration.Test.Controllers
{
    public class CategoryControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public CategoryControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public Task InitializeAsync()
        {
            return _factory.ResetDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]

        public async Task GetCategoryById_WhenACategoryExisting_ReturnCategoryFound()
        {
            // Arrange
            var created = await CreateCategory("Transporte");

            // Act
            var request = await _client.GetAsync($"api/categories/{created.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);

            var result = await request.Content.ReadFromJsonAsync<CategoryResponseDto>();

            Assert.NotNull(result);
            Assert.Equal(result.Name, "Transporte");
        }

        [Fact]
        public async Task GetAllCategories_WhenCategoriesExist_ReturnAllCategories()
        {
            // Arrange 
            await CreateCategory("Navidad");
            await CreateCategory("Comunicaciones");
            await CreateCategory("Transporte");

            // Act 
            var response = await _client.GetAsync("api/categories");

            // Assert
            Assert.Equal(response.StatusCode, HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryResponseDto>>();

            Assert.Equal(result.Count(), 3);
            Assert.True(result.Any());
        }

        [Fact]
        public async Task CreateCategory_WhenDataIsValid_CreatesCategorySuccessfully()
        {
            var request = await _client.PostAsJsonAsync("api/categories", new CreateCategoryRequestDto { Name = "Electricidad" });

            Assert.Equal(HttpStatusCode.Created, request.StatusCode);

            var result = await request.Content.ReadFromJsonAsync<CategoryResponseDto>();

            Assert.Equal(result.Name, "Electricidad");
        }

        [Fact]
        public async Task UpdateCategory_WhenCategoryExisting_UpdatesCategorySuccessfully()
        {
            // Arrange
            var category = await CreateCategory("Comídaaaaaaaaaaaaaaaaaaaaaaa");

            var update = new UpdateCategoryRequestDto
            {
                Name = "Comida"
            };

            // Act
            var request = await _client.PutAsJsonAsync($"api/categories/{category.Id}", update);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, request.StatusCode);

            var result = await request.Content.ReadFromJsonAsync<CategoryResponseDto>();

            Assert.Equal(result.Name, "Comida");
            Assert.NotNull(result);
            Assert.Equal(result.Id, category.Id);
        }

        [Fact]
        public async Task DeleteCategory_WhenCategoryExisting_DeletedCategorySuccessfully()
        {
            // Arrange
            var category = await CreateCategory("Casa");

            // Act 
            var request = await _client.DeleteAsync($"api/categories/{category.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, request.StatusCode);

        }


        private async Task<CategoryResponseDto> CreateCategory(string nombre)
        {

            var data = new CreateCategoryRequestDto { Name = nombre };

            var serielize = JsonSerializer.Serialize(data);

            var request = await _client.PostAsJsonAsync("api/categories", data);

            var response = await request.Content.ReadFromJsonAsync<CategoryResponseDto>();

            return response;
        }

    }
}
