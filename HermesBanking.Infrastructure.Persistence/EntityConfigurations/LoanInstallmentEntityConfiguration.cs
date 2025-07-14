using HermesBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesBanking.Infrastructure.Persistence.EntityConfigurations
{
    public class LoanInstallmentEntityConfiguration : IEntityTypeConfiguration<LoanInstallment>
    {
        public void Configure(EntityTypeBuilder<LoanInstallment> builder)
        {
            builder.ToTable("LoanInstallments");
            builder.HasKey(li => li.Id);

            builder.Property(li => li.Amount).HasPrecision(18, 2);
            builder.Property(li => li.AmountPaid).HasPrecision(18, 2);
            builder.Property(li => li.DueDate).IsRequired();

            builder.HasOne(li => li.Loan)
                   .WithMany(l => l.Installments)
                   .HasForeignKey(li => li.LoanId);
        }
    }
}
