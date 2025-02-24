using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudSingles<TSource> where TSource : class
    {
        Task<TSource> Get<TId>(TId id) where TId : struct;
        
        Task<TSource> Get(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct;

        Task<TSource> Update<TId>(TId id, Action<TSource> updater) where TId : struct;

        Task<TSource> Delete<TId>(TId id) where TId : struct;

        Task<TSource> Delete(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);
        
        Task<TSource> Save(TSource source);
    }
}