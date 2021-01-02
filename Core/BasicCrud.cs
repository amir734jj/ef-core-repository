using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EfCoreRepository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EfCoreRepository
{
    internal class BasicCrud<TSource> : IBasicCrudWrapper<TSource>, IBasicCrudSession<TSource> where TSource : class, IUntypedEntity
    {
        private readonly IEntityProfile<TSource> _profile;
        
        private readonly DbContext _dbContext;
        
        private readonly bool _outerSession;

        private readonly bool _session;

        private readonly DbSet<TSource> _dbSet;

        public BasicCrud(IEntityProfile<TSource> profile, DbContext dbContext, bool outerSession, bool innerSession)
        {
            _profile = profile;
            _dbContext = dbContext;
            _outerSession = outerSession;
            _session = innerSession || outerSession;
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
        public async Task<TSource> Get<TId>(TId id) where TId : struct
        {
            return await _profile.Include(_dbSet).FirstOrDefaultAsync(LambdaFactory(id));
        }

        /// <summary>
        /// Returns filters list of entities
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<TSource> Get(Expression<Func<TSource, bool>> expression)
        {
            return await _profile.Include(_dbSet).FirstOrDefaultAsync(expression);
        }

        public async Task<IEnumerable<TSource>> GetAllWhere(Expression<Func<TSource, bool>> expression)
        {
            return await _profile.Include(_dbSet).Where(expression).ToListAsync();
        }

        public async Task<TSource> Update(Expression<Func<TSource, bool>> expression, TSource dto)
        {
            var entity = await Get(expression);

            if (entity != null)
            {
                _profile.Update(entity, dto);

                if (!_session)
                {
                    await _dbContext.SaveChangesAsync();
                }
                
                return entity;
            }

            return null;
        }

        public async Task<TSource> Delete(Expression<Func<TSource, bool>> expression)
        {
            var entity = await Get(expression);

            if (entity != null)
            {
                _dbSet.Remove(entity);
                
                if (!_session)
                {
                    await _dbContext.SaveChangesAsync();
                }

                return entity;
            }

            return null;
        }

        /// <summary>
        /// Saves an instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Save(TSource instance)
        {
            await _dbSet.AddAsync(instance);
            
            if (!_session)
            {
                await _dbContext.SaveChangesAsync();
            }

            return instance;
        }

        /// <summary>
        /// Deletes entity given the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Delete<TId>(TId id) where TId: struct
        {
            return await Delete(LambdaFactory(id));
        }

        /// <summary>
        /// Updates entity given the id and new instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Update<TId>(TId id, TSource dto) where TId: struct
        {
            return await Update(LambdaFactory(id), dto);
        }

        public void Dispose()
        {
            if (!_outerSession)
            {
                _dbContext.SaveChanges();
            }
        }

        public ValueTask DisposeAsync()
        {
            if (!_outerSession)
            {
                return new ValueTask(_dbContext.SaveChangesAsync());
            }

            return new ValueTask(Task.CompletedTask);
        }

        public IBasicCrudSession<TSource> Session()
        {
            return new BasicCrud<TSource>(_profile, _dbContext, _outerSession, true);
        }

        private static Expression<Func<TSource, bool>> LambdaFactory<TId>(TId id) where TId: struct
        {
            var parameter = Expression.Parameter(typeof(TSource));
            
            var body = Expression.Equal(Expression.PropertyOrField(parameter, "Id"), Expression.Constant(id));
            
            var expression = Expression.Lambda<Func<TSource, bool>>(body, parameter);

            return expression;
        }
    }
}