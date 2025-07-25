using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.ViewModels.SavingsAccount
{
    public class SavingsAccountDetailsViewModel
    {
            //main info
            public required string Id { get; set; }
            public required string AccountNumber { get; set; }
            public required decimal Balance { get; set; }

            //additional info
            public DateTime TransactionDate { get; set; }
            public string? ClientUserId { get; set; }



            public TransactionType? TransactionType { get; set; }

            public List<DisplayTransactionDTO> Transactions { get; set; }

            public PaginationDTO Pagination { get; set; }


    }
    }

