using System.ComponentModel.DataAnnotations;

namespace ESGSustainabilityAPI.ViewModels
{
    /// <summary>
    /// ViewModel para criação de consumo de energia
    /// </summary>
    public class CreateEnergyConsumptionViewModel
    {
        [Required(ErrorMessage = "O tipo de energia é obrigatório")]
        [StringLength(50, ErrorMessage = "O tipo de energia deve ter no máximo 50 caracteres")]
        public string EnergyType { get; set; } = string.Empty;

        [Required(ErrorMessage = "A quantidade de consumo é obrigatória")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal ConsumptionAmount { get; set; }

        [Required(ErrorMessage = "A unidade é obrigatória")]
        [StringLength(10, ErrorMessage = "A unidade deve ter no máximo 10 caracteres")]
        public string Unit { get; set; } = "kWh";

        [Required(ErrorMessage = "A data de registro é obrigatória")]
        public DateTime RecordDate { get; set; }

        [StringLength(100, ErrorMessage = "A fonte deve ter no máximo 100 caracteres")]
        public string Source { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "O custo deve ser maior ou igual a zero")]
        public decimal? Cost { get; set; }

        [StringLength(10, ErrorMessage = "A moeda deve ter no máximo 10 caracteres")]
        public string? CostCurrency { get; set; } = "BRL";

        [Range(0, 100, ErrorMessage = "A porcentagem renovável deve estar entre 0 e 100")]
        public decimal? RenewablePercentage { get; set; }

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ID da empresa é obrigatório")]
        public int CompanyId { get; set; }
    }

    /// <summary>
    /// ViewModel para listagem de consumo de energia
    /// </summary>
    public class EnergyConsumptionListViewModel
    {
        public int Id { get; set; }
        public string EnergyType { get; set; } = string.Empty;
        public decimal ConsumptionAmount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal? Cost { get; set; }
        public decimal? RenewablePercentage { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel para detalhes de consumo de energia
    /// </summary>
    public class EnergyConsumptionDetailViewModel
    {
        public int Id { get; set; }
        public string EnergyType { get; set; } = string.Empty;
        public decimal ConsumptionAmount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public decimal? Cost { get; set; }
        public string? CostCurrency { get; set; }
        public decimal? RenewablePercentage { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

