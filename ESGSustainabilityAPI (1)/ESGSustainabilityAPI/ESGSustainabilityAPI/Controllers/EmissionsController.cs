using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESGSustainabilityAPI.Data;
using ESGSustainabilityAPI.Models;
using ESGSustainabilityAPI.ViewModels;

namespace ESGSustainabilityAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de emissões de carbono
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EmissionsController : ControllerBase
    {
        private readonly ESGDbContext _context;
        private readonly ILogger<EmissionsController> _logger;

        public EmissionsController(ESGDbContext context, ILogger<EmissionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lista emissões de carbono com paginação e filtros
        /// </summary>
        /// <param name="parameters">Parâmetros de paginação e filtros</param>
        /// <returns>Lista paginada de emissões de carbono</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseViewModel<PagedResultViewModel<CarbonEmissionListViewModel>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<PagedResultViewModel<CarbonEmissionListViewModel>>>> GetEmissions(
            [FromQuery] PaginationParametersViewModel parameters)
        {
            try
            {
                _logger.LogInformation("Buscando emissões de carbono - Página: {Page}, Tamanho: {PageSize}", 
                    parameters.Page, parameters.PageSize);

                var query = _context.CarbonEmissions
                    .Include(e => e.Company)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(parameters.SearchTerm))
                {
                    query = query.Where(e => e.Source.Contains(parameters.SearchTerm) ||
                                           e.Category.Contains(parameters.SearchTerm) ||
                                           e.Location.Contains(parameters.SearchTerm) ||
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
                    "source" => parameters.SortDirection?.ToLower() == "desc" 
                        ? query.OrderByDescending(e => e.Source)
                        : query.OrderBy(e => e.Source),
                    "amount" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.EmissionAmount)
                        : query.OrderBy(e => e.EmissionAmount),
                    "date" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.RecordDate)
                        : query.OrderBy(e => e.RecordDate),
                    "company" => parameters.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(e => e.Company.Name)
                        : query.OrderBy(e => e.Company.Name),
                    _ => query.OrderByDescending(e => e.CreatedAt)
                };

                // Contar total de registros
                var totalRecords = await query.CountAsync();

                // Aplicar paginação
                var emissions = await query
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(e => new CarbonEmissionListViewModel
                    {
                        Id = e.Id,
                        Source = e.Source,
                        EmissionAmount = e.EmissionAmount,
                        Unit = e.Unit,
                        RecordDate = e.RecordDate,
                        Category = e.Category,
                        Location = e.Location,
                        CompanyName = e.Company.Name,
                        CreatedAt = e.CreatedAt
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling(totalRecords / (double)parameters.PageSize);

                var pagedResult = new PagedResultViewModel<CarbonEmissionListViewModel>
                {
                    Data = emissions,
                    CurrentPage = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                };

                var response = new ApiResponseViewModel<PagedResultViewModel<CarbonEmissionListViewModel>>
                {
                    Success = true,
                    Message = "Emissões de carbono recuperadas com sucesso",
                    Data = pagedResult
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar emissões de carbono");
                
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
        /// Obtém uma emissão de carbono específica por ID
        /// </summary>
        /// <param name="id">ID da emissão</param>
        /// <returns>Detalhes da emissão de carbono</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseViewModel<CarbonEmissionDetailViewModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 404)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<CarbonEmissionDetailViewModel>>> GetEmission(int id)
        {
            try
            {
                _logger.LogInformation("Buscando emissão de carbono com ID: {Id}", id);

                var emission = await _context.CarbonEmissions
                    .Include(e => e.Company)
                    .Where(e => e.Id == id)
                    .Select(e => new CarbonEmissionDetailViewModel
                    {
                        Id = e.Id,
                        Source = e.Source,
                        EmissionAmount = e.EmissionAmount,
                        Unit = e.Unit,
                        RecordDate = e.RecordDate,
                        Category = e.Category,
                        Location = e.Location,
                        Description = e.Description,
                        CompanyId = e.CompanyId,
                        CompanyName = e.Company.Name,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (emission == null)
                {
                    var notFoundResponse = new ErrorResponseViewModel
                    {
                        Message = "Emissão de carbono não encontrada",
                        StatusCode = 404,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return NotFound(notFoundResponse);
                }

                var response = new ApiResponseViewModel<CarbonEmissionDetailViewModel>
                {
                    Success = true,
                    Message = "Emissão de carbono encontrada",
                    Data = emission
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar emissão de carbono com ID: {Id}", id);
                
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
        /// Registra uma nova emissão de carbono
        /// </summary>
        /// <param name="viewModel">Dados da nova emissão</param>
        /// <returns>Emissão criada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseViewModel<CarbonEmissionDetailViewModel>), 201)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 500)]
        public async Task<ActionResult<ApiResponseViewModel<CarbonEmissionDetailViewModel>>> CreateEmission(
            [FromBody] CreateCarbonEmissionViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var validationResponse = new ApiResponseViewModel<CarbonEmissionDetailViewModel>
                    {
                        Success = false,
                        Message = "Dados inválidos",
                        Errors = errors
                    };

                    return BadRequest(validationResponse);
                }

                _logger.LogInformation("Criando nova emissão de carbono para empresa ID: {CompanyId}", viewModel.CompanyId);

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

                var emission = new CarbonEmission
                {
                    Source = viewModel.Source,
                    EmissionAmount = viewModel.EmissionAmount,
                    Unit = viewModel.Unit,
                    RecordDate = viewModel.RecordDate,
                    Category = viewModel.Category,
                    Location = viewModel.Location,
                    Description = viewModel.Description,
                    CompanyId = viewModel.CompanyId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CarbonEmissions.Add(emission);
                await _context.SaveChangesAsync();

                // Buscar a emissão criada com os dados da empresa
                var createdEmission = await _context.CarbonEmissions
                    .Include(e => e.Company)
                    .Where(e => e.Id == emission.Id)
                    .Select(e => new CarbonEmissionDetailViewModel
                    {
                        Id = e.Id,
                        Source = e.Source,
                        EmissionAmount = e.EmissionAmount,
                        Unit = e.Unit,
                        RecordDate = e.RecordDate,
                        Category = e.Category,
                        Location = e.Location,
                        Description = e.Description,
                        CompanyId = e.CompanyId,
                        CompanyName = e.Company.Name,
                        CreatedAt = e.CreatedAt,
                        UpdatedAt = e.UpdatedAt
                    })
                    .FirstAsync();

                var response = new ApiResponseViewModel<CarbonEmissionDetailViewModel>
                {
                    Success = true,
                    Message = "Emissão de carbono criada com sucesso",
                    Data = createdEmission
                };

                return CreatedAtAction(nameof(GetEmission), new { id = emission.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar emissão de carbono");
                
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

