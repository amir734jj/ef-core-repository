using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EfCoreRepository.Extensions;
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

        private IQueryable<TSource> GetQueryable(SessionType? sessionType = null, Func<IQueryable<TSource>, IQueryable<TSource>> includes = null)
        {
            sessionType ??= _sessionType;
            
            // Do not include any referenced entities if session is lightweight
            if (sessionType.Value.HasFlag(SessionType.LightWeight))
            {
                return _dbSet;
            }

            var queryable = sessionType.Value.HasFlag(SessionType.NoTracking) ? _dbSet.AsNoTracking() : _dbSet;

            if (includes != null)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                return includes(queryable);
            }

            return (IQueryable<TSource>)_profile.Include(queryable);
        }

        // Returns an entity given the id
        public async Task<TSource> Get<TId>(TId id) where TId : struct
        {
            return await GetQueryable().FirstOrDefaultAsync(FilterExpression<TSource, TId>(id));
        }

        // Returns filters list of entities
        public async Task<TSource> Get(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            return await ApplyFilters(GetQueryable(), new []{filterExpr}.Concat(additionalFilterExprs)).FirstOrDefaultAsync();
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

        public async Task<IEnumerable<TSource>> BulkUpdate<TId>(TId[] ids, Action<TSource> updater, int batchSize = 50) where TId : struct
        {
            var result = new List<TSource>();

            foreach (var idChunk in ids.ChunkBy(batchSize))
            {
                var entities = await ApplyFilters(GetQueryable(), [FilterExpression<TSource, TId>(idChunk.ToArray())]).ToListAsync();
            
                foreach (var entity in entities)
                {
                    if (entity != null)
                    {
                        // Manual update
                        updater(entity);
                
                        // Another pass through profile
                        _profile.Update(entity, entity);
                    }

                    result.Add(entity);
                }
            }

            if (!_sessionType.HasFlag(SessionType.Delayed))
            {
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Save changes when disposed
                _anyChanges = true;
            }

            return result;
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
            // This is needed, otherwise delete many deletes everything
            if (additionalIds.Length == 0)
            {
                return [];
            }
            
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

            return [];
        }

        // Get all entities given a filter expression
        public async Task<IEnumerable<TSource>> GetAll(
            Expression<Func<TSource, bool>>[] filterExprs = null,
            Func<IQueryable<TSource>, IQueryable<TSource>> includeExprs = null,
            Expression<Func<TSource, object>> orderBy = null,
            Expression<Func<TSource, object>> orderByDesc = null,
            int? maxResults = null)
        {
            var queryable = ApplyFilters(GetQueryable(includes: includeExprs), filterExprs?.ToArray() ?? []);

            if (orderBy != null)
            {
                queryable = queryable.OrderBy(orderBy);
            }
            
            if (orderByDesc != null)
            {
                queryable = queryable.OrderByDescending(orderByDesc);
            }

            if (maxResults.HasValue)
            {
                queryable = queryable.Take(maxResults.Value);
            }

            return await queryable.ToListAsync();
        }

        // Get all entities given Id array
        public async Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct
        {
            return await GetAll(filterExprs: [FilterExpression<TSource, TId>(ids)]);
        }

        public async Task<int> Count()
        {
            return await GetQueryable().CountAsync();
        }

        // Count entities that pass filter expression
        public async Task<int> Count(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            return await ApplyFilters(GetQueryable(SessionType.LightWeight), new[] { filterExpr }.Concat(additionalFilterExprs)).CountAsync();
        }

        // Updates entity given the id and new instance
        public virtual async Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct
        {
            return (await BulkUpdate([id], entity => _profile.Update(entity, dto))).FirstOrDefault();
        }

        // Updates entity given the id and function that modifies the entity
        public async Task<TSource> Update<TId>(TId id, Action<TSource> updater) where TId : struct
        {
            return (await BulkUpdate([id], updater)).FirstOrDefault();
        }

        public async Task<bool> Any(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs)
        {
            return await ApplyFilters(GetQueryable(), new[] { filterExpr }.Concat(additionalFilterExprs)).AnyAsync();
        }

        public async Task<IEnumerable<TSource>> Take(int limit)
        {
            return await GetQueryable().Take(limit).ToListAsync();
        }

        // Invoke SaveChanges if session mode is active
        public void Dispose()
        {
            if (_anyChanges && _sessionType.HasFlag(SessionType.Delayed))
            {
                _dbContext.SaveChanges();
            }
        }

        // Invoke SaveChangesAsync if session mode is active
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

        public IBasicCrud<TSource> NoTracking()
        {
            return new BasicCrud<TSource>(_profile, _dbContext ,_sessionType | SessionType.NoTracking);
        }

        private static IQueryable<T> ApplyFilters<T>(IQueryable<T> source, IEnumerable<Expression<Func<T, bool>>> filterExprs)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var filterExpr in filterExprs)
            {
                source = source.Where(filterExpr);
            }
            
            return source;
        }
    }
}