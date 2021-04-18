using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    internal class BasicCrud<TSource> : IBasicCrud<TSource> where TSource : class
    {
        private readonly EntityProfile<TSource> _profile;

        private readonly DbContext _dbContext;

        private readonly SessionType _sessionType;

        private readonly DbSet<TSource> _dbSet;

        public BasicCrud(EntityProfile<TSource> profile, DbContext dbContext, SessionType sessionType)
        {
            _profile = profile;
            _dbContext = dbContext;
            _sessionType = sessionType;
            _dbSet = dbContext.Set<TSource>();
        }

        private IQueryable<TSource> GetQueryable()
        {
            if (_sessionType.HasFlag(SessionType.LightWeight))
            {
                return _dbSet;
            }

            return _profile.Include(_dbSet);
        }

        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IEnumerable<TSource>> GetAll()
        {
            return await GetQueryable().ToListAsync();
        }

        /// <summary>
        /// Returns an entity given the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TSource> Get<TId>(TId id) where TId : struct
        {
            return await GetQueryable().FirstOrDefaultAsync(FilterExpression<TSource, TId>(id));
        }

        /// <summary>
        /// Returns filters list of entities
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<TSource> Get(Expression<Func<TSource, bool>> expression)
        {
            return await GetQueryable().FirstOrDefaultAsync(expression);
        }

        /// <summary>
        /// Update entity given filter expression and dto
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<TSource> Update(Expression<Func<TSource, bool>> expression, TSource dto)
        {
            var entity = await Get(expression);

            if (entity != null)
            {
                _profile.Update(entity, dto);

                if (!_sessionType.HasFlag(SessionType.Delayed))
                {
                    await _dbContext.SaveChangesAsync();
                }

                return entity;
            }

            return null;
        }

        /// <summary>
        /// Deletes entity given filter expression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<TSource> Delete(Expression<Func<TSource, bool>> expression)
        {
            var entity = await Get(expression);

            if (entity != null)
            {
                _dbSet.Remove(entity);

                if (!_sessionType.HasFlag(SessionType.Delayed))
                {
                    await _dbContext.SaveChangesAsync();
                }

                return entity;
            }

            return null;
        }

        /// <summary>
        /// Get all entities given a filter expression
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TSource>> GetAll(Expression<Func<TSource, bool>> filter)
        {
            return await GetQueryable().Where(filter).ToListAsync();
        }

        /// <summary>
        /// Get all entities given Id array
        /// </summary>
        /// <param name="ids"></param>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct
        {
            return await GetQueryable().Where(FilterExpression<TSource, TId>(ids)).ToListAsync();
        }

        /// <summary>
        /// Save many DTOs at the same time
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TSource>> Save(params TSource[] instances)
        {
            await _dbSet.AddRangeAsync(instances);

            if (!_sessionType.HasFlag(SessionType.Delayed))
            {
                await _dbContext.SaveChangesAsync();
            }

            return instances;
        }

        /// <summary>
        /// Saves an instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Save(TSource instance)
        {
            await _dbSet.AddAsync(instance);

            if (!_sessionType.HasFlag(SessionType.Delayed))
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
        public virtual async Task<TSource> Delete<TId>(TId id) where TId : struct
        {
            return await Delete(FilterExpression<TSource, TId>(id));
        }

        /// <summary>
        /// Updates entity given the id and new instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct
        {
            return await Update(FilterExpression<TSource, TId>(id), dto);
        }

        /// <summary>
        /// Invoke SaveChanges if session mode is active
        /// </summary>
        public void Dispose()
        {
            if (_sessionType.HasFlag(SessionType.Delayed))
            {
                _dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Invoke SaveChangesAsync if session mode is active
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            if (_sessionType.HasFlag(SessionType.Delayed))
            {
                return new ValueTask(_dbContext.SaveChangesAsync());
            }

            return new ValueTask(Task.CompletedTask);
        }
        
        public IBasicCrud<TSource> Delayed()
        {
            return new BasicCrud<TSource>(_profile, _dbContext ,_sessionType | SessionType.Delayed);
        }

        public IBasicCrud<TSource> Light()
        {
            return new BasicCrud<TSource>(_profile, _dbContext, _sessionType | SessionType.LightWeight);
        }
    }
}