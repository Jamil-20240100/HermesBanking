using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HermesBanking.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class UserLogicFixingMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                schema: "Identity",
                table: "Users");

            migrationBuilder.AddColumn<double>(
                name: "InitialAmount",
                schema: "Identity",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "Identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialAmount",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Identity",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                schema: "Identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
