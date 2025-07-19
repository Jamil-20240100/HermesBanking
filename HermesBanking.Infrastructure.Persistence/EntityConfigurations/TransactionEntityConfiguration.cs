
using HermesBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesBanking.Infrastructure.Persistence.EntityConfigurations
{
    public class TransactionEntityConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type).IsRequired().HasMaxLength(20);
            builder.Property(t => t.Origin).IsRequired();
            builder.Property(t => t.Beneficiary).IsRequired();
            builder.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(t => t.Date).IsRequired();
            builder.Property(t => t.PerformedByCashierId).IsRequired(false);

            builder.HasOne(t => t.SavingsAccount)
                .WithMany()
                .HasForeignKey(t => t.SavingsAccountId);
        }
    }
}
