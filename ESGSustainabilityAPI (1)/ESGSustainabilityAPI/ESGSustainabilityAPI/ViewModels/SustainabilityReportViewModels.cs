using System.ComponentModel.DataAnnotations;

namespace ESGSustainabilityAPI.ViewModels
{
    /// <summary>
    /// ViewModel para criação de relatório de sustentabilidade
    /// </summary>
    public class CreateSustainabilityReportViewModel
    {
        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ano é obrigatório")]
        [Range(2000, 2100, ErrorMessage = "O ano deve estar entre 2000 e 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "O trimestre é obrigatório")]
        [Range(1, 4, ErrorMessage = "O trimestre deve estar entre 1 e 4")]
        public int Quarter { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "As emissões de carbono devem ser maior ou igual a zero")]
        public decimal TotalCarbonEmissions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O consumo de energia deve ser maior ou igual a zero")]
        public decimal TotalEnergyConsumption { get; set; }

        [Range(0, 100, ErrorMessage = "A porcentagem de energia renovável deve estar entre 0 e 100")]
        public decimal RenewableEnergyPercentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O consumo de água deve ser maior ou igual a zero")]
        public decimal WaterConsumption { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Os resíduos gerados devem ser maior ou igual a zero")]
        public decimal WasteGenerated { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Os resíduos reciclados devem ser maior ou igual a zero")]
        public decimal WasteRecycled { get; set; }

        [StringLength(50, ErrorMessage = "A pontuação ESG deve ter no máximo 50 caracteres")]
        public string ESGScore { get; set; } = string.Empty;

        public string EnvironmentalInitiatives { get; set; } = string.Empty;
        public string SocialInitiatives { get; set; } = string.Empty;
        public string GovernanceInitiatives { get; set; } = string.Empty;
        public string Challenges { get; set; } = string.Empty;
        public string FutureGoals { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ID da empresa é obrigatório")]
        public int CompanyId { get; set; }
    }

    /// <summary>
    /// ViewModel para listagem de relatórios de sustentabilidade
    /// </summary>
    public class SustainabilityReportListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal TotalCarbonEmissions { get; set; }
        public decimal TotalEnergyConsumption { get; set; }
        public decimal RenewableEnergyPercentage { get; set; }
        public string ESGScore { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel para detalhes de relatório de sustentabilidade
    /// </summary>
    public class SustainabilityReportDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal TotalCarbonEmissions { get; set; }
        public decimal TotalEnergyConsumption { get; set; }
        public decimal RenewableEnergyPercentage { get; set; }
        public decimal WaterConsumption { get; set; }
        public decimal WasteGenerated { get; set; }
        public decimal WasteRecycled { get; set; }
        public string ESGScore { get; set; } = string.Empty;
        public string EnvironmentalInitiatives { get; set; } = string.Empty;
        public string SocialInitiatives { get; set; } = string.Empty;
        public string GovernanceInitiatives { get; set; } = string.Empty;
        public string Challenges { get; set; } = string.Empty;
        public string FutureGoals { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

