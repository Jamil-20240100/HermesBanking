using HermesBanking.Core.Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.SavingsAccount
{
    public class SaveSavingsAccountViewModel
    {
        //main info
        public int Id { get; set; }
        public string? AccountNumber { get; set; }

        [Required(ErrorMessage = "You must add a balance")]
        public required decimal Balance { get; set; }
        public required AccountType AccountType { get; set; }
        public required bool IsActive { get; set; }

        //additional info
        public required DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "You must attach a client to the savings account")]
        public required string ClientId { get; set; }
        public required string ClientFullName { get; set; }

        //only for secondary accounts info
        public string? CreatedByAdminId { get; set; }
        public string? AdminFullName { get; set; }
    }
}
