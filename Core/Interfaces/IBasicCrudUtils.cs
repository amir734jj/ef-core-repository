using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudUtils<TSource> where TSource : class
    {
        Task<int> Count(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<bool> Any(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<IEnumerable<TSource>> Take(int limit);

        Task<int> Count();
    }
}