using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESGSustainabilityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSQLite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CNPJ = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Industry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EmployeeCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarbonEmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    RecordDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarbonEmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarbonEmissions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnergyConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EnergyType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ConsumptionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    RecordDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CostCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    RenewablePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyConsumptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnergyConsumptions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SustainabilityReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Quarter = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCarbonEmissions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEnergyConsumption = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RenewableEnergyPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaterConsumption = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WasteGenerated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WasteRecycled = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ESGScore = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EnvironmentalInitiatives = table.Column<string>(type: "text", nullable: false),
                    SocialInitiatives = table.Column<string>(type: "text", nullable: false),
                    GovernanceInitiatives = table.Column<string>(type: "text", nullable: false),
                    Challenges = table.Column<string>(type: "text", nullable: false),
                    FutureGoals = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SustainabilityReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SustainabilityReports_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "CNPJ", "City", "ContactEmail", "ContactPhone", "CreatedAt", "EmployeeCount", "Industry", "Name", "State", "UpdatedAt", "ZipCode" },
                values: new object[] { 1, "Av. Paulista, 1000", "12.345.678/0001-90", "São Paulo", "contato@ecotech.com.br", "(11) 99999-9999", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 150, "Tecnologia", "EcoTech Solutions Ltda", "SP", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "01310-100" });

            migrationBuilder.InsertData(
                table: "CarbonEmissions",
                columns: new[] { "Id", "Category", "CompanyId", "CreatedAt", "Description", "EmissionAmount", "Location", "RecordDate", "Source", "Unit", "UpdatedAt" },
                values: new object[] { 1, "Escopo 2", 1, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Emissões provenientes do consumo de energia elétrica do escritório principal", 15.5m, "São Paulo - SP", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Energia Elétrica", "tCO2e", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "EnergyConsumptions",
                columns: new[] { "Id", "CompanyId", "ConsumptionAmount", "Cost", "CostCurrency", "CreatedAt", "Description", "EnergyType", "RecordDate", "RenewablePercentage", "Source", "Unit", "UpdatedAt" },
                values: new object[] { 1, 1, 2500.0m, 1250.0m, "BRL", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Consumo mensal do escritório principal", "Elétrica", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35.0m, "Rede Elétrica", "kWh", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_CarbonEmissions_Category",
                table: "CarbonEmissions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CarbonEmissions_CompanyId",
                table: "CarbonEmissions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CarbonEmissions_RecordDate",
                table: "CarbonEmissions",
                column: "RecordDate");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CNPJ",
                table: "Companies",
                column: "CNPJ",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnergyConsumptions_CompanyId",
                table: "EnergyConsumptions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyConsumptions_EnergyType",
                table: "EnergyConsumptions",
                column: "EnergyType");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyConsumptions_RecordDate",
                table: "EnergyConsumptions",
                column: "RecordDate");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityReports_CompanyId",
                table: "SustainabilityReports",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityReports_ESGScore",
                table: "SustainabilityReports",
                column: "ESGScore");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityReports_Year_Quarter",
                table: "SustainabilityReports",
                columns: new[] { "Year", "Quarter" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarbonEmissions");

            migrationBuilder.DropTable(
                name: "EnergyConsumptions");

            migrationBuilder.DropTable(
                name: "SustainabilityReports");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
