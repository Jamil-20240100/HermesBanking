using HermesBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesBanking.Infrastructure.Persistence.EntityConfigurations
{
    public class SavingsAccountEntityConfiguration : IEntityTypeConfiguration<SavingsAccount>
    {
        public void Configure(EntityTypeBuilder<SavingsAccount> builder)
        {
            #region basic configuration
            builder.ToTable("SavingsAccounts");
            builder.HasKey(sa => sa.Id);
            #endregion

            #region properties configurations
            builder.Property(sa => sa.AccountNumber).IsRequired().HasMaxLength(9);
            builder.Property(sa => sa.Balance).IsRequired();
            builder.Property(sa => sa.AccountType).IsRequired();
            builder.Property(sa => sa.IsActive).IsRequired();
            builder.Property(sa => sa.ClientId).IsRequired();
            builder.Property(sa => sa.CreatedAt).IsRequired();
            builder.Property(sa => sa.ClientFullName).IsRequired();
            #endregion
        }
    }
}
