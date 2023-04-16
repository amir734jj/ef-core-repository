using System;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepository
    {
        /// <summary>
        /// Get basic CRUD
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        IBasicCrud<TSource> For<TSource>() where TSource : class;

        internal object For(Type type);
    }
}