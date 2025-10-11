using System.ComponentModel.DataAnnotations;

namespace ESGSustainabilityAPI.ViewModels
{
    /// <summary>
    /// ViewModel para criação de emissão de carbono
    /// </summary>
    public class CreateCarbonEmissionViewModel
    {
        [Required(ErrorMessage = "A fonte da emissão é obrigatória")]
        [StringLength(100, ErrorMessage = "A fonte deve ter no máximo 100 caracteres")]
        public string Source { get; set; } = string.Empty;

        [Required(ErrorMessage = "A quantidade de emissão é obrigatória")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal EmissionAmount { get; set; }

        [Required(ErrorMessage = "A unidade é obrigatória")]
        [StringLength(10, ErrorMessage = "A unidade deve ter no máximo 10 caracteres")]
        public string Unit { get; set; } = "tCO2e";

        [Required(ErrorMessage = "A data de registro é obrigatória")]
        public DateTime RecordDate { get; set; }

        [StringLength(50, ErrorMessage = "A categoria deve ter no máximo 50 caracteres")]
        public string Category { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "A localização deve ter no máximo 100 caracteres")]
        public string Location { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ID da empresa é obrigatório")]
        public int CompanyId { get; set; }
    }

    /// <summary>
    /// ViewModel para listagem de emissões de carbono
    /// </summary>
    public class CarbonEmissionListViewModel
    {
        public int Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal EmissionAmount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel para detalhes de emissão de carbono
    /// </summary>
    public class CarbonEmissionDetailViewModel
    {
        public int Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal EmissionAmount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

