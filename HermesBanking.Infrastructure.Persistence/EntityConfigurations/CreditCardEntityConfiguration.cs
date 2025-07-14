using HermesBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesBanking.Infrastructure.Persistence.EntityConfigurations
{
    public class CreditCardEntityConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            #region Basic Configuration
            builder.ToTable("CreditCards");
            builder.HasKey(cc => cc.Id);
            #endregion

            #region Properties Configuration

            builder.Property(cc => cc.CardId)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(cc => cc.ClientId)
                .IsRequired();

            builder.Property(cc => cc.ClientFullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(cc => cc.CreditLimit)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(cc => cc.TotalOwedAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(cc => cc.CVC)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(cc => cc.IsActive)
                .IsRequired();

            builder.Property(cc => cc.ExpirationDate)
                .IsRequired();

            builder.Property(cc => cc.CreatedAt)
                .IsRequired();

            builder.Property(cc => cc.CreatedByAdminId)
                .HasMaxLength(450);

            builder.Property(cc => cc.AdminFullName)
                .HasMaxLength(100);

            #endregion
        }
    }
}
