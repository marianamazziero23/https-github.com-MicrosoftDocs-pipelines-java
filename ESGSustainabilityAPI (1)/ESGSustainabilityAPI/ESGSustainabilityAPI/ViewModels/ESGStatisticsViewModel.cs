namespace ESGSustainabilityAPI.ViewModels
{
    /// <summary>
    /// ViewModel para estatísticas ESG do dashboard
    /// </summary>
    public class ESGStatisticsViewModel
    {
        public decimal TotalCarbonEmissions { get; set; }
        public decimal TotalEnergyConsumption { get; set; }
        public decimal AverageRenewablePercentage { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalReports { get; set; }
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, decimal> EmissionsByCategory { get; set; } = new();
        public Dictionary<string, decimal> EnergyByType { get; set; } = new();
    }
}

