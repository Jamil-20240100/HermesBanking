using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HermesBanking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CommerceIdTransactionAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommerceId",
                table: "Transactions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommerceId",
                table: "Transactions");
        }
    }
}
