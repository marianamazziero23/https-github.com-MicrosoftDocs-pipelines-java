using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESGSustainabilityAPI.Data;
using ESGSustainabilityAPI.ViewModels;

namespace ESGSustainabilityAPI.Controllers
{
    /// <summary>
    /// Controller para estatísticas e dashboard ESG
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ESGDashboardController : ControllerBase
    {
        private readonly ESGDbContext _context;
        private readonly ILogger<ESGDashboardController> _logger;

        public ESGDashboardController(ESGDbContext context, ILogger<ESGDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtém estatísticas gerais do sistema ESG
        /// </summary>
        /// <param name="companyId">ID da empresa (opcional)</param>
        /// <param name="startDate">Data inicial (opcional)</param>
        /// <param name="endDate">Data final (opcional)</param>
        /// <returns>Estatísticas gerais ESG</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponseViewModel<ESGStatisticsViewModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<ESGStatisticsViewModel>>> GetESGStatistics(
            [FromQuery] int? companyId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                _logger.LogInformation("Calculando estatísticas ESG gerais");

                var emissionsQuery = _context.CarbonEmissions.AsQueryable();
                var energyQuery = _context.EnergyConsumptions.AsQueryable();

                if (companyId.HasValue)
                {
                    emissionsQuery = emissionsQuery.Where(e => e.CompanyId == companyId.Value);
                    energyQuery = energyQuery.Where(e => e.CompanyId == companyId.Value);
                }

                if (startDate.HasValue)
                {
                    emissionsQuery = emissionsQuery.Where(e => e.RecordDate >= startDate.Value);
                    energyQuery = energyQuery.Where(e => e.RecordDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    emissionsQuery = emissionsQuery.Where(e => e.RecordDate <= endDate.Value);
                    energyQuery = energyQuery.Where(e => e.RecordDate <= endDate.Value);
                }

                var statistics = new ESGStatisticsViewModel
                {
                    TotalCarbonEmissions = await emissionsQuery.SumAsync(e => e.EmissionAmount),
                    TotalEnergyConsumption = await energyQuery.SumAsync(e => e.ConsumptionAmount),
                    AverageRenewablePercentage = await energyQuery
                        .Where(e => e.RenewablePercentage.HasValue)
                        .AverageAsync(e => e.RenewablePercentage.Value),
                    TotalCompanies = companyId.HasValue ? 1 : await _context.Companies.CountAsync(),
                    TotalReports = await _context.SustainabilityReports.CountAsync(),
                    LastUpdated = DateTime.UtcNow,
                    EmissionsByCategory = await emissionsQuery
                        .GroupBy(e => e.Category)
                        .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.EmissionAmount)),
                    EnergyByType = await energyQuery
                        .GroupBy(e => e.EnergyType)
                        .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.ConsumptionAmount))
                };

                var response = new ApiResponseViewModel<ESGStatisticsViewModel>
                {
                    Success = true,
                    Message = "Estatísticas ESG calculadas com sucesso",
                    Data = statistics
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular estatísticas ESG");
                
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
        /// Obtém tendências de emissões de carbono ao longo do tempo
        /// </summary>
        /// <param name="companyId">ID da empresa (opcional)</param>
        /// <param name="period">Período de agrupamento (month, quarter, year)</param>
        /// <returns>Tendências de emissões</returns>
        [HttpGet("emissions-trends")]
        [ProducesResponseType(typeof(ApiResponseViewModel<object>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<object>>> GetEmissionsTrends(
            [FromQuery] int? companyId,
            [FromQuery] string period = "month")
        {
            try
            {
                _logger.LogInformation("Calculando tendências de emissões - Período: {Period}", period);

                var query = _context.CarbonEmissions.AsQueryable();

                if (companyId.HasValue)
                {
                    query = query.Where(e => e.CompanyId == companyId.Value);
                }

                object trends = period.ToLower() switch
                {
                    "month" => await query
                        .GroupBy(e => new { e.RecordDate.Year, e.RecordDate.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            TotalEmissions = g.Sum(e => e.EmissionAmount),
                            AverageEmissions = g.Average(e => e.EmissionAmount),
                            RecordCount = g.Count()
                        })
                        .OrderBy(g => g.Year).ThenBy(g => g.Month)
                        .ToListAsync(),

                    "quarter" => await query
                        .GroupBy(e => new { e.RecordDate.Year, Quarter = (e.RecordDate.Month - 1) / 3 + 1 })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Quarter = g.Key.Quarter,
                            TotalEmissions = g.Sum(e => e.EmissionAmount),
                            AverageEmissions = g.Average(e => e.EmissionAmount),
                            RecordCount = g.Count()
                        })
                        .OrderBy(g => g.Year).ThenBy(g => g.Quarter)
                        .ToListAsync(),

                    "year" => await query
                        .GroupBy(e => e.RecordDate.Year)
                        .Select(g => new
                        {
                            Year = g.Key,
                            TotalEmissions = g.Sum(e => e.EmissionAmount),
                            AverageEmissions = g.Average(e => e.EmissionAmount),
                            RecordCount = g.Count()
                        })
                        .OrderBy(g => g.Year)
                        .ToListAsync(),

                    _ => throw new ArgumentException("Período inválido. Use: month, quarter ou year")
                };

                var response = new ApiResponseViewModel<object>
                {
                    Success = true,
                    Message = "Tendências de emissões calculadas com sucesso",
                    Data = trends
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var badRequestResponse = new ErrorResponseViewModel
                {
                    Message = ex.Message,
                    StatusCode = 400,
                    TraceId = HttpContext.TraceIdentifier
                };

                return BadRequest(badRequestResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular tendências de emissões");
                
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
        /// Obtém ranking de empresas por performance ESG
        /// </summary>
        /// <param name="metric">Métrica para ranking (emissions, energy, esg_score)</param>
        /// <param name="limit">Número máximo de empresas no ranking</param>
        /// <returns>Ranking de empresas</returns>
        [HttpGet("company-ranking")]
        [ProducesResponseType(typeof(ApiResponseViewModel<object>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<object>>> GetCompanyRanking(
            [FromQuery] string metric = "emissions",
            [FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Calculando ranking de empresas - Métrica: {Metric}", metric);

                if (limit <= 0 || limit > 100)
                {
                    var limitErrorResponse = new ErrorResponseViewModel
                    {
                        Message = "O limite deve estar entre 1 e 100",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return BadRequest(limitErrorResponse);
                }

                object ranking = metric.ToLower() switch
                {
                    "emissions" => await _context.Companies
                        .Select(c => new
                        {
                            CompanyId = c.Id,
                            CompanyName = c.Name,
                            Industry = c.Industry,
                            TotalEmissions = c.CarbonEmissions.Sum(e => e.EmissionAmount),
                            EmissionsPerEmployee = c.EmployeeCount > 0 
                                ? c.CarbonEmissions.Sum(e => e.EmissionAmount) / c.EmployeeCount 
                                : 0,
                            RecordCount = c.CarbonEmissions.Count()
                        })
                        .Where(c => c.RecordCount > 0)
                        .OrderBy(c => c.TotalEmissions)
                        .Take(limit)
                        .ToListAsync(),

                    "energy" => await _context.Companies
                        .Select(c => new
                        {
                            CompanyId = c.Id,
                            CompanyName = c.Name,
                            Industry = c.Industry,
                            TotalEnergyConsumption = c.EnergyConsumptions.Sum(e => e.ConsumptionAmount),
                            AverageRenewablePercentage = c.EnergyConsumptions
                                .Where(e => e.RenewablePercentage.HasValue)
                                .Average(e => e.RenewablePercentage.Value),
                            EnergyPerEmployee = c.EmployeeCount > 0 
                                ? c.EnergyConsumptions.Sum(e => e.ConsumptionAmount) / c.EmployeeCount 
                                : 0,
                            RecordCount = c.EnergyConsumptions.Count()
                        })
                        .Where(c => c.RecordCount > 0)
                        .OrderByDescending(c => c.AverageRenewablePercentage)
                        .Take(limit)
                        .ToListAsync(),

                    "esg_score" => await _context.Companies
                        .Select(c => new
                        {
                            CompanyId = c.Id,
                            CompanyName = c.Name,
                            Industry = c.Industry,
                            LatestESGScore = c.SustainabilityReports
                                .OrderByDescending(r => r.Year)
                                .ThenByDescending(r => r.Quarter)
                                .Select(r => r.ESGScore)
                                .FirstOrDefault(),
                            ReportCount = c.SustainabilityReports.Count(),
                            LatestReportDate = c.SustainabilityReports
                                .OrderByDescending(r => r.CreatedAt)
                                .Select(r => r.CreatedAt)
                                .FirstOrDefault()
                        })
                        .Where(c => c.ReportCount > 0 && !string.IsNullOrEmpty(c.LatestESGScore))
                        .OrderBy(c => c.LatestESGScore)
                        .Take(limit)
                        .ToListAsync(),

                    _ => throw new ArgumentException("Métrica inválida. Use: emissions, energy ou esg_score")
                };

                var response = new ApiResponseViewModel<object>
                {
                    Success = true,
                    Message = "Ranking de empresas calculado com sucesso",
                    Data = ranking
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var badRequestResponse = new ErrorResponseViewModel
                {
                    Message = ex.Message,
                    StatusCode = 400,
                    TraceId = HttpContext.TraceIdentifier
                };

                return BadRequest(badRequestResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular ranking de empresas");
                
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
        /// Obtém comparação entre empresas ou períodos
        /// </summary>
        /// <param name="companyIds">IDs das empresas para comparar</param>
        /// <param name="startDate">Data inicial</param>
        /// <param name="endDate">Data final</param>
        /// <returns>Dados de comparação</returns>
        [HttpGet("comparison")]
        [ProducesResponseType(typeof(ApiResponseViewModel<object>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<object>>> GetComparison(
            [FromQuery] int[] companyIds,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                _logger.LogInformation("Gerando comparação para empresas: {CompanyIds}", string.Join(",", companyIds));

                if (companyIds == null || companyIds.Length == 0)
                {
                    var noCompaniesResponse = new ErrorResponseViewModel
                    {
                        Message = "Pelo menos uma empresa deve ser especificada para comparação",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return BadRequest(noCompaniesResponse);
                }

                if (companyIds.Length > 10)
                {
                    var tooManyCompaniesResponse = new ErrorResponseViewModel
                    {
                        Message = "Máximo de 10 empresas podem ser comparadas simultaneamente",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return BadRequest(tooManyCompaniesResponse);
                }

                var comparison = new List<object>();

                foreach (var companyId in companyIds)
                {
                    var company = await _context.Companies.FindAsync(companyId);
                    if (company == null) continue;

                    var emissionsQuery = _context.CarbonEmissions.Where(e => e.CompanyId == companyId);
                    var energyQuery = _context.EnergyConsumptions.Where(e => e.CompanyId == companyId);

                    if (startDate.HasValue)
                    {
                        emissionsQuery = emissionsQuery.Where(e => e.RecordDate >= startDate.Value);
                        energyQuery = energyQuery.Where(e => e.RecordDate >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        emissionsQuery = emissionsQuery.Where(e => e.RecordDate <= endDate.Value);
                        energyQuery = energyQuery.Where(e => e.RecordDate <= endDate.Value);
                    }

                    var companyData = new
                    {
                        CompanyId = company.Id,
                        CompanyName = company.Name,
                        Industry = company.Industry,
                        EmployeeCount = company.EmployeeCount,
                        TotalEmissions = await emissionsQuery.SumAsync(e => e.EmissionAmount),
                        TotalEnergyConsumption = await energyQuery.SumAsync(e => e.ConsumptionAmount),
                        AverageRenewablePercentage = await energyQuery
                            .Where(e => e.RenewablePercentage.HasValue)
                            .AverageAsync(e => e.RenewablePercentage.Value),
                        EmissionsPerEmployee = company.EmployeeCount > 0 
                            ? await emissionsQuery.SumAsync(e => e.EmissionAmount) / company.EmployeeCount 
                            : 0,
                        EnergyPerEmployee = company.EmployeeCount > 0 
                            ? await energyQuery.SumAsync(e => e.ConsumptionAmount) / company.EmployeeCount 
                            : 0,
                        LatestESGScore = await _context.SustainabilityReports
                            .Where(r => r.CompanyId == companyId)
                            .OrderByDescending(r => r.Year)
                            .ThenByDescending(r => r.Quarter)
                            .Select(r => r.ESGScore)
                            .FirstOrDefaultAsync(),
                        Period = new
                        {
                            StartDate = startDate,
                            EndDate = endDate
                        }
                    };

                    comparison.Add(companyData);
                }

                var response = new ApiResponseViewModel<object>
                {
                    Success = true,
                    Message = "Comparação gerada com sucesso",
                    Data = new
                    {
                        Companies = comparison,
                        ComparisonDate = DateTime.UtcNow,
                        Period = new { StartDate = startDate, EndDate = endDate }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar comparação");
                
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
    }
}

