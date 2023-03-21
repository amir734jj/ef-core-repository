using System;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrud<TSource> :
        IDisposable,
        IAsyncDisposable,
        IBasicCrudSingles<TSource>,
        IBasicCrudMany<TSource>,
        IBasicCrudUtils<TSource>,
        IBasicCrudUnsafe<TSource>
        where TSource : class
    {
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
    }
}