using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HermesBanking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DetailsCreditCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MerchantName",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantName",
                table: "Transactions");
        }
    }
}
