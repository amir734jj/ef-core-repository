using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudMany<TSource> where TSource : class
    {
        Task<IEnumerable<TSource>> SaveMany(params TSource[] additionalSources);

        Task<IEnumerable<TSource>> DeleteMany<TId>(params TId[] additionalIds) where TId : struct;

        Task<IEnumerable<TSource>> DeleteMany(params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<IEnumerable<TSource>> GetAll(
            Expression<Func<TSource, bool>>[] filterExprs = default,
            Func<IQueryable<TSource>, IQueryable<TSource>> includeExprs = default);
        
        Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct;
    }
}