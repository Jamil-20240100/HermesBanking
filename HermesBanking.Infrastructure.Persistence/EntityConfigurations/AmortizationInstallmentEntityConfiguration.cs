using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Infrastructure.Persistence.Configurations
{
    public class AmortizationInstallmentEntityConfiguration : IEntityTypeConfiguration<AmortizationInstallment>
    {
        public void Configure(EntityTypeBuilder<AmortizationInstallment> builder)
        {
            builder.HasKey(ai => ai.Id);

            builder.Property(ai => ai.InstallmentNumber)
                   .IsRequired();

            builder.Property(ai => ai.DueDate)
                   .IsRequired();

            builder.Property(ai => ai.InstallmentValue)
                   .HasColumnType("decimal(18, 2)")
                   .IsRequired();

            builder.Property(ai => ai.PrincipalAmount)
                   .HasColumnType("decimal(18, 2)")
                   .IsRequired();

            builder.Property(ai => ai.InterestAmount)
                   .HasColumnType("decimal(18, 2)")
                   .IsRequired();

            builder.Property(ai => ai.RemainingBalance)
                   .HasColumnType("decimal(18, 2)")
                   .IsRequired();

            builder.Property(ai => ai.IsPaid)
                   .IsRequired();

            builder.Property(ai => ai.IsOverdue)
                   .IsRequired();

            builder.Property(ai => ai.PaidDate)
                   .IsRequired(false);

            builder.ToTable("AmortizationInstallments");
        }
    }
}