using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudUtils<TSource> where TSource : class
    {
        Task<int> Count(Expression<Func<TSource, bool>>[] filterExprs);

        Task<bool> Any(Expression<Func<TSource, bool>>[] filterExprs);

        Task<IEnumerable<TSource>> Take(int limit);
    }
}