namespace HermesBanking.Core.Domain.Common.Enums
{
    public enum TransactionType
    {
        [System.ComponentModel.Description("DEPÓSITO")]
        Deposit,

        [System.ComponentModel.Description("RETIRO")]
        Withdraw,

        [System.ComponentModel.Description("TRANSFERENCIA")]
        Transfer,

        [System.ComponentModel.Description("PAGO DE PRÉSTAMO")]
        LoanPayment,

        [System.ComponentModel.Description("PAGO DE TARJETA")]
        CreditCardPayment
    }
}
