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
    /// Testes de integração para o controller de relatórios de sustentabilidade
    /// </summary>
    public class SustainabilityReportsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public SustainabilityReportsControllerTests(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("TestDatabaseReports");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Testa se o endpoint GET /api/sustainability-reports retorna status 200
        /// </summary>
        [Fact]
        public async Task GetSustainabilityReports_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/sustainability-reports");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint GET /api/sustainability-reports retorna dados válidos
        /// </summary>
        [Fact]
        public async Task GetSustainabilityReports_ReturnsValidData()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/sustainability-reports");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"success\":true", content.ToLower());
            Assert.Contains("relatórios de sustentabilidade recuperados com sucesso", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint GET /api/sustainability-reports/{id} retorna status 200 para ID válido
        /// </summary>
        [Fact]
        public async Task GetSustainabilityReport_WithValidId_ReturnsSuccessStatusCode()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/sustainability-reports/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint GET /api/sustainability-reports/{id} retorna 404 para ID inválido
        /// </summary>
        [Fact]
        public async Task GetSustainabilityReport_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/sustainability-reports/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint POST /api/sustainability-reports cria um novo relatório com sucesso
        /// </summary>
        [Fact]
        public async Task CreateSustainabilityReport_WithValidData_ReturnsCreated()
        {
            // Arrange
            await SeedTestData();
            
            var newReport = new CreateSustainabilityReportViewModel
            {
                Title = "Relatório ESG Q2 2024",
                Year = 2024,
                Quarter = 2,
                TotalCarbonEmissions = 45.5m,
                TotalEnergyConsumption = 8500.0m,
                RenewableEnergyPercentage = 60.0m,
                WaterConsumption = 1200.0m,
                WasteGenerated = 500.0m,
                WasteRecycled = 350.0m,
                ESGScore = "B",
                EnvironmentalInitiatives = "Implementação de painéis solares",
                SocialInitiatives = "Programa de diversidade e inclusão",
                GovernanceInitiatives = "Comitê de ética e compliance",
                Challenges = "Redução de emissões de carbono",
                FutureGoals = "Neutralidade de carbono até 2030",
                CompanyId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/sustainability-reports", newReport);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"success\":true", content.ToLower());
            Assert.Contains("relatório de sustentabilidade criado com sucesso", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint POST /api/sustainability-reports retorna erro para dados inválidos
        /// </summary>
        [Fact]
        public async Task CreateSustainabilityReport_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidReport = new CreateSustainabilityReportViewModel
            {
                // Title é obrigatório, mas não está sendo fornecido
                Year = 0, // Ano inválido
                Quarter = 5, // Trimestre inválido (deve ser 1-4)
                CompanyId = 999 // Empresa inexistente
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/sustainability-reports", invalidReport);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Testa filtros de busca no endpoint GET /api/sustainability-reports
        /// </summary>
        [Fact]
        public async Task GetSustainabilityReports_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/sustainability-reports?year=2024&quarter=1");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("2024", content);
        }

        /// <summary>
        /// Testa endpoint de geração automática de relatório
        /// </summary>
        [Fact]
        public async Task GenerateAutomaticReport_WithValidData_ReturnsCreated()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.PostAsync("/api/sustainability-reports/generate?companyId=1&year=2024&quarter=3", null);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"success\":true", content.ToLower());
            Assert.Contains("relatório de sustentabilidade gerado automaticamente com sucesso", content.ToLower());
        }

        /// <summary>
        /// Testa geração automática com empresa inexistente
        /// </summary>
        [Fact]
        public async Task GenerateAutomaticReport_WithInvalidCompany_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync("/api/sustainability-reports/generate?companyId=999&year=2024&quarter=1", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("empresa não encontrada", content.ToLower());
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
                Name = "Empresa Teste Sustentabilidade",
                CNPJ = "33.333.333/0001-33",
                Industry = "Sustentabilidade",
                Address = "Rua Verde, 789",
                City = "Brasília",
                State = "DF",
                ZipCode = "70000-000",
                ContactEmail = "sustentabilidade@teste.com",
                ContactPhone = "(61) 3333-3333",
                EmployeeCount = 300,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Companies.Add(company);

            // Adicionar relatório de teste
            var report = new ESGSustainabilityAPI.Models.SustainabilityReport
            {
                Id = 1,
                Title = "Relatório ESG Q1 2024 - Teste",
                Year = 2024,
                Quarter = 1,
                TotalCarbonEmissions = 30.0m,
                TotalEnergyConsumption = 5000.0m,
                RenewableEnergyPercentage = 45.0m,
                WaterConsumption = 800.0m,
                WasteGenerated = 300.0m,
                WasteRecycled = 200.0m,
                ESGScore = "B",
                EnvironmentalInitiatives = "Iniciativas ambientais de teste",
                SocialInitiatives = "Iniciativas sociais de teste",
                GovernanceInitiatives = "Iniciativas de governança de teste",
                Challenges = "Desafios de teste",
                FutureGoals = "Metas futuras de teste",
                CompanyId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.SustainabilityReports.Add(report);

            // Adicionar dados para geração automática
            var emission = new ESGSustainabilityAPI.Models.CarbonEmission
            {
                Id = 1,
                Source = "Energia Teste",
                EmissionAmount = 15.0m,
                Unit = "tCO2e",
                RecordDate = new DateTime(2024, 7, 15),
                Category = "Escopo 2",
                Location = "Brasília - DF",
                Description = "Emissão para teste de geração automática",
                CompanyId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.CarbonEmissions.Add(emission);

            var consumption = new ESGSustainabilityAPI.Models.EnergyConsumption
            {
                Id = 1,
                EnergyType = "Elétrica",
                ConsumptionAmount = 2000.0m,
                Unit = "kWh",
                RecordDate = new DateTime(2024, 7, 15),
                Source = "Rede Elétrica",
                Cost = 1000.0m,
                CostCurrency = "BRL",
                RenewablePercentage = 50.0m,
                Description = "Consumo para teste de geração automática",
                CompanyId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.EnergyConsumptions.Add(consumption);
            await context.SaveChangesAsync();
        }
    }
}

