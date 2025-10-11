using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESGSustainabilityAPI.Models
{
    /// <summary>
    /// Representa um relatório de sustentabilidade
    /// </summary>
    public class SustainabilityReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [Required]
        public int Quarter { get; set; } // 1, 2, 3, 4

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCarbonEmissions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEnergyConsumption { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RenewableEnergyPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WaterConsumption { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WasteGenerated { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WasteRecycled { get; set; }

        [StringLength(50)]
        public string ESGScore { get; set; } = string.Empty; // A, B, C, D, E

        [Column(TypeName = "text")]
        public string EnvironmentalInitiatives { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string SocialInitiatives { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string GovernanceInitiatives { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string Challenges { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string FutureGoals { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

