using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using ESGSustainabilityAPI.Data;
using ESGSustainabilityAPI.ViewModels;

namespace ESGSustainabilityAPI.Tests
{
    /// <summary>
    /// Testes de integração para o controller de emissões de carbono
    /// </summary>
    public class EmissionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public EmissionsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remover o DbContext existente
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ESGDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Adicionar DbContext em memória para testes
                    services.AddDbContext<ESGDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Testa se o endpoint GET /api/emissions retorna status 200
        /// </summary>
        [Fact]
        public async Task GetEmissions_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/emissions");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint GET /api/emissions retorna dados válidos
        /// </summary>
        [Fact]
        public async Task GetEmissions_ReturnsValidData()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/emissions");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"success\":true", content.ToLower());
            Assert.Contains("emissões de carbono recuperadas com sucesso", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint GET /api/emissions com paginação funciona corretamente
        /// </summary>
        [Fact]
        public async Task GetEmissions_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/emissions?page=1&pageSize=5");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"currentPage\":1", content.ToLower());
            Assert.Contains("\"pageSize\":5", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint GET /api/emissions/{id} retorna status 200 para ID válido
        /// </summary>
        [Fact]
        public async Task GetEmission_WithValidId_ReturnsSuccessStatusCode()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/emissions/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint GET /api/emissions/{id} retorna 404 para ID inválido
        /// </summary>
        [Fact]
        public async Task GetEmission_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/emissions/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint POST /api/emissions cria uma nova emissão com sucesso
        /// </summary>
        [Fact]
        public async Task CreateEmission_WithValidData_ReturnsCreated()
        {
            // Arrange
            await SeedTestData();
            
            var newEmission = new CreateCarbonEmissionViewModel
            {
                Source = "Transporte",
                EmissionAmount = 25.0m,
                Unit = "tCO2e",
                RecordDate = DateTime.UtcNow,
                Category = "Escopo 1",
                Location = "Rio de Janeiro - RJ",
                Description = "Emissões do transporte corporativo",
                CompanyId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/emissions", newEmission);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"success\":true", content.ToLower());
            Assert.Contains("emissão de carbono criada com sucesso", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint POST /api/emissions retorna erro para dados inválidos
        /// </summary>
        [Fact]
        public async Task CreateEmission_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidEmission = new CreateCarbonEmissionViewModel
            {
                // Source é obrigatório, mas não está sendo fornecido
                EmissionAmount = -10.0m, // Valor negativo inválido
                Unit = "",
                RecordDate = DateTime.UtcNow,
                CompanyId = 999 // Empresa inexistente
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/emissions", invalidEmission);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint POST /api/emissions retorna erro para empresa inexistente
        /// </summary>
        [Fact]
        public async Task CreateEmission_WithNonExistentCompany_ReturnsBadRequest()
        {
            // Arrange
            var emission = new CreateCarbonEmissionViewModel
            {
                Source = "Teste",
                EmissionAmount = 10.0m,
                Unit = "tCO2e",
                RecordDate = DateTime.UtcNow,
                Category = "Escopo 1",
                Location = "Teste",
                Description = "Teste",
                CompanyId = 999 // Empresa inexistente
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/emissions", emission);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("empresa não encontrada", content.ToLower());
        }

        /// <summary>
        /// Testa filtros de busca no endpoint GET /api/emissions
        /// </summary>
        [Fact]
        public async Task GetEmissions_WithSearchFilter_ReturnsFilteredResults()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/emissions?searchTerm=Energia");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("energia", content.ToLower());
        }

        /// <summary>
        /// Testa ordenação no endpoint GET /api/emissions
        /// </summary>
        [Fact]
        public async Task GetEmissions_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/emissions?sortBy=amount&sortDirection=desc");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Método auxiliar para popular dados de teste
        /// </summary>
        private async Task SeedTestData()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ESGDbContext>();
            
            // Limpar dados existentes
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Adicionar empresa de teste
            var company = new ESGSustainabilityAPI.Models.Company
            {
                Id = 1,
                Name = "Empresa Teste",
                CNPJ = "11.111.111/0001-11",
                Industry = "Teste",
                Address = "Rua Teste, 123",
                City = "São Paulo",
                State = "SP",
                ZipCode = "01000-000",
                ContactEmail = "teste@teste.com",
                ContactPhone = "(11) 1111-1111",
                EmployeeCount = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Companies.Add(company);

            // Adicionar emissão de teste
            var emission = new ESGSustainabilityAPI.Models.CarbonEmission
            {
                Id = 1,
                Source = "Energia Elétrica Teste",
                EmissionAmount = 20.0m,
                Unit = "tCO2e",
                RecordDate = DateTime.UtcNow.AddDays(-10),
                Category = "Escopo 2",
                Location = "São Paulo - SP",
                Description = "Emissão de teste",
                CompanyId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.CarbonEmissions.Add(emission);
            await context.SaveChangesAsync();
        }
    }
}

