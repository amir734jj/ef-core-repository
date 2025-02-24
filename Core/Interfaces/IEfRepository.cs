using System;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepository
    {
        // Get basic CRUD
        IBasicCrud<TSource> For<TSource>() where TSource : class;

        internal object For(Type type);
    }
}