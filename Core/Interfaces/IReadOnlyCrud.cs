using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EfCoreRepository.Models;

namespace EfCoreRepository.Interfaces
{
  /// <summary>
  /// The read-only surface of the repository: querying, counting and projecting, with no
  /// insert/update/delete and no by-id access (which needs an entity key). A
  /// <see cref="Join{TInner,TKey}"/> returns this same read-only surface over the joined
  /// pair, so join results are query-only by construction - there is nothing to guard against.
  /// </summary>
  public interface IReadOnlyCrud<TSource> where TSource : class
  {
    /// <summary>Returns the first entity matching the filters, or <c>null</c>.</summary>
    Task<TSource> Get(Expression<Func<TSource, bool>>[] filterExprs);

    /// <summary>Queries, optionally filters/orders/limits, and projects to <typeparamref name="TProject"/>.</summary>
    Task<IEnumerable<TProject>> GetAll<TProject>(
        Expression<Func<TSource, bool>>[] filterExprs = null,
        Func<IQueryable<TSource>, IQueryable<TSource>> includeExprs = null,
        Expression<Func<TSource, object>> orderBy = null,
        Expression<Func<TSource, object>> orderByDesc = null,
        Expression<Func<TSource, TProject>> project = null,
        int? maxResults = null,
        Expression<Func<TSource, object>> distinctBy = null,
        Expression<Func<TSource, object>> thenBy = null,
        Expression<Func<TSource, object>> thenByDesc = null) where TProject : class;

    /// <summary>Returns all entities.</summary>
    Task<IEnumerable<TSource>> GetAll();

    /// <summary>Whether any entity exists.</summary>
    Task<bool> Any();

    /// <summary>Whether any entity matches the filters.</summary>
    Task<bool> Any(Expression<Func<TSource, bool>>[] filterExprs);

    /// <summary>Counts all entities.</summary>
    Task<int> Count();

    /// <summary>Counts entities matching the filters.</summary>
    Task<int> Count(Expression<Func<TSource, bool>>[] filterExprs);

    /// <summary>Returns up to <paramref name="limit"/> entities.</summary>
    Task<IEnumerable<TSource>> Take(int limit);

    /// <summary>
    /// Joins <typeparamref name="TSource"/> with <typeparamref name="TInner"/> on matching keys
    /// and returns the result as a read-only surface over a <see cref="Joined{TOuter,TInner}"/>
    /// pair. Joins can be chained.
    /// </summary>
    /// <param name="joinType">Which side(s) to drive the join from (inner, left, right, full outer).</param>
    /// <param name="inclusivity">
    /// Whether to keep matched rows (<see cref="JoinInclusivity.Inclusive"/>) or only the rows
    /// that exist on a single side (<see cref="JoinInclusivity.Exclusive"/>). Exclusive has no
    /// meaning for an inner join and is rejected for that combination.
    /// </param>
    IReadOnlyCrud<Joined<TSource, TInner>> Join<TInner, TKey>(
        Expression<Func<TSource, TKey>> outerKey,
        Expression<Func<TInner, TKey>> innerKey,
        JoinType joinType = JoinType.Inner,
        JoinInclusivity inclusivity = JoinInclusivity.Inclusive) where TInner : class;
  }
}
