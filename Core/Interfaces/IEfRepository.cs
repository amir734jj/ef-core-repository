using System;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepository
    {
        // Get basic CRUD
        IBasicCrud<TSource> For<TSource>() where TSource : class, new();

        internal object For(Type type);
    }
}