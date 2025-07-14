using HermesBanking.Core.Domain.Entities;
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
        public DbSet<LoanInstallment> LoanInstallments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }


        //
        // ENTITY CONFIGURATIONS APPLICATION
        //

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
