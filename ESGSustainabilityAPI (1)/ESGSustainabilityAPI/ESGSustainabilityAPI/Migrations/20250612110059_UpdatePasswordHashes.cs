using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESGSustainabilityAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$/sE5A5UoHQd8paczSIJgjelIVrZjIhoxnPAITJlxB2bKLcmJ5fvSq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$mQaOGi34WyMUorJCpcf3bOsik7fwnUHVP6hr6VaQRp4bYmiUdw9O.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$S6aUyIVe9V4NRrclikBdaOf6VppRdPg.0Qh/14aTpHMXmG52n6cGK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$8YQjQKJZKJZKJZKJZKJZKOeKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$8YQjQKJZKJZKJZKJZKJZKOeKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$8YQjQKJZKJZKJZKJZKJZKOeKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK");
        }
    }
}
