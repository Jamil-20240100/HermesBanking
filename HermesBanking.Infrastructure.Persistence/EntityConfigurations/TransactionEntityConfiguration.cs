// HermesBanking.Infrastructure.Persistence.EntityConfigurations/TransactionEntityConfiguration.cs
// ¡Versión corregida para coincidir con la entidad Transaction 100% nullable!

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

            builder.HasKey(t => t.Id); // Id siempre es la clave primaria y non-nullable

            // Configura Amount, que es un decimal (non-nullable por defecto)
            builder.Property(t => t.Amount)
                .IsRequired() // Se mantiene como requerido, ya que decimal no es nullable
                .HasColumnType("decimal(18,2)");

            // Todas las demás propiedades se configuran para ser opcionales en la base de datos,
            // ya que son nullable en la entidad.
            // Para string?, int?, DateTime? no necesitas .IsRequired() ni .HasConversion<string>() a menos que quieras
            // controlar el largo o el formato en la DB.

            builder.Property(t => t.SavingsAccountId)
                .HasMaxLength(100); // MaxLength es bueno para strings, incluso si son nullable

            builder.Property(t => t.Type)
                .HasMaxLength(50);

            builder.Property(t => t.Origin)
                .HasMaxLength(255);

            builder.Property(t => t.Beneficiary)
                .HasMaxLength(255);

            builder.Property(t => t.Date); // DateTime? no necesita más configuración

            builder.Property(t => t.CashierId)
                .HasMaxLength(100);

            builder.Property(t => t.SourceAccountId)
                .HasMaxLength(100);

            builder.Property(t => t.DestinationAccountId)
                .HasMaxLength(100);

            builder.Property(t => t.TransactionDate); // DateTime? no necesita más configuración

            builder.Property(t => t.DestinationLoanId); // int? no necesita más configuración

            // Para enums nullable, si quieres guardarlos como string, aún necesitas HasConversion.
            // Si son nullable, el valor NULL en la DB se mapeará a null en el objeto.
            builder.Property(t => t.TransactionType)
                .HasConversion<string>() // Opcional: si quieres almacenar el enum como string
                .HasMaxLength(50); // Asegura que la columna tenga un tamaño adecuado para el nombre del enum

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.DestinationCardId)
                .HasMaxLength(100);

            builder.Property(t => t.Status)
                .HasConversion<string>() // Opcional: si quieres almacenar el enum Status como string
                .HasMaxLength(50);

            builder.Property(t => t.CreditCardId); // int? no necesita más configuración

            // Si tenías relaciones de navegación con .HasOne().WithMany().HasForeignKey(),
            // asegúrate de que esas propiedades existan en la entidad y que los Foreign Keys sean nullable
            // si la relación es opcional. Por ahora, las dejo fuera.
        }
    }
}