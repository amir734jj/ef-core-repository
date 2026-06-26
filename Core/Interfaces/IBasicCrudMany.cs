using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudMany<TSource> where TSource : class
    {
        Task<IEnumerable<TSource>> SaveMany(TSource[] sources);

        Task<IEnumerable<TSource>> DeleteMany<TId>(TId[] ids) where TId : struct;

        Task<IEnumerable<TSource>> DeleteMany(Expression<Func<TSource, bool>>[] filterExprs);

        Task<IEnumerable<TSource>> GetAll<TId>(TId[] ids) where TId : struct;
    }
}