using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrud<TSource> : IDisposable, IAsyncDisposable where TSource : class
    {
        #region Utilities

        /// <summary>
        /// For complex and multi-action where we want to defer the save until the dispose takes place
        /// </summary>
        /// <returns></returns>
        IBasicCrud<TSource> Delayed();
        
        /// <summary>
        /// Avoids eager loading altogether for a lightweight session
        /// </summary>
        /// <returns></returns>
        IBasicCrud<TSource> Light();

        #endregion
        
        #region Basics

        Task<int> Count();
        
        Task<IEnumerable<TSource>> Save(TSource source, params TSource[] additionalSources);
        
        #endregion
        
        #region IdAware
        
        Task<TSource> Get<TId>(TId id) where TId : struct;

        Task<IEnumerable<TSource>> GetAll<TId>(params TId[] ids) where TId : struct;

        Task<TSource> Delete<TId>(TId id) where TId : struct;

        Task<TSource> Update<TId>(TId id, TSource dto) where TId : struct;

        Task<TSource> Update<TId>(TId id, Action<TSource> updater) where TId : struct;

        #endregion

        #region IdUnAware

        Task<bool> Any(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<TSource> Get(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<TSource> Update(TSource dto, Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<TSource> Update(Action<TSource> updater, Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<TSource> Delete(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<IEnumerable<TSource>> GetAll(params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        Task<int> Count(Expression<Func<TSource, bool>> filterExpr, params Expression<Func<TSource, bool>>[] additionalFilterExprs);

        #endregion
    }
}