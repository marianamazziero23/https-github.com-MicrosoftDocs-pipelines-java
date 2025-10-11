using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;
using ESGSustainabilityAPI.Data;

namespace ESGSustainabilityAPI.Tests
{
    /// <summary>
    /// Testes de integração para o controller de dashboard ESG
    /// </summary>
    public class ESGDashboardControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ESGDashboardControllerTests(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("TestDatabaseDashboard");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        /// <summary>
        /// Testa se o endpoint GET /api/esg-dashboard/statistics retorna status 200
        /// </summary>
        [Fact]
        public async Task GetESGStatistics_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/statistics");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa se o endpoint GET /api/esg-dashboard/statistics retorna dados válidos
        /// </summary>
        [Fact]
        public async Task GetESGStatistics_ReturnsValidData()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/statistics");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"success\":true", content.ToLower());
            Assert.Contains("estatísticas esg calculadas com sucesso", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint GET /api/esg-dashboard/emissions-trends retorna status 200
        /// </summary>
        [Fact]
        public async Task GetEmissionsTrends_ReturnsSuccessStatusCode()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/emissions-trends");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa tendências de emissões com diferentes períodos
        /// </summary>
        [Fact]
        public async Task GetEmissionsTrends_WithDifferentPeriods_ReturnsValidData()
        {
            // Arrange
            await SeedTestData();

            // Act & Assert - Mensal
            var monthlyResponse = await _client.GetAsync("/api/esg-dashboard/emissions-trends?period=month");
            Assert.Equal(HttpStatusCode.OK, monthlyResponse.StatusCode);

            // Act & Assert - Trimestral
            var quarterlyResponse = await _client.GetAsync("/api/esg-dashboard/emissions-trends?period=quarter");
            Assert.Equal(HttpStatusCode.OK, quarterlyResponse.StatusCode);

            // Act & Assert - Anual
            var yearlyResponse = await _client.GetAsync("/api/esg-dashboard/emissions-trends?period=year");
            Assert.Equal(HttpStatusCode.OK, yearlyResponse.StatusCode);
        }

        /// <summary>
        /// Testa tendências de emissões com período inválido
        /// </summary>
        [Fact]
        public async Task GetEmissionsTrends_WithInvalidPeriod_ReturnsBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/emissions-trends?period=invalid");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("período inválido", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint GET /api/esg-dashboard/company-ranking retorna status 200
        /// </summary>
        [Fact]
        public async Task GetCompanyRanking_ReturnsSuccessStatusCode()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/company-ranking");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa ranking de empresas com diferentes métricas
        /// </summary>
        [Fact]
        public async Task GetCompanyRanking_WithDifferentMetrics_ReturnsValidData()
        {
            // Arrange
            await SeedTestData();

            // Act & Assert - Emissões
            var emissionsResponse = await _client.GetAsync("/api/esg-dashboard/company-ranking?metric=emissions");
            Assert.Equal(HttpStatusCode.OK, emissionsResponse.StatusCode);

            // Act & Assert - Energia
            var energyResponse = await _client.GetAsync("/api/esg-dashboard/company-ranking?metric=energy");
            Assert.Equal(HttpStatusCode.OK, energyResponse.StatusCode);

            // Act & Assert - Score ESG
            var esgResponse = await _client.GetAsync("/api/esg-dashboard/company-ranking?metric=esg_score");
            Assert.Equal(HttpStatusCode.OK, esgResponse.StatusCode);
        }

        /// <summary>
        /// Testa ranking com métrica inválida
        /// </summary>
        [Fact]
        public async Task GetCompanyRanking_WithInvalidMetric_ReturnsBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/company-ranking?metric=invalid");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("métrica inválida", content.ToLower());
        }

        /// <summary>
        /// Testa ranking com limite inválido
        /// </summary>
        [Fact]
        public async Task GetCompanyRanking_WithInvalidLimit_ReturnsBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/company-ranking?limit=0");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("o limite deve estar entre 1 e 100", content.ToLower());
        }

        /// <summary>
        /// Testa se o endpoint GET /api/esg-dashboard/comparison retorna status 200
        /// </summary>
        [Fact]
        public async Task GetComparison_WithValidCompanies_ReturnsSuccessStatusCode()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/comparison?companyIds=1&companyIds=2");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa comparação sem empresas especificadas
        /// </summary>
        [Fact]
        public async Task GetComparison_WithoutCompanies_ReturnsBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/comparison");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("pelo menos uma empresa deve ser especificada", content.ToLower());
        }

        /// <summary>
        /// Testa comparação com muitas empresas
        /// </summary>
        [Fact]
        public async Task GetComparison_WithTooManyCompanies_ReturnsBadRequest()
        {
            // Act
            var companyIds = string.Join("&", Enumerable.Range(1, 15).Select(i => $"companyIds={i}"));
            var response = await _client.GetAsync($"/api/esg-dashboard/comparison?{companyIds}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("máximo de 10 empresas", content.ToLower());
        }

        /// <summary>
        /// Testa estatísticas com filtro de empresa
        /// </summary>
        [Fact]
        public async Task GetESGStatistics_WithCompanyFilter_ReturnsFilteredData()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/esg-dashboard/statistics?companyId=1");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

            // Adicionar empresas de teste
            var companies = new[]
            {
                new ESGSustainabilityAPI.Models.Company
                {
                    Id = 1,
                    Name = "Empresa Dashboard 1",
                    CNPJ = "44.444.444/0001-44",
                    Industry = "Tecnologia",
                    Address = "Rua Dashboard, 111",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01000-000",
                    ContactEmail = "dashboard1@teste.com",
                    ContactPhone = "(11) 4444-4444",
                    EmployeeCount = 100,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ESGSustainabilityAPI.Models.Company
                {
                    Id = 2,
                    Name = "Empresa Dashboard 2",
                    CNPJ = "55.555.555/0001-55",
                    Industry = "Energia",
                    Address = "Rua Dashboard, 222",
                    City = "Rio de Janeiro",
                    State = "RJ",
                    ZipCode = "20000-000",
                    ContactEmail = "dashboard2@teste.com",
                    ContactPhone = "(21) 5555-5555",
                    EmployeeCount = 200,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Companies.AddRange(companies);

            // Adicionar emissões de teste
            var emissions = new[]
            {
                new ESGSustainabilityAPI.Models.CarbonEmission
                {
                    Id = 1,
                    Source = "Energia Dashboard 1",
                    EmissionAmount = 25.0m,
                    Unit = "tCO2e",
                    RecordDate = new DateTime(2024, 6, 1),
                    Category = "Escopo 2",
                    Location = "São Paulo - SP",
                    Description = "Emissão dashboard teste 1",
                    CompanyId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ESGSustainabilityAPI.Models.CarbonEmission
                {
                    Id = 2,
                    Source = "Energia Dashboard 2",
                    EmissionAmount = 35.0m,
                    Unit = "tCO2e",
                    RecordDate = new DateTime(2024, 6, 15),
                    Category = "Escopo 2",
                    Location = "Rio de Janeiro - RJ",
                    Description = "Emissão dashboard teste 2",
                    CompanyId = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.CarbonEmissions.AddRange(emissions);

            // Adicionar consumos de energia de teste
            var consumptions = new[]
            {
                new ESGSustainabilityAPI.Models.EnergyConsumption
                {
                    Id = 1,
                    EnergyType = "Elétrica",
                    ConsumptionAmount = 4000.0m,
                    Unit = "kWh",
                    RecordDate = new DateTime(2024, 6, 1),
                    Source = "Rede Elétrica",
                    Cost = 2000.0m,
                    CostCurrency = "BRL",
                    RenewablePercentage = 30.0m,
                    Description = "Consumo dashboard teste 1",
                    CompanyId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ESGSustainabilityAPI.Models.EnergyConsumption
                {
                    Id = 2,
                    EnergyType = "Solar",
                    ConsumptionAmount = 3000.0m,
                    Unit = "kWh",
                    RecordDate = new DateTime(2024, 6, 15),
                    Source = "Painéis Solares",
                    Cost = 1000.0m,
                    CostCurrency = "BRL",
                    RenewablePercentage = 100.0m,
                    Description = "Consumo dashboard teste 2",
                    CompanyId = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.EnergyConsumptions.AddRange(consumptions);

            // Adicionar relatórios de teste
            var reports = new[]
            {
                new ESGSustainabilityAPI.Models.SustainabilityReport
                {
                    Id = 1,
                    Title = "Relatório Dashboard 1",
                    Year = 2024,
                    Quarter = 2,
                    TotalCarbonEmissions = 25.0m,
                    TotalEnergyConsumption = 4000.0m,
                    RenewableEnergyPercentage = 30.0m,
                    ESGScore = "B",
                    CompanyId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ESGSustainabilityAPI.Models.SustainabilityReport
                {
                    Id = 2,
                    Title = "Relatório Dashboard 2",
                    Year = 2024,
                    Quarter = 2,
                    TotalCarbonEmissions = 35.0m,
                    TotalEnergyConsumption = 3000.0m,
                    RenewableEnergyPercentage = 100.0m,
                    ESGScore = "A",
                    CompanyId = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.SustainabilityReports.AddRange(reports);
            await context.SaveChangesAsync();
        }
    }
}

