using System;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudSession<TSource, in TId> : IBasicCrud<TSource, TId>, IDisposable, IAsyncDisposable where TSource : class, IEntity<TId>
    {
        
    }
}