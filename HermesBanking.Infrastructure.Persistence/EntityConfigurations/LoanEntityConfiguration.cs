using HermesBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesBanking.Infrastructure.Persistence.EntityConfigurations
{
    public class LoanEntityConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.ToTable("Loans");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.LoanNumber)
                .IsRequired()
                .HasMaxLength(9);

            builder.Property(l => l.TotalAmount).HasPrecision(18, 2);
            builder.Property(l => l.RemainingAmount).HasPrecision(18, 2);
            builder.Property(l => l.IsCompleted).IsRequired();
            builder.Property(l => l.ClientId).IsRequired();
            builder.Property(l => l.CreatedAt).IsRequired();
        }
    }
}
