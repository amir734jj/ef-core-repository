using System;
using System.Linq.Expressions;
using EfCoreRepository.Models;

namespace EfCoreRepository.Interfaces
{
    /// <summary>
    /// Join entry point for an entity set. Joins across two entity sets that have no
    /// navigation property between them and returns the result as a read-only
    /// <see cref="IBasicCrud{TSource}"/> over a <see cref="Joined{TOuter,TInner}"/> pair -
    /// so the usual <c>GetAll</c>/<c>Get</c>/<c>Any</c>/<c>Count</c> read surface (with
    /// <c>filterExprs</c> and <c>project</c>) applies to the joined rows. The underlying
    /// <c>IQueryable</c> is never exposed.
    /// </summary>
    public interface IBasicCrudJoins<TSource> where TSource : class
    {
        /// <summary>
        /// Joins <typeparamref name="TSource"/> with <typeparamref name="TInner"/> on matching
        /// keys, using the given <paramref name="joinType"/>.
        /// </summary>
        IBasicCrud<Joined<TSource, TInner>> Join<TInner, TKey>(
            Expression<Func<TSource, TKey>> outerKey,
            Expression<Func<TInner, TKey>> innerKey,
            JoinType joinType = JoinType.Inner) where TInner : class;
    }
}
