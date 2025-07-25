// HermesBanking.Core.Domain.Common.Enums/TransactionType.cs (crea esta ruta si no existe)
namespace HermesBanking.Core.Domain.Common.Enums
{
    public enum TransactionType
    {
        Transferencia = 1,
        Deposito = 2,
        Retiro = 3,
        PagoTarjetaCredito = 4,
        PagoPrestamo = 5,
        PagoBeneficiario = 6,
        DesembolsoPrestamo = 7,
        AVANCE = 8,
        CREDITO,
        DEBITO
    }
}