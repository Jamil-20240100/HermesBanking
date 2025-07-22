namespace HermesBanking.Core.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDisposable BeginTransaction();
        Task CommitAsync();
        Task RollbackAsync();
    }
}