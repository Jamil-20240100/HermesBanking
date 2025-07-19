using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HermesBanking.Infrastructure.Persistence.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersistenceMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beneficiaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BeneficiaryAccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOwedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CVC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByAdminId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AdminFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanIdentifier = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ClientFullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanTermMonths = table.Column<int>(type: "int", nullable: false),
                    MonthlyInstallmentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalInstallments = table.Column<int>(type: "int", nullable: false),
                    PaidInstallments = table.Column<int>(type: "int", nullable: false),
                    PendingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOverdue = table.Column<bool>(type: "bit", nullable: false),
                    AssignedByAdminId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AdminFullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavingsAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientFullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByAdminId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminFullName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AmortizationInstallments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanId = table.Column<int>(type: "int", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InstallmentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    IsOverdue = table.Column<bool>(type: "bit", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmortizationInstallments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmortizationInstallments_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beneficiary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SavingsAccountId = table.Column<int>(type: "int", nullable: true),
                    PerformedByCashierId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    CreditCardId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CashierId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_SavingsAccount_SavingsAccountId",
                        column: x => x.SavingsAccountId,
                        principalTable: "SavingsAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmortizationInstallments_LoanId",
                table: "AmortizationInstallments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SavingsAccountId",
                table: "Transactions",
                column: "SavingsAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmortizationInstallments");

            migrationBuilder.DropTable(
                name: "Beneficiaries");

            migrationBuilder.DropTable(
                name: "CreditCards");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "SavingsAccount");
        }
    }
}
