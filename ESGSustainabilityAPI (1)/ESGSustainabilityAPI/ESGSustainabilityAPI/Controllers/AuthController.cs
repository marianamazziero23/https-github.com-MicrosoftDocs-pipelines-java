using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ESGSustainabilityAPI.Data;
using ESGSustainabilityAPI.Models;
using ESGSustainabilityAPI.ViewModels;
using ESGSustainabilityAPI.Services;

namespace ESGSustainabilityAPI.Controllers
{
    /// <summary>
    /// Controller para autenticação e autorização
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly ESGDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ESGDbContext context, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza login do usuário
        /// </summary>
        /// <param name="loginModel">Dados de login</param>
        /// <returns>Token JWT e informações do usuário</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponseViewModel<AuthResponseViewModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 401)]
        public async Task<ActionResult<ApiResponseViewModel<AuthResponseViewModel>>> Login([FromBody] LoginViewModel loginModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var validationResponse = new ApiResponseViewModel<AuthResponseViewModel>
                    {
                        Success = false,
                        Message = "Dados inválidos",
                        Errors = errors
                    };

                    return BadRequest(validationResponse);
                }

                _logger.LogInformation("Tentativa de login para usuário: {Username}", loginModel.Username);

                // Buscar usuário
                var user = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Username == loginModel.Username && u.IsActive);

                if (user == null || !VerifyPassword(loginModel.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Falha no login para usuário: {Username}", loginModel.Username);
                    
                    var unauthorizedResponse = new ErrorResponseViewModel
                    {
                        Message = "Credenciais inválidas",
                        StatusCode = 401,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return Unauthorized(unauthorizedResponse);
                }

                // Atualizar último login
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Gerar token
                var token = _jwtService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(60); // Configurável

                var authResponse = new AuthResponseViewModel
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = new UserInfoViewModel
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role,
                        CompanyId = user.CompanyId,
                        CompanyName = user.Company?.Name,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                };

                var response = new ApiResponseViewModel<AuthResponseViewModel>
                {
                    Success = true,
                    Message = "Login realizado com sucesso",
                    Data = authResponse
                };

                _logger.LogInformation("Login bem-sucedido para usuário: {Username}", loginModel.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o login");
                
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
        /// Registra um novo usuário
        /// </summary>
        /// <param name="registerModel">Dados de registro</param>
        /// <returns>Usuário criado</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponseViewModel<UserInfoViewModel>), 201)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        public async Task<ActionResult<ApiResponseViewModel<UserInfoViewModel>>> Register([FromBody] RegisterViewModel registerModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var validationResponse = new ApiResponseViewModel<UserInfoViewModel>
                    {
                        Success = false,
                        Message = "Dados inválidos",
                        Errors = errors
                    };

                    return BadRequest(validationResponse);
                }

                _logger.LogInformation("Tentativa de registro para usuário: {Username}", registerModel.Username);

                // Verificar se usuário já existe
                var existingUser = await _context.Users
                    .AnyAsync(u => u.Username == registerModel.Username || u.Email == registerModel.Email);

                if (existingUser)
                {
                    var conflictResponse = new ErrorResponseViewModel
                    {
                        Message = "Usuário ou email já existe",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return BadRequest(conflictResponse);
                }

                // Verificar se empresa existe (se fornecida)
                if (registerModel.CompanyId.HasValue)
                {
                    var companyExists = await _context.Companies.AnyAsync(c => c.Id == registerModel.CompanyId.Value);
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
                }

                // Criar usuário
                var user = new User
                {
                    Username = registerModel.Username,
                    Email = registerModel.Email,
                    PasswordHash = HashPassword(registerModel.Password),
                    FirstName = registerModel.FirstName,
                    LastName = registerModel.LastName,
                    Role = "User", // Usuários novos sempre começam como "User"
                    CompanyId = registerModel.CompanyId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Buscar usuário criado com dados da empresa
                var createdUser = await _context.Users
                    .Include(u => u.Company)
                    .FirstAsync(u => u.Id == user.Id);

                var userInfo = new UserInfoViewModel
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    Role = createdUser.Role,
                    CompanyId = createdUser.CompanyId,
                    CompanyName = createdUser.Company?.Name,
                    CreatedAt = createdUser.CreatedAt,
                    LastLoginAt = createdUser.LastLoginAt
                };

                var response = new ApiResponseViewModel<UserInfoViewModel>
                {
                    Success = true,
                    Message = "Usuário registrado com sucesso",
                    Data = userInfo
                };

                _logger.LogInformation("Usuário registrado com sucesso: {Username}", registerModel.Username);
                return CreatedAtAction(nameof(GetProfile), new { }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o registro");
                
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
        /// Obtém o perfil do usuário autenticado
        /// </summary>
        /// <returns>Informações do usuário</returns>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponseViewModel<UserInfoViewModel>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 401)]
        public async Task<ActionResult<ApiResponseViewModel<UserInfoViewModel>>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    var unauthorizedResponse = new ErrorResponseViewModel
                    {
                        Message = "Token inválido",
                        StatusCode = 401,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return Unauthorized(unauthorizedResponse);
                }

                var user = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value && u.IsActive);

                if (user == null)
                {
                    var notFoundResponse = new ErrorResponseViewModel
                    {
                        Message = "Usuário não encontrado",
                        StatusCode = 404,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return NotFound(notFoundResponse);
                }

                var userInfo = new UserInfoViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company?.Name,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };

                var response = new ApiResponseViewModel<UserInfoViewModel>
                {
                    Success = true,
                    Message = "Perfil recuperado com sucesso",
                    Data = userInfo
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar perfil do usuário");
                
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
        /// Altera a senha do usuário autenticado
        /// </summary>
        /// <param name="changePasswordModel">Dados para alteração de senha</param>
        /// <returns>Confirmação da alteração</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponseViewModel<object>), 200)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 400)]
        [ProducesResponseType(typeof(ErrorResponseViewModel), 401)]
        public async Task<ActionResult<ApiResponseViewModel<object>>> ChangePassword([FromBody] ChangePasswordViewModel changePasswordModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var validationResponse = new ApiResponseViewModel<object>
                    {
                        Success = false,
                        Message = "Dados inválidos",
                        Errors = errors
                    };

                    return BadRequest(validationResponse);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    var unauthorizedResponse = new ErrorResponseViewModel
                    {
                        Message = "Token inválido",
                        StatusCode = 401,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return Unauthorized(unauthorizedResponse);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value && u.IsActive);
                if (user == null)
                {
                    var notFoundResponse = new ErrorResponseViewModel
                    {
                        Message = "Usuário não encontrado",
                        StatusCode = 404,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return NotFound(notFoundResponse);
                }

                // Verificar senha atual
                if (!VerifyPassword(changePasswordModel.CurrentPassword, user.PasswordHash))
                {
                    var wrongPasswordResponse = new ErrorResponseViewModel
                    {
                        Message = "Senha atual incorreta",
                        StatusCode = 400,
                        TraceId = HttpContext.TraceIdentifier
                    };

                    return BadRequest(wrongPasswordResponse);
                }

                // Atualizar senha
                user.PasswordHash = HashPassword(changePasswordModel.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var response = new ApiResponseViewModel<object>
                {
                    Success = true,
                    Message = "Senha alterada com sucesso",
                    Data = new { }
                };

                _logger.LogInformation("Senha alterada para usuário: {UserId}", userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha");
                
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
        /// Obtém o ID do usuário atual do token JWT
        /// </summary>
        /// <returns>ID do usuário ou null</returns>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;

            return int.TryParse(userIdClaim.Value, out int userId) ? userId : null;
        }

        /// <summary>
        /// Gera hash da senha usando BCrypt
        /// </summary>
        /// <param name="password">Senha em texto plano</param>
        /// <returns>Hash da senha</returns>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifica se a senha corresponde ao hash
        /// </summary>
        /// <param name="password">Senha em texto plano</param>
        /// <param name="hash">Hash armazenado</param>
        /// <returns>True se a senha estiver correta</returns>
        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}

