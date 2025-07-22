using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HermesBanking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class duedatemigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "AmortizationInstallments",
                newName: "DueDate");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "AmortizationInstallments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "AmortizationInstallments");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "AmortizationInstallments",
                newName: "PaymentDate");
        }
    }
}
