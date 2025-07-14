using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HermesBanking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Estas columnas ya existen, por lo que no las agregamos de nuevo.
            //migrationBuilder.AddColumn<string>(
            //    name: "AdminFullName",
            //    table: "SavingsAccounts",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "ClientFullName",
            //    table: "SavingsAccounts",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminFullName",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "ClientFullName",
                table: "SavingsAccounts");
        }
    }
}
