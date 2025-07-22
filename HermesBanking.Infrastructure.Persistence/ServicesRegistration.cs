using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;
using HermesBanking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HermesBanking.Infrastructure.Persistence
{
    public static class ServicesRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<HermesBankingContext>(opt =>
                                              opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("DefaultConnection");
                services.AddDbContext<HermesBankingContext>(
                    (serviceProvider, opt) =>
                    {
                        opt.EnableSensitiveDataLogging();
                        opt.UseSqlServer(connectionString,
                        m => m.MigrationsAssembly(typeof(HermesBankingContext).Assembly.FullName));
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                    );
                #endregion

                #region repositories IOC
                services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                services.AddScoped<ISavingsAccountRepository, SavingsAccountRepository>();
                services.AddScoped<ITransactionRepository, TransactionRepository>();
                services.AddScoped<ICreditCardRepository, CreditCardRepository>();
                services.AddScoped<ILoanRepository, LoanRepository>();
                services.AddScoped<IAmortizationInstallmentRepository, AmortizationLInstallmentRepository>();
                services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                #endregion
            }
        }
    }
}