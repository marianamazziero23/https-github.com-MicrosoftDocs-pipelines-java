using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ESGSustainabilityAPI.Models
{
    /// <summary>
    /// Representa uma emissão de carbono registrada no sistema
    /// </summary>
    public class CarbonEmission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Source { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EmissionAmount { get; set; }

        [Required]
        [StringLength(10)]
        public string Unit { get; set; } = "tCO2e"; // toneladas de CO2 equivalente

        [Required]
        public DateTime RecordDate { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Escopo 1, 2 ou 3

        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

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

