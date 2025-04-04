using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AgileObjects.AgileMapper;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    internal sealed class BasicCrud<TSource>(IEntityMapping profile, DbContext dbContext, SessionType type)
        : IBasicCrud<TSource>
        where TSource : class, new()
    {
        private readonly DbSet<TSource> _dbSet = dbContext.Set<TSource>();

        private bool _anyChanges;

        private IQueryable<TSource> GetQueryable(SessionType? sessionType = null, Func<IQueryable<TSource>, IQueryable<TSource>> includes = null)
        {
            sessionType ??= type;
            
            IQueryable<TSource> queryable = _dbSet;

            if (sessionType.Value.HasFlag(SessionType.SplitQuery))
            {
                queryable = _dbSet.AsSplitQuery();
            }

            if (sessionType.Value.HasFlag(SessionType.NoTracking))
            {
                queryable = _dbSet.AsNoTracking();
            }
            
            // Do not include any referenced entities if session is lightweight
            if (sessionType.Value.HasFlag(SessionType.LightWeight))
            {
                return queryable;
            }

            // custom includes if any
            if (includes != null)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                return includes(queryable);
            }

            return (IQueryable<TSource>)profile.Include(queryable);
        }

        // Returns an entity given the id
        public async Task<TSource> Get<TId>(TId id) where TId : struct
        {
            return await GetQueryable().FirstOrDefaultAsync(FilterExpression<TSource, TId>(id));
        }

        // Returns filters list of entities
        public async Task<TSource> Get(Expression<Func<TSource, bool>>[] filterExprs)
        {
            return await ApplyFilters(GetQueryable(), filterExprs).FirstOrDefaultAsync();
        }

        public async Task<TSource> Delete<TId>(TId id) where TId : struct
        {
            return (await DeleteMany([FilterExpression<TSource, TId>(id)])).FirstOrDefault();
        }

        public async Task<TSource> Delete(Expression<Func<TSource, bool>>[] filterExprs)
        {
            return (await DeleteMany(filterExprs)).FirstOrDefault();
        }

        public async Task<TSource> Save(TSource source)
        {
            return (await SaveMany([source])).FirstOrDefault();
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
                        profile.Update(entity, entity);
                    }

                    result.Add(entity);
                }
            }

            if (!type.HasFlag(SessionType.Delayed))
            {
                await dbContext.SaveChangesAsync();
            }
            else
            {
                // Save changes when disposed
                _anyChanges = true;
            }

            return result;
        }

        public async Task<IEnumerable<TSource>> SaveMany(TSource[] sources)
        {
            await _dbSet.AddRangeAsync(sources);

            if (!type.HasFlag(SessionType.Delayed))
            {
                await dbContext.SaveChangesAsync();
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
            
            return await DeleteMany([FilterExpression<TSource, TId>(additionalIds)]);
        }

        public async Task<IEnumerable<TSource>> DeleteMany(Expression<Func<TSource, bool>>[] filterExprs)
        {
            // With tracking
            var entities = await ApplyFilters(GetQueryable(), filterExprs).ToListAsync();

            if (entities.Any())
            {
                _dbSet.RemoveRange(entities);

                if (!type.HasFlag(SessionType.Delayed))
                {
                    await dbContext.SaveChangesAsync();
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
        public async Task<IEnumerable<TProject>> GetAll<TProject>(
            Expression<Func<TSource, bool>>[] filterExprs = null,
            Func<IQueryable<TSource>, IQueryable<TSource>> includeExprs = null,
            Expression<Func<TSource, object>> orderBy = null,
            Expression<Func<TSource, object>> orderByDesc = null,
            Expression<Func<TSource, TProject>> project = null,
            int? maxResults = null) where TProject : class, new()
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
    
            if (project != null)
            {
                return await queryable.Select(project).ToListAsync();
            }

            if (typeof(TProject) == typeof(TSource))
            {
                return await queryable.Cast<TProject>().ToListAsync();
            }

            return (await queryable.ToListAsync()).Select(entity => Mapper.Map(entity).ToANew<TProject>(opt => opt.MapEntityKeys())).ToList();
        }

        // Get all entities given Id array
        public async Task<IEnumerable<TSource>> GetAll<TId>(TId[] ids) where TId : struct
        {
            return await this.GetAll(filterExprs: [FilterExpression<TSource, TId>(ids)]);
        }

        public async Task<IEnumerable<TSource>> GetAll()
        {
            return await GetQueryable().ToListAsync();
        }

        public async Task<bool> Any()
        {
            return await Any([]);
        }

        public async Task<int> Count()
        {
            return await GetQueryable().CountAsync();
        }

        // Count entities that pass filter expression
        public async Task<int> Count(Expression<Func<TSource, bool>>[] filterExprs)
        {
            return await ApplyFilters(GetQueryable(SessionType.LightWeight), filterExprs).CountAsync();
        }

        // Updates entity given the id and new instance
        public async Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct
        {
            return (await BulkUpdate([id], entity => profile.Update(entity, dto))).FirstOrDefault();
        }

        // Updates entity given the id and function that modifies the entity
        public async Task<TSource> Update<TId>(TId id, Action<TSource> updater) where TId : struct
        {
            return (await BulkUpdate([id], updater)).FirstOrDefault();
        }

        public async Task<bool> Any(Expression<Func<TSource, bool>>[] filterExprs)
        {
            return await ApplyFilters(GetQueryable(), filterExprs).AnyAsync();
        }

        public async Task<IEnumerable<TSource>> Take(int limit)
        {
            return await GetQueryable().Take(limit).ToListAsync();
        }

        // Invoke SaveChanges if session mode is active
        public void Dispose()
        {
            if (_anyChanges && type.HasFlag(SessionType.Delayed))
            {
                dbContext.SaveChanges();
            }
        }

        // Invoke SaveChangesAsync if session mode is active
        public ValueTask DisposeAsync()
        {
            if (_anyChanges && type.HasFlag(SessionType.Delayed))
            {
                return new ValueTask(dbContext.SaveChangesAsync());
            }

            return new ValueTask(Task.CompletedTask);
        }
        
        public IBasicCrud<TSource> Delayed()
        {
            return new BasicCrud<TSource>(profile, dbContext ,type | SessionType.Delayed);
        }

        public IBasicCrud<TSource> Light()
        {
            return new BasicCrud<TSource>(profile, dbContext, type | SessionType.LightWeight);
        }

        public IBasicCrud<TSource> NoTracking()
        {
            return new BasicCrud<TSource>(profile, dbContext ,type | SessionType.NoTracking);
        }

        public IBasicCrud<TSource> SplitQuery()
        {
            return new BasicCrud<TSource>(profile, dbContext ,type | SessionType.SplitQuery);
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