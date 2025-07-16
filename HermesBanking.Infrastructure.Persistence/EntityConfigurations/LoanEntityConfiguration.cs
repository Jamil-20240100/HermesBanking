using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Infrastructure.Persistence.Configurations
{
    public class LoanEntityConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.LoanIdentifier)
                   .IsRequired()
                   .HasMaxLength(8);

            builder.Property(l => l.ClientId)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.Property(l => l.ClientFullName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(l => l.Amount)
                   .HasColumnType("decimal(18, 2)")
                   .IsRequired();

            builder.Property(l => l.InterestRate)
                   .HasColumnType("decimal(18, 2)")
                   .IsRequired();

            builder.Property(l => l.LoanTermMonths)
                   .IsRequired();

            builder.Property(l => l.MonthlyInstallmentValue)
                   .HasColumnType("decimal(18, 2)")
                   .IsRequired();

            builder.Property(l => l.TotalInstallments)
                   .IsRequired();

            builder.Property(l => l.PaidInstallments)
                   .IsRequired();

            builder.Property(l => l.PendingAmount)
                   .HasColumnType("decimal(18, 2)")
                   .IsRequired();

            builder.Property(l => l.IsActive)
                   .IsRequired();

            builder.Property(l => l.IsOverdue)
                   .IsRequired();

            builder.Property(l => l.AssignedByAdminId)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.Property(l => l.AdminFullName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(l => l.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(l => l.CompletedAt)
                   .IsRequired(false);

            builder.HasMany(l => l.AmortizationInstallments)
                   .WithOne(ai => ai.Loan)
                   .HasForeignKey(ai => ai.LoanId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Loans");
        }
    }
}