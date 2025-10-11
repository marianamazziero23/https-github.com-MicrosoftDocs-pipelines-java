using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using ESGSustainabilityAPI.Data;
using ESGSustainabilityAPI.ViewModels;

namespace ESGSustainabilityAPI.Tests
{
    /// <summary>
    /// Testes de integração para o controller de consumo de energia
    /// </summary>
    public class EnergyConsumptionControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public EnergyConsumptionControllerTests(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("TestDatabaseEnergy");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Testa se o endpoint GET /api/energy-consumption retorna status 200
        /// </summary>
        [Fact]
        public async Task GetEnergyConsumptions_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/energy-consumption");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint GET /api/energy-consumption retorna dados válidos
        /// </summary>
        [Fact]
        public async Task GetEnergyConsumptions_ReturnsValidData()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/energy-consumption");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"success\":true", content.ToLower());
            Assert.Contains("consumos de energia recuperados com sucesso", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint GET /api/energy-consumption/{id} retorna status 200 para ID válido
        /// </summary>
        [Fact]
        public async Task GetEnergyConsumption_WithValidId_ReturnsSuccessStatusCode()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/energy-consumption/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint GET /api/energy-consumption/{id} retorna 404 para ID inválido
        /// </summary>
        [Fact]
        public async Task GetEnergyConsumption_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/energy-consumption/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint POST /api/energy-consumption cria um novo consumo com sucesso
        /// </summary>
        [Fact]
        public async Task CreateEnergyConsumption_WithValidData_ReturnsCreated()
        {
            // Arrange
            await SeedTestData();
            
            var newConsumption = new CreateEnergyConsumptionViewModel
            {
                EnergyType = "Solar",
                ConsumptionAmount = 1500.0m,
                Unit = "kWh",
                RecordDate = DateTime.UtcNow,
                Source = "Painéis Solares",
                Cost = 750.0m,
                CostCurrency = "BRL",
                RenewablePercentage = 100.0m,
                Description = "Consumo de energia solar",
                CompanyId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/energy-consumption", newConsumption);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"success\":true", content.ToLower());
            Assert.Contains("consumo de energia criado com sucesso", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint POST /api/energy-consumption retorna erro para dados inválidos
        /// </summary>
        [Fact]
        public async Task CreateEnergyConsumption_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidConsumption = new CreateEnergyConsumptionViewModel
            {
                // EnergyType é obrigatório, mas não está sendo fornecido
                ConsumptionAmount = -100.0m, // Valor negativo inválido
                Unit = "",
                RecordDate = DateTime.UtcNow,
                CompanyId = 999 // Empresa inexistente
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/energy-consumption", invalidConsumption);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Testa filtros de busca no endpoint GET /api/energy-consumption
        /// </summary>
        [Fact]
        public async Task GetEnergyConsumptions_WithSearchFilter_ReturnsFilteredResults()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/energy-consumption?searchTerm=Elétrica");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("elétrica", content.ToLower());
        }

        /// <summary>
        /// Testa endpoint de estatísticas de energia
        /// </summary>
        [Fact]
        public async Task GetEnergyStatistics_ReturnsSuccessStatusCode()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/energy-consumption/statistics");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"success\":true", content.ToLower());
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
                Name = "Empresa Teste Energia",
                CNPJ = "22.222.222/0001-22",
                Industry = "Energia",
                Address = "Rua Energia, 456",
                City = "Rio de Janeiro",
                State = "RJ",
                ZipCode = "20000-000",
                ContactEmail = "energia@teste.com",
                ContactPhone = "(21) 2222-2222",
                EmployeeCount = 200,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Companies.Add(company);

            // Adicionar consumo de energia de teste
            var consumption = new ESGSustainabilityAPI.Models.EnergyConsumption
            {
                Id = 1,
                EnergyType = "Elétrica",
                ConsumptionAmount = 3000.0m,
                Unit = "kWh",
                RecordDate = DateTime.UtcNow.AddDays(-5),
                Source = "Rede Elétrica",
                Cost = 1500.0m,
                CostCurrency = "BRL",
                RenewablePercentage = 40.0m,
                Description = "Consumo mensal de teste",
                CompanyId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.EnergyConsumptions.Add(consumption);
            await context.SaveChangesAsync();
        }
    }
}

