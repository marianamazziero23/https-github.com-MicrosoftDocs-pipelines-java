using System.ComponentModel.DataAnnotations;

namespace ESGSustainabilityAPI.ViewModels
{
    /// <summary>
    /// ViewModel para login de usuário
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username é obrigatório")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel para registro de usuário
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username é obrigatório")]
        [StringLength(100, ErrorMessage = "Username deve ter no máximo 100 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password é obrigatório")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password deve ter entre 6 e 100 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "FirstName é obrigatório")]
        [StringLength(100, ErrorMessage = "FirstName deve ter no máximo 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "LastName é obrigatório")]
        [StringLength(100, ErrorMessage = "LastName deve ter no máximo 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        public int? CompanyId { get; set; }
    }

    /// <summary>
    /// ViewModel para resposta de autenticação
    /// </summary>
    public class AuthResponseViewModel
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfoViewModel User { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para informações do usuário
    /// </summary>
    public class UserInfoViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    /// <summary>
    /// ViewModel para alteração de senha
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Senha atual é obrigatória")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Nova senha deve ter entre 6 e 100 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("NewPassword", ErrorMessage = "Nova senha e confirmação devem ser iguais")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

