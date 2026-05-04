using System;

namespace EfCoreRepository.Interfaces
{
    /// <summary>
    /// A disposable EfRepository session created from IEfRepositoryCreator.
    /// Each session owns its own DbContext and should be disposed after use.
    /// </summary>
    public interface IEfRepositorySession : IEfRepository, IDisposable, IAsyncDisposable
    {
    }
}
