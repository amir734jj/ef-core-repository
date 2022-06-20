using Microsoft.EntityFrameworkCore;

namespace EfCoreRepository.Interfaces
{
    public interface IEfRepository
    {
        /// <summary>
        /// Get basic CRUD
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        IBasicCrud<TSource> For<TSource>(DbContext context) where TSource : class;
    }
}