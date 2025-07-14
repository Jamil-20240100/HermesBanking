using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Application.ViewModels.SavingsAccount
{
    public class SavingsAccountViewModel
    {
        //main info
        public int Id { get; set; }
        public required string AccountNumber { get; set; }
        public required decimal Balance { get; set; }
        public required AccountType AccountType { get; set; }
        public required bool IsActive { get; set; }

        //additional info
        public required DateTime CreatedAt { get; set; }
        public required string ClientId { get; set; }
        public required string ClientFullName { get; set; }
        public string? ClientUserId { get; set; }

        //only for secondary accounts info
        public string? CreatedByAdminId { get; set; }
        public string? AdminFullName { get; set; }
    }
}
