using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core
{
    internal class BasicCrud<TSource, TId> : IBasicCrud<TSource, TId> where TSource : class, IEntity<TId>
    {
        private readonly IEntityProfile<TSource, TId> _profile;
        
        private readonly DbContext _dbContext;

        private readonly DbSet<TSource> _dbSet;

        public BasicCrud(IEntityProfile<TSource, TId> profile, DbContext dbContext)
        {
            _profile = profile;
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TSource>();
        }
        
        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TSource>> GetAll()
        {
            return await _profile.Include(_dbSet).ToListAsync();
        }

        /// <summary>
        /// Returns an entity given the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Get(TId id)
        {
            return await _profile.Include(_dbSet).FirstOrDefaultAsync(x => Equals(x.Id, id));
        }

        /// <summary>
        /// Saves an instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Save(TSource instance)
        {
            await _dbSet.AddAsync(instance);
            
            await _dbContext.SaveChangesAsync();
            
            return instance;
        }

        /// <summary>
        /// Deletes entity given the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Delete(TId id)
        {
            var entity = await Get(id);

            if (entity != null)
            {
                _dbSet.Remove(entity);
                
                await _dbContext.SaveChangesAsync();
                
                return entity;
            }

            return null;
        }

        /// <summary>
        /// Updates entity given the id and new instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Update(TId id, TSource dto)
        {
            var entity = await Get(id);

            if (entity != null)
            {
                var result = _profile.Update(entity, dto);
                
                await _dbContext.SaveChangesAsync();

                return result;
            }

            return null;
        }
    }
}