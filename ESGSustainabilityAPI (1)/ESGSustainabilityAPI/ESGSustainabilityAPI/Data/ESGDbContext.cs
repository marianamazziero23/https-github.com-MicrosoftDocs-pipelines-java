using Microsoft.EntityFrameworkCore;
using ESGSustainabilityAPI.Models;

namespace ESGSustainabilityAPI.Data
{
    /// <summary>
    /// Contexto do banco de dados para o sistema ESG
    /// </summary>
    public class ESGDbContext : DbContext
    {
        public ESGDbContext(DbContextOptions<ESGDbContext> options) : base(options)
        {
        }

        // DbSets para as entidades
        public DbSet<Company> Companies { get; set; }
        public DbSet<CarbonEmission> CarbonEmissions { get; set; }
        public DbSet<EnergyConsumption> EnergyConsumptions { get; set; }
        public DbSet<SustainabilityReport> SustainabilityReports { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da entidade Company
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CNPJ).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.CNPJ).IsUnique();
                entity.Property(e => e.Industry).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(10);
                entity.Property(e => e.ContactEmail).HasMaxLength(100);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
            });

            // Configuração da entidade CarbonEmission
            modelBuilder.Entity<CarbonEmission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmissionAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Company)
                      .WithMany(c => c.CarbonEmissions)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.RecordDate);
                entity.HasIndex(e => e.Category);
            });

            // Configuração da entidade EnergyConsumption
            modelBuilder.Entity<EnergyConsumption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EnergyType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ConsumptionAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Source).HasMaxLength(100);
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostCurrency).HasMaxLength(10);
                entity.Property(e => e.RenewablePercentage).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Company)
                      .WithMany(c => c.EnergyConsumptions)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.RecordDate);
                entity.HasIndex(e => e.EnergyType);
            });

            // Configuração da entidade SustainabilityReport
            modelBuilder.Entity<SustainabilityReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TotalCarbonEmissions).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalEnergyConsumption).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RenewableEnergyPercentage).HasColumnType("decimal(18,2)");
                entity.Property(e => e.WaterConsumption).HasColumnType("decimal(18,2)");
                entity.Property(e => e.WasteGenerated).HasColumnType("decimal(18,2)");
                entity.Property(e => e.WasteRecycled).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ESGScore).HasMaxLength(50);
                entity.Property(e => e.EnvironmentalInitiatives).HasColumnType("text");
                entity.Property(e => e.SocialInitiatives).HasColumnType("text");
                entity.Property(e => e.GovernanceInitiatives).HasColumnType("text");
                entity.Property(e => e.Challenges).HasColumnType("text");
                entity.Property(e => e.FutureGoals).HasColumnType("text");

                entity.HasOne(e => e.Company)
                      .WithMany(c => c.SustainabilityReports)
                      .HasForeignKey(e => e.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.Year, e.Quarter });
                entity.HasIndex(e => e.ESGScore);
            });

            // Configuração da entidade User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
                entity.Property(u => u.IsActive).IsRequired();
                entity.Property(u => u.CreatedAt).IsRequired();
                entity.Property(u => u.UpdatedAt).IsRequired();

                // Índices
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Role);
                entity.HasIndex(u => u.IsActive);

                // Relacionamento com Company
                entity.HasOne(u => u.Company)
                      .WithMany()
                      .HasForeignKey(u => u.CompanyId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Dados iniciais (Seed Data)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Empresa exemplo
            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                    Id = 1,
                    Name = "EcoTech Solutions Ltda",
                    CNPJ = "12.345.678/0001-90",
                    Industry = "Tecnologia",
                    Address = "Av. Paulista, 1000",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01310-100",
                    ContactEmail = "contato@ecotech.com.br",
                    ContactPhone = "(11) 99999-9999",
                    EmployeeCount = 150,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Emissão de carbono exemplo
            modelBuilder.Entity<CarbonEmission>().HasData(
                new CarbonEmission
                {
                    Id = 1,
                    Source = "Energia Elétrica",
                    EmissionAmount = 15.5m,
                    Unit = "tCO2e",
                    RecordDate = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                    Category = "Escopo 2",
                    Location = "São Paulo - SP",
                    Description = "Emissões provenientes do consumo de energia elétrica do escritório principal",
                    CompanyId = 1,
                    CreatedAt = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Consumo de energia exemplo
            modelBuilder.Entity<EnergyConsumption>().HasData(
                new EnergyConsumption
                {
                    Id = 1,
                    EnergyType = "Elétrica",
                    ConsumptionAmount = 2500.0m,
                    Unit = "kWh",
                    RecordDate = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                    Source = "Rede Elétrica",
                    Cost = 1250.0m,
                    CostCurrency = "BRL",
                    RenewablePercentage = 35.0m,
                    Description = "Consumo mensal do escritório principal",
                    CompanyId = 1,
                    CreatedAt = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Usuários exemplo
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@ecotech.com.br",
                    PasswordHash = "$2a$11$/sE5A5UoHQd8paczSIJgjelIVrZjIhoxnPAITJlxB2bKLcmJ5fvSq", // senha: admin123
                    FirstName = "Administrador",
                    LastName = "Sistema",
                    Role = "Admin",
                    CompanyId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Username = "manager",
                    Email = "manager@ecotech.com.br",
                    PasswordHash = "$2a$11$mQaOGi34WyMUorJCpcf3bOsik7fwnUHVP6hr6VaQRp4bYmiUdw9O.", // senha: manager123
                    FirstName = "Gerente",
                    LastName = "ESG",
                    Role = "Manager",
                    CompanyId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 3,
                    Username = "user",
                    Email = "user@ecotech.com.br",
                    PasswordHash = "$2a$11$S6aUyIVe9V4NRrclikBdaOf6VppRdPg.0Qh/14aTpHMXmG52n6cGK", // senha: user123
                    FirstName = "Usuário",
                    LastName = "Padrão",
                    Role = "User",
                    CompanyId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}

