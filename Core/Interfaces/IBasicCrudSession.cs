using System;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudSession<TSource> : IBasicCrud<TSource>, IDisposable, IAsyncDisposable where TSource : class
    {
        
    }
}