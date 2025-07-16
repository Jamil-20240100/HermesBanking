using Hangfire;
using HermesBanking.Core.Application.Interfaces;

namespace HermesBanking.Infrastructure.Shared.Services
{
    public class BackgroundTaskScheduler : IBackgroundTaskScheduler
    {
        public void ScheduleDailyInstallmentCheck()
        {
            RecurringJob.AddOrUpdate<ILoanService>(
                "CheckOverdueInstallments",
                service => service.CheckOverdueInstallmentsAsync(),
                Cron.Daily);
        }
    }
}