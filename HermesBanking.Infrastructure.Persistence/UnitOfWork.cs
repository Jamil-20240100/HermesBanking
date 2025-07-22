using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Storage;


namespace HermesBanking.Infrastructure.Persistence
{

    public class UnitOfWork : IUnitOfWork
    {
        private readonly HermesBankingContext _dbContext;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(HermesBankingContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IDisposable BeginTransaction()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("Ya hay una transacción en progreso.");
            }
            _currentTransaction = _dbContext.Database.BeginTransaction();
            return _currentTransaction;
        }

        public async Task CommitAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No hay una transacción en progreso para confirmar.");
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                _currentTransaction.Commit();
            }
            catch
            {
                _currentTransaction.Rollback();
                throw;
            }
            finally
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No hay una transacción en progreso para revertir.");
            }

            try
            {
                _currentTransaction.Rollback();
            }
            finally
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}