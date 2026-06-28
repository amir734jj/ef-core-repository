using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    /// <summary>
    /// Full read/write CRUD over a <see cref="DbSet{TSource}"/>. Inherits all query operations
    /// from <see cref="ReadOnlyCrud{TSource}"/> and adds inserts, updates, deletes, by-id access
    /// and session behavior.
    /// </summary>
    internal sealed class BasicCrud<TSource> : ReadOnlyCrud<TSource>, IBasicCrud<TSource>
        where TSource : class, new()
    {
        private readonly IEntityMapping profile;
        private readonly SessionType type;
        private readonly IAsyncDisposable ownedSession;
        private readonly DbSet<TSource> _dbSet;

        private bool _anyChanges;

        public BasicCrud(IEntityMapping profile, DbContext dbContext, SessionType type, IAsyncDisposable ownedSession = null)
            : base(dbContext)
        {
            this.profile = profile;
            this.type = type;
            this.ownedSession = ownedSession;
            _dbSet = dbContext.Set<TSource>();
        }

        protected override IQueryable<TSource> GetQueryable(SessionType? sessionType = null, Func<IQueryable<TSource>, IQueryable<TSource>> includes = null)
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

        public async Task<TSource> Delete<TId>(TId id) where TId : struct
        {
            return (await DeleteInternal([FilterExpression<TSource, TId>(id)], single: true)).FirstOrDefault();
        }

        public async Task<TSource> Delete(Expression<Func<TSource, bool>>[] filterExprs)
        {
            return (await DeleteInternal(filterExprs, single: true)).FirstOrDefault();
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
                await DbContext.SaveChangesAsync();
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
                await DbContext.SaveChangesAsync();
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
            if (filterExprs.Length == 0)
            {
                return [];
            }

            return await DeleteInternal(filterExprs, single: false);
        }

        private async Task<IEnumerable<TSource>> DeleteInternal(Expression<Func<TSource, bool>>[] filterExprs, bool single)
        {
            // With tracking
            var queryable = ApplyFilters(GetQueryable(), filterExprs);

            if (single)
            {
                // Take must run over an ordered query to be stable. Order by the entity key when
                // one exists to suppress RowLimitingOperationWithoutOrderByWarning; a keyless view
                // has none, so Take stays unordered for those.
                if (TryFindIdProperty<TSource>() != null)
                {
                    queryable = queryable.OrderBy(IdSelectorExpr<TSource>());
                }

                queryable = queryable.Take(1);
            }

            var entities = await queryable.ToListAsync();

            if (entities.Count > 0)
            {
                _dbSet.RemoveRange(entities);

                if (!type.HasFlag(SessionType.Delayed))
                {
                    await DbContext.SaveChangesAsync();
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

        // Get all entities given Id array
        public async Task<IEnumerable<TSource>> GetAll<TId>(TId[] ids) where TId : struct
        {
            return await GetAll<TSource>(filterExprs: [FilterExpression<TSource, TId>(ids)]);
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

        // Updates the first entity matching the filters - works for non-struct/no key entities.
        public async Task<TSource> Update(Expression<Func<TSource, bool>>[] filterExprs, Action<TSource> updater)
        {
            var entity = await ApplyFilters(GetQueryable(), filterExprs).FirstOrDefaultAsync();

            if (entity is null)
            {
                return null;
            }

            updater(entity);
            profile.Update(entity, entity);

            if (!type.HasFlag(SessionType.Delayed))
            {
                await DbContext.SaveChangesAsync();
            }
            else
            {
                _anyChanges = true;
            }

            return entity;
        }

        /// <summary>
        /// Checks if a navigation is a collection navigation.
        /// EF Core 8 compatibility: IsCollection is on INavigation directly
        /// </summary>
        private static bool IsCollectionNavigation(IReadOnlyNavigationBase navigation)
        {
            return navigation switch
            {
                // Try INavigation first (regular navigation)
                INavigation nav => nav.IsCollection,
                // Try ISkipNavigation (many-to-many) - these are always collections
                ISkipNavigation => true,
                _ => false
            };
        }

        // Checks whether entity has open references
        public async Task<bool> HasReferences(TSource entity)
        {
            var entry = DbContext.Entry(entity);
            var entityType = entry.Metadata;

            foreach (var navigation in entityType.GetNavigations())
            {
                var navigationEntry = entry.Navigation(navigation.Name);

                if (!IsCollectionNavigation(navigation) && !navigation.ForeignKey.IsOwnership && navigation.ForeignKey.PrincipalEntityType == entityType)
                {
                    // 1:1 or many:1 where current entity is the principal (has the other side referencing it)
                    if (!navigationEntry.IsLoaded)
                    {
                        await navigationEntry.LoadAsync();
                    }

                    var value = navigationEntry.CurrentValue;
                    if (value != null)
                    {
                        return true;
                    }
                }
                else if (IsCollectionNavigation(navigation))
                {
                    // Collection navigations (1:N)
                    if (!navigationEntry.IsLoaded)
                    {
                        await navigationEntry.LoadAsync();
                    }

                    var collection = (IEnumerable<object>)navigationEntry.CurrentValue;
                    if (collection?.Any() == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Invoke SaveChanges if session mode is active
        public void Dispose()
        {
            if (_anyChanges && type.HasFlag(SessionType.Delayed))
            {
                DbContext.SaveChanges();
            }

            (ownedSession as IDisposable)?.Dispose();
        }

        // Invoke SaveChangesAsync if session mode is active
        public async ValueTask DisposeAsync()
        {
            if (_anyChanges && type.HasFlag(SessionType.Delayed))
            {
                await DbContext.SaveChangesAsync();
            }

            if (ownedSession != null)
            {
                await ownedSession.DisposeAsync();
            }
        }

        public IBasicCrud<TSource> Delayed()
        {
            return new BasicCrud<TSource>(profile, DbContext, type | SessionType.Delayed);
        }

        public IBasicCrud<TSource> Light()
        {
            return new BasicCrud<TSource>(profile, DbContext, type | SessionType.LightWeight);
        }

        public IBasicCrud<TSource> NoTracking()
        {
            return new BasicCrud<TSource>(profile, DbContext, type | SessionType.NoTracking);
        }

        public IBasicCrud<TSource> SplitQuery()
        {
            return new BasicCrud<TSource>(profile, DbContext, type | SessionType.SplitQuery);
        }
    }
}
