using HermesBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesBanking.Infrastructure.Persistence.EntityConfigurations
{
    public class BeneficiaryEntityConfiguration : IEntityTypeConfiguration<Beneficiary>
    {
        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {
            #region Basic Configuration
            builder.ToTable("Beneficiaries");
            builder.HasKey(b => b.Id);
            #endregion

            #region Properties Configuration
            builder.Property(b => b.ClientId)
                   .IsRequired();

            builder.Property(b => b.BeneficiaryAccountNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(b => b.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.CreatedAt)
                   .IsRequired();
            #endregion
        }
    }
}