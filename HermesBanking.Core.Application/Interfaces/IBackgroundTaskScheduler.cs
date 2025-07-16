namespace HermesBanking.Core.Application.Interfaces
{
    public interface IBackgroundTaskScheduler
    {
        void ScheduleDailyInstallmentCheck();
    }
}