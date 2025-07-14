using HermesBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesBanking.Infrastructure.Persistence.EntityConfigurations
{
    public class CreditCardEntityConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            builder.ToTable("CreditCards");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CardNumber)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(c => c.Balance)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(c => c.IsActive).IsRequired();
            builder.Property(c => c.CreatedAt).IsRequired();
            builder.Property(c => c.ClientId).IsRequired();
        }
    }
}
