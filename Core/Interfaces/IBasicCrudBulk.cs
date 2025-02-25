using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudBulk<TSource> where TSource : class
    {
        Task<IEnumerable<TSource>> BulkUpdate<TId>(TId[] ids, Action<TSource> updater, int batchSize = 50) where TId : struct;
    }
}