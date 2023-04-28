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
        private readonly IEntityMapping _profile;

        private readonly DbContext _dbContext;

        private readonly SessionType _sessionType;

        private readonly DbSet<TSource> _dbSet;

        private bool _anyChanges;

        public BasicCrud(IEntityMapping profile, DbContext dbContext, SessionType sessionType)
        {
            _profile = profile;
            _dbContext = dbContext;
            _sessionType = sessionType;
            _dbSet = dbContext.Set<TSource>();
        }

        private IQueryable<TSource> GetQueryable(SessionType? sessionType = null)
        {
            sessionType ??= _sessionType;
            
            // Do not include any referenced entities if session is lightweight
            if (sessionType.Value.HasFlag(SessionType.LightWeight))
            {
                return _dbSet;
            }

            return (IQueryable<TSource>)_profile.Include(_dbSet);
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
        /// <param name="filterExpr"></param>
        /// <param name="additionalFilterExprs"></param>
        /// <returns></returns>
        public async Task<TSource> Get(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            return await ApplyFilters(GetQueryable(), new []{filterExpr}.Concat(additionalFilterExprs)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Update entity given filter expression and dto
        /// </summary>
        /// <param name="filterExpr"></param>
        /// <param name="dto"></param>
        /// <param name="additionalFilterExprs"></param>
        /// <returns></returns>
        public async Task<TSource> Update(TSource dto, Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            // With tracking
            var entity = await ApplyFilters(GetQueryable(), new []{filterExpr}.Concat(additionalFilterExprs)).FirstOrDefaultAsync();

            if (entity != null)
            {
                _profile.Update(entity, dto);

                if (!_sessionType.HasFlag(SessionType.Delayed))
                {
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Save changes when disposed
                    _anyChanges = true;
                }

                return entity;
            }

            return null;
        }

        public async Task<TSource> Delete<TId>(TId id) where TId : struct
        {
            return (await DeleteMany(FilterExpression<TSource, TId>(id))).FirstOrDefault();
        }

        public async Task<TSource> Delete(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            return (await DeleteMany(new []{filterExpr}.Concat(additionalFilterExprs).ToArray())).FirstOrDefault();
        }

        public async Task<TSource> Save(TSource source)
        {
            return (await SaveMany(source)).FirstOrDefault();
        }

        public async Task<IEnumerable<TSource>> SaveMany(params TSource[] sources)
        {
            await _dbSet.AddRangeAsync(sources);

            if (!_sessionType.HasFlag(SessionType.Delayed))
            {
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Save changes when disposed
                _anyChanges = true;
            }

            return sources;
        }

        public async Task<IEnumerable<TSource>> DeleteMany<TId>(params TId[] additionalIds) where TId : struct
        {
            return await DeleteMany(FilterExpression<TSource, TId>(additionalIds));
        }

        public async Task<IEnumerable<TSource>> DeleteMany(params Expression<Func<TSource, bool>>[] filterExprs)
        {
            // With tracking
            var entities = await ApplyFilters(GetQueryable(), filterExprs).ToListAsync();

            if (entities != null && entities.Any())
            {
                _dbSet.RemoveRange(entities);

                if (!_sessionType.HasFlag(SessionType.Delayed))
                {
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Save changes when disposed
                    _anyChanges = true;
                }

                return entities;
            }

            return null;
        }

        /// <summary>
        /// Get all entities given a filter expression
        /// </summary>
        /// <param name="filterExprs"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TSource>> GetAll(params Expression<Func<TSource, bool>>[] filterExprs)
        {
            return await ApplyFilters(GetQueryable(), filterExprs.ToArray()).ToListAsync();
        }

        /// <summary>
        /// Get all entities given Id array
        /// </summary>
        /// <param name="ids"></param>
        /// <typeparam name="TId"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct
        {
            return await GetAll(FilterExpression<TSource, TId>(ids));
        }

        public async Task<IEnumerable<TSource>> GetAll()
        {
            return await GetQueryable().ToListAsync();
        }

        public async Task<int> Count()
        {
            return await GetQueryable().CountAsync();
        }

        /// <summary>
        /// Count entities that pass filter expression
        /// </summary>
        /// <param name="filterExpr"></param>
        /// <param name="additionalFilterExprs"></param>
        /// <returns></returns>
        public async Task<int> Count(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            return await ApplyFilters(GetQueryable(SessionType.LightWeight), new[] { filterExpr }.Concat(additionalFilterExprs)).CountAsync();
        }

        /// <summary>
        /// Updates entity given the id and new instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual async Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct
        {
            return await Update(dto, FilterExpression<TSource, TId>(id));
        }

        /// <summary>
        /// Updates entity given the filter expression and function that modifies the entity
        /// </summary>
        /// <param name="filterExpr"></param>
        /// <param name="additionalFilterExprs"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        public async Task<TSource> Update(Action<TSource> updater, Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            // With tracking
            var entity = await ApplyFilters(GetQueryable(), new[] { filterExpr }.Concat(additionalFilterExprs)).FirstOrDefaultAsync();

            if (entity != null)
            {
                // Manual update
                updater(entity);
                
                // Another pass through profile
                _profile.Update(entity, entity);

                if (!_sessionType.HasFlag(SessionType.Delayed))
                {
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Save changes when disposed
                    _anyChanges = true;
                }

                return entity;
            }

            return null;
        }
        
        /// <summary>
        /// Updates entity given the id and function that modifies the entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        public async Task<TSource> Update<TId>(TId id, Action<TSource> updater) where TId : struct
        {
            return await Update(updater, FilterExpression<TSource, TId>(id));
        }

        public async Task<bool> Any(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            return await ApplyFilters(GetQueryable(), new[] { filterExpr }.Concat(additionalFilterExprs)).AnyAsync();
        }
        
        /// <summary>
        /// Invoke SaveChanges if session mode is active
        /// </summary>
        public void Dispose()
        {
            if (_anyChanges && _sessionType.HasFlag(SessionType.Delayed))
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
            if (_anyChanges && _sessionType.HasFlag(SessionType.Delayed))
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

        private static IQueryable<T> ApplyFilters<T>(IQueryable<T> source, IEnumerable<Expression<Func<T, bool>>> filterExprs)
        {
            return filterExprs.Aggregate(source, (current, filterExpr) => current.Where(filterExpr));
        }
    }
}