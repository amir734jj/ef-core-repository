using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudSingles<TSource> where TSource : class
    {
        Task<TSource> Get<TId>(TId id) where TId : struct;

        Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct;

        Task<TSource> Update<TId>(TId id, Action<TSource> updater) where TId : struct;

        /// <summary>
        /// Updates the first entity matching the filters by applying <paramref name="updater"/>.
        /// Works for entities with a non-struct key (e.g. a string) or no key at all.
        /// </summary>
        Task<TSource> Update(Expression<Func<TSource, bool>>[] filterExprs, Action<TSource> updater);

        Task<TSource> Delete<TId>(TId id) where TId : struct;

        Task<TSource> Delete(Expression<Func<TSource, bool>>[] filterExprs);

        Task<TSource> Save(TSource source);
    }
}