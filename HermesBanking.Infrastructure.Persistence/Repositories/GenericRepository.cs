using Microsoft.EntityFrameworkCore;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<Entity> : IGenericRepository<Entity> where Entity : class
    {
        private readonly HermesBankingContext _context;

        public GenericRepository(HermesBankingContext context)
        {
            _context = context;
        }

        //METHODS IMPLEMENTATION

        public virtual async Task<Entity?> AddAsync(Entity entity)
        {
            await _context.Set<Entity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<Entity?> UpdateAsync(int id, Entity entity)
        {
            var entry = await _context.Set<Entity>().FindAsync(id);

            if (entry != null)
            {
                _context.Entry(entry).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                return entry;
            }
            return null;
        }

        public virtual async Task DeleteAsync(int? id)
        {
            var entity = await _context.Set<Entity>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<Entity>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<Entity?> GetById(int id)
        {
            return await _context.Set<Entity>().FindAsync(id);
        }

        public virtual async Task<List<Entity>> GetAll()
        {
            return await _context.Set<Entity>().ToListAsync();
        }

        public virtual async Task<List<Entity>> GetAllWithInclude(List<string> properties)
        {
            var query = _context.Set<Entity>().AsQueryable();

            foreach (var property in properties)
            {
                query = query.Include(property);
            }

            return await query.ToListAsync();
        }

        public virtual IQueryable<Entity> GetAllQuery()
        {
            return _context.Set<Entity>().AsQueryable();
        }

        public virtual IQueryable<Entity> GetAllQueryWithInclude(List<string> properties)
        {
            var query = _context.Set<Entity>().AsQueryable();

            foreach (var property in properties)
            {
                query = query.Include(property);
            }
            return query;
        }
    }
}
