using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESGSustainabilityAPI.Data;
using ESGSustainabilityAPI.Models;
using ESGSustainabilityAPI.ViewModels;

namespace ESGSustainabilityAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de consumo de energia
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EnergyConsumptionController : ControllerBase
    {
        private readonly ESGDbContext _context;
        private readonly ILogger<EnergyConsumptionController> _logger;

        public EnergyConsumptionController(ESGDbContext context, ILogger<EnergyConsumptionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lista consumos de energia com paginação e filtros
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação e filtros</param>
        /// <returns>Lista paginada de consumos de energia</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseViewModel<PagedResultViewModel<EnergyConsumptionListViewModel>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<PagedResultViewModel<EnergyConsumptionListViewModel>>>> GetEnergyConsumptions(
            [FromQuery] PaginationParametersViewModel parameters)
        {
            try
            {
                _logger.LogInformation("Buscando consumos de energia - Página: {Page}, Tamanho: {PageSize}", 
                    parameters.Page, parameters.PageSize);

                var query = _context.EnergyConsumptions
                    .Include(e => e.Company)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    query = query.Where(e => e.EnergyType.Contains(parameters.SearchTerm) ||
                                           e.Source.Contains(parameters.SearchTerm) ||
                                           e.Company.Name.Contains(parameters.SearchTerm));
                }

                if (parameters.StartDate.HasValue)
                {
                    query = query.Where(e => e.RecordDate >= parameters.StartDate.Value);
                }

                if (parameters.EndDate.HasValue)
                {
                    query = query.Where(e => e.RecordDate <= parameters.EndDate.Value);
                }

                // Aplicar ordenação
                query = parameters.SortBy?.ToLower() switch
                {
                    "energytype" => parameters.SortDirection?.ToLower() == "desc" 
                        ? query.OrderByDescending(e => e.EnergyType)
                        : query.OrderBy(e => e.EnergyType),
                    "amount" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.ConsumptionAmount)
                        : query.OrderBy(e => e.ConsumptionAmount),
                    "date" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.RecordDate)
                        : query.OrderBy(e => e.RecordDate),
                    "cost" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.Cost)
                        : query.OrderBy(e => e.Cost),
                    "renewable" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.RenewablePercentage)
                        : query.OrderBy(e => e.RenewablePercentage),
                    "company" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.Company.Name)
                        : query.OrderBy(e => e.Company.Name),
                    _ => query.OrderByDescending(e => e.CreatedAt)
                };

                // Contar total de registros
                var totalRecords = await query.CountAsync();

                // Aplicar paginação
                var consumptions = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(e => new EnergyConsumptionListViewModel
                    {
                        Id = e.Id,
                        EnergyType = e.EnergyType,
                        ConsumptionAmount = e.ConsumptionAmount,
                        Unit = e.Unit,
                        RecordDate = e.RecordDate,
                        Source = e.Source,
                        Cost = e.Cost,
                        RenewablePercentage = e.RenewablePercentage,
                        CompanyName = e.Company.Name,
                        CreatedAt = e.CreatedAt
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling(totalRecords / (double)parameters.PageSize);

                var pagedResult = new PagedResultViewModel<EnergyConsumptionListViewModel>
                {
                    Data = consumptions,
                    CurrentPage = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                };

                var response = new ApiResponseViewModel<PagedResultViewModel<EnergyConsumptionListViewModel>>
                {
                    Success = true,
                    Message = "Consumos de energia recuperados com sucesso",
                    Data = pagedResult
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar consumos de energia");
                
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
        /// Obtém um consumo de energia específico por ID
        /// </summary>
        /// <param name="id">ID do consumo</param>
        /// <returns>Detalhes do consumo de energia</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseViewModel<EnergyConsumptionDetailViewModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 404)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<EnergyConsumptionDetailViewModel>>> GetEnergyConsumption(int id)
        {
            try
            {
                _logger.LogInformation("Buscando consumo de energia com ID: {Id}", id);

                var consumption = await _context.EnergyConsumptions
                    .Include(e => e.Company)
                    .Where(e => e.Id == id)
                    .Select(e => new EnergyConsumptionDetailViewModel
                    {
                        Id = e.Id,
                        EnergyType = e.EnergyType,
                        ConsumptionAmount = e.ConsumptionAmount,
                        Unit = e.Unit,
                        RecordDate = e.RecordDate,
                        Source = e.Source,
                        Cost = e.Cost,
                        CostCurrency = e.CostCurrency,
                        RenewablePercentage = e.RenewablePercentage,
                        Description = e.Description,
                        CompanyId = e.CompanyId,
                        CompanyName = e.Company.Name,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (consumption == null)
                {
                    var notFoundResponse = new ErrorResponseViewModel
                    {
                        Message = "Consumo de energia não encontrado",
                        StatusCode = 404,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return NotFound(notFoundResponse);
                }

                var response = new ApiResponseViewModel<EnergyConsumptionDetailViewModel>
                {
                    Success = true,
                    Message = "Consumo de energia encontrado",
                    Data = consumption
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar consumo de energia com ID: {Id}", id);
                
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
        /// Registra um novo consumo de energia
        /// </summary>
        /// <param name="viewModel">Dados do novo consumo</param>
        /// <returns>Consumo criado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseViewModel<EnergyConsumptionDetailViewModel>), 201)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<EnergyConsumptionDetailViewModel>>> CreateEnergyConsumption(
            [FromBody] CreateEnergyConsumptionViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var validationResponse = new ApiResponseViewModel<EnergyConsumptionDetailViewModel>
                    {
                        Success = false,
                        Message = "Dados inválidos",
                        Errors = errors
                    };

                    return BadRequest(validationResponse);
                }

                _logger.LogInformation("Criando novo consumo de energia para empresa ID: {CompanyId}", viewModel.CompanyId);

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

                var consumption = new EnergyConsumption
                {
                    EnergyType = viewModel.EnergyType,
                    ConsumptionAmount = viewModel.ConsumptionAmount,
                    Unit = viewModel.Unit,
                    RecordDate = viewModel.RecordDate,
                    Source = viewModel.Source,
                    Cost = viewModel.Cost,
                    CostCurrency = viewModel.CostCurrency,
                    RenewablePercentage = viewModel.RenewablePercentage,
                    Description = viewModel.Description,
                    CompanyId = viewModel.CompanyId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.EnergyConsumptions.Add(consumption);
                await _context.SaveChangesAsync();

                // Buscar o consumo criado com os dados da empresa
                var createdConsumption = await _context.EnergyConsumptions
                    .Include(e => e.Company)
                    .Where(e => e.Id == consumption.Id)
                    .Select(e => new EnergyConsumptionDetailViewModel
                    {
                        Id = e.Id,
                        EnergyType = e.EnergyType,
                        ConsumptionAmount = e.ConsumptionAmount,
                        Unit = e.Unit,
                        RecordDate = e.RecordDate,
                        Source = e.Source,
                        Cost = e.Cost,
                        CostCurrency = e.CostCurrency,
                        RenewablePercentage = e.RenewablePercentage,
                        Description = e.Description,
                        CompanyId = e.CompanyId,
                        CompanyName = e.Company.Name,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    })
                    .FirstAsync();

                var response = new ApiResponseViewModel<EnergyConsumptionDetailViewModel>
                {
                    Success = true,
                    Message = "Consumo de energia criado com sucesso",
                    Data = createdConsumption
                };

                return CreatedAtAction(nameof(GetEnergyConsumption), new { id = consumption.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar consumo de energia");
                
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
        /// Obtém estatísticas de consumo de energia
        /// </summary>
        /// <param name="companyId">ID da empresa (opcional)</param>
        /// <param name="startDate">Data inicial (opcional)</param>
        /// <param name="endDate">Data final (opcional)</param>
        /// <returns>Estatísticas de consumo de energia</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponseViewModel<object>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<object>>> GetEnergyStatistics(
            [FromQuery] int? companyId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                _logger.LogInformation("Calculando estatísticas de consumo de energia");

                var query = _context.EnergyConsumptions.AsQueryable();

                if (companyId.HasValue)
                {
                    query = query.Where(e => e.CompanyId == companyId.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(e => e.RecordDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(e => e.RecordDate <= endDate.Value);
                }

                var statistics = new
                {
                    TotalConsumption = await query.SumAsync(e => e.ConsumptionAmount),
                    AverageConsumption = await query.AverageAsync(e => e.ConsumptionAmount),
                    TotalCost = await query.SumAsync(e => e.Cost ?? 0),
                    AverageRenewablePercentage = await query.Where(e => e.RenewablePercentage.HasValue)
                                                           .AverageAsync(e => e.RenewablePercentage.Value),
                    ConsumptionByType = await query.GroupBy(e => e.EnergyType)
                                                  .Select(g => new { EnergyType = g.Key, Total = g.Sum(e => e.ConsumptionAmount) })
                                                  .ToListAsync(),
                    MonthlyTrend = await query.GroupBy(e => new { e.RecordDate.Year, e.RecordDate.Month })
                                             .Select(g => new { 
                                                 Year = g.Key.Year, 
                                                 Month = g.Key.Month, 
                                                 Total = g.Sum(e => e.ConsumptionAmount) 
                                             })
                                             .OrderBy(g => g.Year).ThenBy(g => g.Month)
                                             .ToListAsync()
                };

                var response = new ApiResponseViewModel<object>
                {
                    Success = true,
                    Message = "Estatísticas de consumo de energia calculadas com sucesso",
                    Data = statistics
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular estatísticas de consumo de energia");
                
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

