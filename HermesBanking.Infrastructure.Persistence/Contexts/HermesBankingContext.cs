using HermesBanking.Core.Domain.Entities;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HermesBanking.Infrastructure.Persistence.Contexts
{
    public class HermesBankingContext : DbContext
    {
        public HermesBankingContext(DbContextOptions<HermesBankingContext> options) : base(options) { }

        //
        // DB SETS
        //

        public DbSet<SavingsAccount> SavingsAccount { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<AmortizationInstallment> AmortizationInstallments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }

        //
        // ENTITY CONFIGURATIONS APPLICATION
        //

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.InitialAmount)
                .HasColumnType("decimal(18, 2)"); // Especifica precisión 18 y escala 2 para valores decimales

            modelBuilder.Entity<Loan>()
                .Property(l => l.RemainingDebt)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<SavingsAccount>()
                .Property(s => s.Balance)
                .HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)");
        }
    }
}