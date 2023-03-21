using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudMany<TSource> where TSource : class
    {
        Task<IEnumerable<TSource>> SaveMany(TSource source, params TSource[] additionalSources);

        Task<IEnumerable<TSource>> DeleteMany<TId>(TId id, params TId[] additionalIds) where TId : struct;

        Task<IEnumerable<TSource>> DeleteMany(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<IEnumerable<TSource>> GetAll(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);
        
        Task<IEnumerable<TSource>> GetAll<TId>(TId id, params TId[] ids) where TId : struct;
    }
}