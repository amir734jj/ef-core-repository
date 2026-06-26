using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AgileObjects.AgileMapper;
using EfCoreRepository.Interfaces;
using EfCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    /// <summary>
    /// Read-only implementation shared by every query path. Instantiated directly to wrap a
    /// composed join projection (so a join result has no write methods at all), and used as the
    /// base of the writable <see cref="BasicCrud{TSource}"/>, which overrides
    /// <see cref="GetQueryable"/> to layer in profiles, includes and session behavior.
    /// </summary>
    internal class ReadOnlyCrud<TSource> : IReadOnlyCrud<TSource>
        where TSource : class, new()
    {
        protected readonly DbContext DbContext;

        private readonly IQueryable<TSource> _root;

        // Standalone read-only projection (e.g. a join result).
        public ReadOnlyCrud(DbContext dbContext, IQueryable<TSource> root)
        {
            DbContext = dbContext;
            _root = root;
        }

        // Used by the writable BasicCrud subclass, which overrides GetQueryable.
        protected ReadOnlyCrud(DbContext dbContext)
        {
            DbContext = dbContext;
        }

        protected virtual IQueryable<TSource> GetQueryable(SessionType? sessionType = null, Func<IQueryable<TSource>, IQueryable<TSource>> includes = null)
        {
            return includes != null ? includes(_root) : _root;
        }

        public async Task<TSource> Get(Expression<Func<TSource, bool>>[] filterExprs)
        {
            return await ApplyFilters(GetQueryable(), filterExprs).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TProject>> GetAll<TProject>(
            Expression<Func<TSource, bool>>[] filterExprs = null,
            Func<IQueryable<TSource>, IQueryable<TSource>> includeExprs = null,
            Ordering<TSource> orderBy = null,
            Expression<Func<TSource, TProject>> project = null,
            int? skip = null,
            int? maxResults = null,
            Expression<Func<TSource, object>> distinctBy = null) where TProject : class
        {
            var queryable = ApplyFilters(GetQueryable(includes: includeExprs), filterExprs?.ToArray() ?? []);

            if (distinctBy != null)
            {
                // Distinct by an arbitrary key: one row per group. No key selector means no distincting.
                queryable = queryable.GroupBy(distinctBy).Select(g => g.First());
            }

            if (orderBy is { Keys.Count: > 0 })
            {
                // First key is ORDER BY; the rest chain as THEN BY, preserving direction per key.
                var keys = orderBy.Keys;
                var ordered = keys[0].Descending
                    ? queryable.OrderByDescending(keys[0].KeySelector)
                    : queryable.OrderBy(keys[0].KeySelector);

                for (var i = 1; i < keys.Count; i++)
                {
                    ordered = keys[i].Descending
                        ? ordered.ThenByDescending(keys[i].KeySelector)
                        : ordered.ThenBy(keys[i].KeySelector);
                }

                queryable = ordered;
            }

            if (skip.HasValue || maxResults.HasValue)
            {
                // Skip/Take must run over an ordered query to be stable. Suppress
                // RowLimitingOperationWithoutOrderByWarning when the entity has a key to stabilize
                // on. A join projection or keyless view has none, so callers order those explicitly.
                if (orderBy is not { Keys.Count: > 0 } && TryFindIdProperty<TSource>() != null)
                {
                    queryable = queryable.OrderBy(IdSelectorExpr<TSource>());
                }

                if (skip.HasValue)
                {
                    queryable = queryable.Skip(skip.Value);
                }

                if (maxResults.HasValue)
                {
                    queryable = queryable.Take(maxResults.Value);
                }
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

        public async Task<IEnumerable<TSource>> GetAll()
        {
            return await GetQueryable().ToListAsync();
        }

        public async Task<bool> Any()
        {
            return await Any([]);
        }

        public async Task<bool> Any(Expression<Func<TSource, bool>>[] filterExprs)
        {
            return await ApplyFilters(GetQueryable(), filterExprs).AnyAsync();
        }

        public async Task<int> Count()
        {
            return await GetQueryable().CountAsync();
        }

        public async Task<int> Count(Expression<Func<TSource, bool>>[] filterExprs)
        {
            return await ApplyFilters(GetQueryable(SessionType.LightWeight), filterExprs).CountAsync();
        }

        public async Task<IEnumerable<TSource>> Take(int limit)
        {
            return await GetQueryable().Take(limit).ToListAsync();
        }

        public IReadOnlyCrud<Joined<TSource, TInner>> Join<TInner, TKey>(
            Expression<Func<TSource, TKey>> outerKey,
            Expression<Func<TInner, TKey>> innerKey,
            JoinType joinType = JoinType.Inner,
            JoinInclusivity inclusivity = JoinInclusivity.Inclusive) where TInner : class
        {
            // Join roots are read-only and projected, so use a no-tracking, no-eager-include
            // query on both sides to avoid building "god objects".
            var outer = GetQueryable(SessionType.NoTracking | SessionType.LightWeight);
            var inner = DbContext.Set<TInner>().AsNoTracking();

            IQueryable<Joined<TSource, TInner>> joined = joinType switch
            {
                JoinType.Inner => InnerJoin(outer, inner, outerKey, innerKey),
                JoinType.Left => LeftJoin(outer, inner, outerKey, innerKey),
                JoinType.Right => RightJoin(outer, inner, outerKey, innerKey),
                JoinType.FullOuter => FullOuterJoin(outer, inner, outerKey, innerKey),
                _ => throw new ArgumentOutOfRangeException(nameof(joinType), joinType, "Unsupported join type.")
            };

            if (inclusivity == JoinInclusivity.Exclusive)
            {
                joined = ExcludeMatched(joined, joinType);
            }

            return new ReadOnlyCrud<Joined<TSource, TInner>>(DbContext, joined);
        }

        // Queryable.Join translates to a SQL INNER JOIN.
        private static IQueryable<Joined<TLeft, TRight>> InnerJoin<TLeft, TRight, TKey>(
            IQueryable<TLeft> left, IQueryable<TRight> right,
            Expression<Func<TLeft, TKey>> leftKey, Expression<Func<TRight, TKey>> rightKey)
        {
            return left.Join(right, leftKey, rightKey,
                (l, r) => new Joined<TLeft, TRight> { Outer = l, Inner = r });
        }

        // GroupJoin + SelectMany + DefaultIfEmpty translates to a SQL LEFT JOIN.
        private static IQueryable<Joined<TLeft, TRight>> LeftJoin<TLeft, TRight, TKey>(
            IQueryable<TLeft> left, IQueryable<TRight> right,
            Expression<Func<TLeft, TKey>> leftKey, Expression<Func<TRight, TKey>> rightKey)
        {
            return left
                .GroupJoin(right, leftKey, rightKey, (l, g) => new { l, g })
                .SelectMany(x => x.g.DefaultIfEmpty(),
                    (x, r) => new Joined<TLeft, TRight> { Outer = x.l, Inner = r });
        }

        // A right join is just a left join with the sides swapped back into place.
        private static IQueryable<Joined<TLeft, TRight>> RightJoin<TLeft, TRight, TKey>(
            IQueryable<TLeft> left, IQueryable<TRight> right,
            Expression<Func<TLeft, TKey>> leftKey, Expression<Func<TRight, TKey>> rightKey)
        {
            return Swap(LeftJoin(right, left, rightKey, leftKey));
        }

        // A full outer join is the left join plus the right rows that had no left match.
        private static IQueryable<Joined<TLeft, TRight>> FullOuterJoin<TLeft, TRight, TKey>(
            IQueryable<TLeft> left, IQueryable<TRight> right,
            Expression<Func<TLeft, TKey>> leftKey, Expression<Func<TRight, TKey>> rightKey)
        {
            var leftRows = LeftJoin(left, right, leftKey, rightKey);
            var rightOnlyRows = Swap(LeftJoin(right, left, rightKey, leftKey)).Where(p => p.Outer == null);

            return leftRows.Concat(rightOnlyRows);
        }

        // Flips a joined pair so the inner and outer sides trade places.
        private static IQueryable<Joined<TRight, TLeft>> Swap<TLeft, TRight>(IQueryable<Joined<TLeft, TRight>> source)
        {
            return source.Select(p => new Joined<TRight, TLeft> { Outer = p.Inner, Inner = p.Outer });
        }

        // Narrows an inclusive join down to its exclusive region - the outer crescents of the Venn
        // diagram, i.e. the rows that exist on only one side.
        private static IQueryable<Joined<TLeft, TRight>> ExcludeMatched<TLeft, TRight>(
            IQueryable<Joined<TLeft, TRight>> joined, JoinType joinType)
        {
            return joinType switch
            {
                // Left only (not B): outer rows that found no inner match.
                JoinType.Left => joined.Where(p => p.Inner == null),
                // Right only (not A): inner rows that found no outer match.
                JoinType.Right => joined.Where(p => p.Outer == null),
                // Symmetric difference (A XOR B): rows present on exactly one side.
                JoinType.FullOuter => joined.Where(p => p.Outer == null || p.Inner == null),
                // An inner join only contains matched rows, so its exclusive region is empty by
                // definition - reject the combination rather than silently returning nothing.
                JoinType.Inner => throw new ArgumentException(
                    "An inner join has no exclusive variant; its exclusive region is always empty.",
                    nameof(joinType)),
                _ => throw new ArgumentOutOfRangeException(nameof(joinType), joinType, "Unsupported join type.")
            };
        }

        protected static IQueryable<T> ApplyFilters<T>(IQueryable<T> source, IEnumerable<Expression<Func<T, bool>>> filterExprs)
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
