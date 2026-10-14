using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudBulk<TSource> where TSource : class
    {
        Task<IEnumerable<TSource>> BulkUpdate<TId>(TId[] ids, Action<TSource> updater, int batchSize = 50) where TId : struct;

        /// <summary>
        /// Executes an immediate set-based update for all entities matching the filters.
        /// Returns the number of affected rows. The update bypasses the change tracker, so tracked
        /// entity instances are not synchronized with the database values.
        /// </summary>
        Task<int> ExecuteUpdate(
            Expression<Func<TSource, bool>>[] filterExprs,
            Action<UpdateSettersBuilder<TSource>> setters,
            CancellationToken cancellationToken = default);
    }
}