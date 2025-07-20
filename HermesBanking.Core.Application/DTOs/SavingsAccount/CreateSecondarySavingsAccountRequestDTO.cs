namespace HermesBanking.Core.Application.DTOs.SavingsAccount
{
    public class CreateSecondarySavingsAccountRequestDTO
    {
        public string CedulaCliente { get; set; } = string.Empty;
        public decimal BalanceInicial { get; set; }
    }
}
