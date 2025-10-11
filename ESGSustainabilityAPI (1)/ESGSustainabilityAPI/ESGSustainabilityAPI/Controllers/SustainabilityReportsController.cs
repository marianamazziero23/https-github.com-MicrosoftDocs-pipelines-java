using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESGSustainabilityAPI.Data;
using ESGSustainabilityAPI.Models;
using ESGSustainabilityAPI.ViewModels;

namespace ESGSustainabilityAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de relatórios de sustentabilidade
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SustainabilityReportsController : ControllerBase
    {
        private readonly ESGDbContext _context;
        private readonly ILogger<SustainabilityReportsController> _logger;

        public SustainabilityReportsController(ESGDbContext context, ILogger<SustainabilityReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lista relatórios de sustentabilidade com paginação e filtros
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação e filtros</param>
        /// <returns>Lista paginada de relatórios de sustentabilidade</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseViewModel<PagedResultViewModel<SustainabilityReportListViewModel>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<PagedResultViewModel<SustainabilityReportListViewModel>>>> GetSustainabilityReports(
            [FromQuery] PaginationParametersViewModel parameters,
            [FromQuery] int? year,
            [FromQuery] int? quarter,
            [FromQuery] int? companyId)
        {
            try
            {
                _logger.LogInformation("Buscando relatórios de sustentabilidade - Página: {Page}, Tamanho: {PageSize}", 
                    parameters.Page, parameters.PageSize);

                var query = _context.SustainabilityReports
                    .Include(r => r.Company)
                    .AsQueryable();

                // Aplicar filtros específicos
                if (year.HasValue)
                {
                    query = query.Where(r => r.Year == year.Value);
                }

                if (quarter.HasValue)
                {
                    query = query.Where(r => r.Quarter == quarter.Value);
                }

                if (companyId.HasValue)
                {
                    query = query.Where(r => r.CompanyId == companyId.Value);
                }

                // Aplicar filtros gerais
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    query = query.Where(r => r.Title.Contains(parameters.SearchTerm) ||
                                           r.ESGScore.Contains(parameters.SearchTerm) ||
                                           r.Company.Name.Contains(parameters.SearchTerm));
                }

                // Aplicar ordenação
                query = parameters.SortBy?.ToLower() switch
                {
                    "title" => parameters.SortDirection?.ToLower() == "desc" 
                        ? query.OrderByDescending(r => r.Title)
                        : query.OrderBy(r => r.Title),
                    "year" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.Year)
                        : query.OrderBy(r => r.Year),
                    "quarter" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.Quarter)
                        : query.OrderBy(r => r.Quarter),
                    "esgscore" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.ESGScore)
                        : query.OrderBy(r => r.ESGScore),
                    "emissions" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.TotalCarbonEmissions)
                        : query.OrderBy(r => r.TotalCarbonEmissions),
                    "energy" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.TotalEnergyConsumption)
                        : query.OrderBy(r => r.TotalEnergyConsumption),
                    "company" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(r => r.Company.Name)
                        : query.OrderBy(r => r.Company.Name),
                    _ => query.OrderByDescending(r => r.Year).ThenByDescending(r => r.Quarter)
                };

                // Contar total de registros
                var totalRecords = await query.CountAsync();

                // Aplicar paginação
                var reports = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(r => new SustainabilityReportListViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Year = r.Year,
                        Quarter = r.Quarter,
                        TotalCarbonEmissions = r.TotalCarbonEmissions,
                        TotalEnergyConsumption = r.TotalEnergyConsumption,
                        RenewableEnergyPercentage = r.RenewableEnergyPercentage,
                        ESGScore = r.ESGScore,
                        CompanyName = r.Company.Name,
                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling(totalRecords / (double)parameters.PageSize);

                var pagedResult = new PagedResultViewModel<SustainabilityReportListViewModel>
                {
                    Data = reports,
                    CurrentPage = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                };

                var response = new ApiResponseViewModel<PagedResultViewModel<SustainabilityReportListViewModel>>
                {
                    Success = true,
                    Message = "Relatórios de sustentabilidade recuperados com sucesso",
                    Data = pagedResult
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar relatórios de sustentabilidade");
                
                var errorResponse = new ErrorResponseViewModel
                {
                    Message = "Erro interno do servidor",
                    Details = ex.Message,
                    StatusCode = 500,
                    TraceId = HttpContext.TraceIdentifier
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Obtém um relatório de sustentabilidade específico por ID
        /// </summary>
        /// <param name="id">ID do relatório</param>
        /// <returns>Detalhes do relatório de sustentabilidade</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseViewModel<SustainabilityReportDetailViewModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 404)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<SustainabilityReportDetailViewModel>>> GetSustainabilityReport(int id)
        {
            try
            {
                _logger.LogInformation("Buscando relatório de sustentabilidade com ID: {Id}", id);

                var report = await _context.SustainabilityReports
                    .Include(r => r.Company)
                    .Where(r => r.Id == id)
                    .Select(r => new SustainabilityReportDetailViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Year = r.Year,
                        Quarter = r.Quarter,
                        TotalCarbonEmissions = r.TotalCarbonEmissions,
                        TotalEnergyConsumption = r.TotalEnergyConsumption,
                        RenewableEnergyPercentage = r.RenewableEnergyPercentage,
                        WaterConsumption = r.WaterConsumption,
                        WasteGenerated = r.WasteGenerated,
                        WasteRecycled = r.WasteRecycled,
                        ESGScore = r.ESGScore,
                        EnvironmentalInitiatives = r.EnvironmentalInitiatives,
                        SocialInitiatives = r.SocialInitiatives,
                        GovernanceInitiatives = r.GovernanceInitiatives,
                        Challenges = r.Challenges,
                        FutureGoals = r.FutureGoals,
                        CompanyId = r.CompanyId,
                        CompanyName = r.Company.Name,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (report == null)
                {
                    var notFoundResponse = new ErrorResponseViewModel
                    {
                        Message = "Relatório de sustentabilidade não encontrado",
                        StatusCode = 404,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return NotFound(notFoundResponse);
                }

                var response = new ApiResponseViewModel<SustainabilityReportDetailViewModel>
                {
                    Success = true,
                    Message = "Relatório de sustentabilidade encontrado",
                    Data = report
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar relatório de sustentabilidade com ID: {Id}", id);
                
                var errorResponse = new ErrorResponseViewModel
                {
                    Message = "Erro interno do servidor",
                    Details = ex.Message,
                    StatusCode = 500,
                    TraceId = HttpContext.TraceIdentifier
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Cria um novo relatório de sustentabilidade
        /// </summary>
        /// <param name="viewModel">Dados do novo relatório</param>
        /// <returns>Relatório criado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseViewModel<SustainabilityReportDetailViewModel>), 201)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<SustainabilityReportDetailViewModel>>> CreateSustainabilityReport(
            [FromBody] CreateSustainabilityReportViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var validationResponse = new ApiResponseViewModel<SustainabilityReportDetailViewModel>
                    {
                        Success = false,
                        Message = "Dados inválidos",
                        Errors = errors
                    };

                    return BadRequest(validationResponse);
                }

                _logger.LogInformation("Criando novo relatório de sustentabilidade para empresa ID: {CompanyId}", viewModel.CompanyId);

                // Verificar se a empresa existe
                var companyExists = await _context.Companies.AnyAsync(c => c.Id == viewModel.CompanyId);
                if (!companyExists)
                {
                    var companyNotFoundResponse = new ErrorResponseViewModel
                    {
                        Message = "Empresa não encontrada",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return BadRequest(companyNotFoundResponse);
                }

                // Verificar se já existe um relatório para o mesmo período
                var existingReport = await _context.SustainabilityReports
                    .AnyAsync(r => r.CompanyId == viewModel.CompanyId && 
                                  r.Year == viewModel.Year && 
                                  r.Quarter == viewModel.Quarter);

                if (existingReport)
                {
                    var duplicateResponse = new ErrorResponseViewModel
                    {
                        Message = "Já existe um relatório para esta empresa no período especificado",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return BadRequest(duplicateResponse);
                }

                var report = new SustainabilityReport
                {
                    Title = viewModel.Title,
                    Year = viewModel.Year,
                    Quarter = viewModel.Quarter,
                    TotalCarbonEmissions = viewModel.TotalCarbonEmissions,
                    TotalEnergyConsumption = viewModel.TotalEnergyConsumption,
                    RenewableEnergyPercentage = viewModel.RenewableEnergyPercentage,
                    WaterConsumption = viewModel.WaterConsumption,
                    WasteGenerated = viewModel.WasteGenerated,
                    WasteRecycled = viewModel.WasteRecycled,
                    ESGScore = viewModel.ESGScore,
                    EnvironmentalInitiatives = viewModel.EnvironmentalInitiatives,
                    SocialInitiatives = viewModel.SocialInitiatives,
                    GovernanceInitiatives = viewModel.GovernanceInitiatives,
                    Challenges = viewModel.Challenges,
                    FutureGoals = viewModel.FutureGoals,
                    CompanyId = viewModel.CompanyId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SustainabilityReports.Add(report);
                await _context.SaveChangesAsync();

                // Buscar o relatório criado com os dados da empresa
                var createdReport = await _context.SustainabilityReports
                    .Include(r => r.Company)
                    .Where(r => r.Id == report.Id)
                    .Select(r => new SustainabilityReportDetailViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Year = r.Year,
                        Quarter = r.Quarter,
                        TotalCarbonEmissions = r.TotalCarbonEmissions,
                        TotalEnergyConsumption = r.TotalEnergyConsumption,
                        RenewableEnergyPercentage = r.RenewableEnergyPercentage,
                        WaterConsumption = r.WaterConsumption,
                        WasteGenerated = r.WasteGenerated,
                        WasteRecycled = r.WasteRecycled,
                        ESGScore = r.ESGScore,
                        EnvironmentalInitiatives = r.EnvironmentalInitiatives,
                        SocialInitiatives = r.SocialInitiatives,
                        GovernanceInitiatives = r.GovernanceInitiatives,
                        Challenges = r.Challenges,
                        FutureGoals = r.FutureGoals,
                        CompanyId = r.CompanyId,
                        CompanyName = r.Company.Name,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .FirstAsync();

                var response = new ApiResponseViewModel<SustainabilityReportDetailViewModel>
                {
                    Success = true,
                    Message = "Relatório de sustentabilidade criado com sucesso",
                    Data = createdReport
                };

                return CreatedAtAction(nameof(GetSustainabilityReport), new { id = report.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar relatório de sustentabilidade");
                
                var errorResponse = new ErrorResponseViewModel
                {
                    Message = "Erro interno do servidor",
                    Details = ex.Message,
                    StatusCode = 500,
                    TraceId = HttpContext.TraceIdentifier
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Gera relatório automático baseado nos dados existentes
        /// </summary>
        /// <param name="companyId">ID da empresa</param>
        /// <param name="year">Ano do relatório</param>
        /// <param name="quarter">Trimestre do relatório</param>
        /// <returns>Relatório gerado automaticamente</returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(ApiResponseViewModel<SustainabilityReportDetailViewModel>), 201)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<SustainabilityReportDetailViewModel>>> GenerateAutomaticReport(
            [FromQuery] int companyId,
            [FromQuery] int year,
            [FromQuery] int quarter)
        {
            try
            {
                _logger.LogInformation("Gerando relatório automático para empresa {CompanyId}, {Year}Q{Quarter}", 
                    companyId, year, quarter);

                // Verificar se a empresa existe
                var company = await _context.Companies.FindAsync(companyId);
                if (company == null)
                {
                    var companyNotFoundResponse = new ErrorResponseViewModel
                    {
                        Message = "Empresa não encontrada",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return BadRequest(companyNotFoundResponse);
                }

                // Calcular datas do trimestre
                var startDate = new DateTime(year, (quarter - 1) * 3 + 1, 1);
                var endDate = startDate.AddMonths(3).AddDays(-1);

                // Calcular dados agregados
                var totalEmissions = await _context.CarbonEmissions
                    .Where(e => e.CompanyId == companyId && 
                               e.RecordDate >= startDate && 
                               e.RecordDate <= endDate)
                    .SumAsync(e => e.EmissionAmount);

                var totalEnergyConsumption = await _context.EnergyConsumptions
                    .Where(e => e.CompanyId == companyId && 
                               e.RecordDate >= startDate && 
                               e.RecordDate <= endDate)
                    .SumAsync(e => e.ConsumptionAmount);

                var renewablePercentage = await _context.EnergyConsumptions
                    .Where(e => e.CompanyId == companyId && 
                               e.RecordDate >= startDate && 
                               e.RecordDate <= endDate &&
                               e.RenewablePercentage.HasValue)
                    .AverageAsync(e => e.RenewablePercentage.Value);

                // Calcular score ESG simplificado
                var esgScore = CalculateESGScore(totalEmissions, totalEnergyConsumption, renewablePercentage);

                var report = new SustainabilityReport
                {
                    Title = $"Relatório de Sustentabilidade - {company.Name} - {year}Q{quarter}",
                    Year = year,
                    Quarter = quarter,
                    TotalCarbonEmissions = totalEmissions,
                    TotalEnergyConsumption = totalEnergyConsumption,
                    RenewableEnergyPercentage = renewablePercentage,
                    WaterConsumption = 0, // Seria calculado se houvesse dados
                    WasteGenerated = 0,   // Seria calculado se houvesse dados
                    WasteRecycled = 0,    // Seria calculado se houvesse dados
                    ESGScore = esgScore,
                    EnvironmentalInitiatives = "Relatório gerado automaticamente com base nos dados disponíveis.",
                    SocialInitiatives = "Dados não disponíveis para geração automática.",
                    GovernanceInitiatives = "Dados não disponíveis para geração automática.",
                    Challenges = "Análise detalhada necessária para identificação de desafios específicos.",
                    FutureGoals = "Definição de metas requer análise estratégica personalizada.",
                    CompanyId = companyId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SustainabilityReports.Add(report);
                await _context.SaveChangesAsync();

                // Buscar o relatório criado com os dados da empresa
                var createdReport = await _context.SustainabilityReports
                    .Include(r => r.Company)
                    .Where(r => r.Id == report.Id)
                    .Select(r => new SustainabilityReportDetailViewModel
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Year = r.Year,
                        Quarter = r.Quarter,
                        TotalCarbonEmissions = r.TotalCarbonEmissions,
                        TotalEnergyConsumption = r.TotalEnergyConsumption,
                        RenewableEnergyPercentage = r.RenewableEnergyPercentage,
                        WaterConsumption = r.WaterConsumption,
                        WasteGenerated = r.WasteGenerated,
                        WasteRecycled = r.WasteRecycled,
                        ESGScore = r.ESGScore,
                        EnvironmentalInitiatives = r.EnvironmentalInitiatives,
                        SocialInitiatives = r.SocialInitiatives,
                        GovernanceInitiatives = r.GovernanceInitiatives,
                        Challenges = r.Challenges,
                        FutureGoals = r.FutureGoals,
                        CompanyId = r.CompanyId,
                        CompanyName = r.Company.Name,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .FirstAsync();

                var response = new ApiResponseViewModel<SustainabilityReportDetailViewModel>
                {
                    Success = true,
                    Message = "Relatório de sustentabilidade gerado automaticamente com sucesso",
                    Data = createdReport
                };

                return CreatedAtAction(nameof(GetSustainabilityReport), new { id = report.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório automático");
                
                var errorResponse = new ErrorResponseViewModel
                {
                    Message = "Erro interno do servidor",
                    Details = ex.Message,
                    StatusCode = 500,
                    TraceId = HttpContext.TraceIdentifier
                };

                return StatusCode(500, errorResponse);
            }
        }

        private string CalculateESGScore(decimal emissions, decimal energy, decimal renewable)
        {
            // Algoritmo simplificado para calcular score ESG
            var score = 100;

            // Penalizar por altas emissões (exemplo: > 50 tCO2e)
            if (emissions > 50) score -= 20;
            else if (emissions > 20) score -= 10;

            // Penalizar por alto consumo de energia (exemplo: > 10000 kWh)
            if (energy > 10000) score -= 15;
            else if (energy > 5000) score -= 8;

            // Bonificar por alta porcentagem de energia renovável
            if (renewable > 80) score += 10;
            else if (renewable > 50) score += 5;
            else if (renewable < 20) score -= 10;

            return score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "E"
            };
        }
    }
}

