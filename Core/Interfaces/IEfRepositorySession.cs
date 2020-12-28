using System;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepositorySession : IAsyncDisposable, IDisposable
    {
        IBasicCrud<TSource> For<TSource>() where TSource : class, IUntypedEntity;
    }
}