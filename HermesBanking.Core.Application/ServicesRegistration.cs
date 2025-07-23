using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.Services;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HermesBanking.Core.Application
{
    public static class ServicesRegistration
    {
        //Extension method - Decorator pattern
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Configurations
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            #endregion

            #region Services IOC
            services.AddScoped<ISavingsAccountService, SavingsAccountService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            services.AddScoped<ICashAdvanceService, CashAdvanceService>();
            services.AddScoped<ITransferService, TransferService>();

            services.AddScoped<ICashierService, CashierService>();
            services.AddScoped<ICashierTransactionService, CashierTransactionService>();
            services.AddScoped<ILoanAmortizationService, LoanAmortizationService>();
            #endregion
        }
    }
}
