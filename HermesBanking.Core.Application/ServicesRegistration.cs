using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.Services;
using HermesBanking.Infrastructure.Application.Services;
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
           // services.AddScoped<ICashierService, CashierService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            #endregion
        }
    }
}
