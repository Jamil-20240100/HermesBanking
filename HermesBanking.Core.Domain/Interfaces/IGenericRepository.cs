using System.Linq.Expressions;

namespace HermesBanking.Core.Domain.Interfaces
{
    public interface IGenericRepository<Entity> where Entity : class
    {
        // BASIC CRUD
        Task<Entity?> AddAsync(Entity entity);
        Task<Entity?> UpdateAsync(Entity entity);
        Task<Entity?> UpdateAsync(int Id, Entity entity);
        Task DeleteAsync(Entity entity);
        Task DeleteAsync(int? Id);

        // NAVIGATION METHODS
        Task<Entity?> GetById(int id);
        Task<List<Entity>> GetAll();
        Task<List<Entity>> GetAllWithInclude(List<string> properties);
        IQueryable<Entity> GetAllQuery();
        IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);

        Task<IEnumerable<Entity>> GetByConditionAsync(Expression<Func<Entity, bool>> expression);
    }
}