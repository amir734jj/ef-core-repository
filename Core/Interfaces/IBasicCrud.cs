using System;

namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrud<TSource> :
        IDisposable,
        IAsyncDisposable,
        IBasicCrudSingles<TSource>,
        IBasicCrudMany<TSource>,
        IBasicCrudUtils<TSource>,
        IBasicCrudBulk<TSource>
        where TSource : class
    {
        // For complex and multi-action where we want to defer the save until the dispose takes place
        IBasicCrud<TSource> Delayed();

        // Avoids eager loading altogether for a lightweight session
        IBasicCrud<TSource> Light();

        // It avoids tracking of entities for change, but also it doesn't load 2 level nested properties
        IBasicCrud<TSource> NoTracking();

        // Uses split query instead of default single query
        IBasicCrud<TSource> SplitQuery();
    }
}