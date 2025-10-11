using System.ComponentModel.DataAnnotations;

namespace ESGSustainabilityAPI.Models
{
    /// <summary>
    /// Representa uma empresa no sistema ESG
    /// </summary>
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CNPJ { get; set; } = string.Empty;

        [StringLength(100)]
        public string Industry { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [StringLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        public int EmployeeCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        public virtual ICollection<CarbonEmission> CarbonEmissions { get; set; } = new List<CarbonEmission>();
        public virtual ICollection<EnergyConsumption> EnergyConsumptions { get; set; } = new List<EnergyConsumption>();
        public virtual ICollection<SustainabilityReport> SustainabilityReports { get; set; } = new List<SustainabilityReport>();
    }
}

