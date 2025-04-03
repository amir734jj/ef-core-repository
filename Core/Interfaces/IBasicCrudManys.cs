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

        Task<IEnumerable<TProject>> GetAll<TProject>(
            Expression<Func<TSource, bool>>[] filterExprs = null,
            Func<IQueryable<TSource>, IQueryable<TSource>> includeExprs = null,
            Expression<Func<TSource, object>> orderBy = null,
            Expression<Func<TSource, object>> orderByDesc = null,
            Expression<Func<TSource, TProject>> project = null,
            int? maxResults = null) where TProject : class, new();
        
        Task<IEnumerable<TSource>> GetAll<TId>(TId[] ids) where TId : struct;
    }
}