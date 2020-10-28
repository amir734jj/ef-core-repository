using System;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepositorySession : IAsyncDisposable, IDisposable
    {
        IBasicCrud<TSource, TId> For<TSource, TId>() where TSource : class, IEntity<TId>;
    }
}