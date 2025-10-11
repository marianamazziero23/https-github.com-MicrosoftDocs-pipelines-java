using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESGSustainabilityAPI.Models
{
    /// <summary>
    /// Representa o consumo de energia de uma empresa
    /// </summary>
    public class EnergyConsumption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EnergyType { get; set; } = string.Empty; // Elétrica, Gás, Solar, etc.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ConsumptionAmount { get; set; }

        [Required]
        [StringLength(10)]
        public string Unit { get; set; } = "kWh"; // kWh, m³, etc.

        [Required]
        public DateTime RecordDate { get; set; }

        [StringLength(100)]
        public string Source { get; set; } = string.Empty; // Fonte da energia

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [StringLength(10)]
        public string? CostCurrency { get; set; } = "BRL";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RenewablePercentage { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

