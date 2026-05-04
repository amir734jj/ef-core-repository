using System.Threading.Tasks;

namespace EfCoreRepository.Interfaces
{
    /// <summary>
    /// Creates short-lived IBasicCrud&lt;TSource&gt; instances, each backed by its own DbContext.
    /// Inject this for parallel query execution on a specific entity type.
    /// Dispose the returned IBasicCrud when done to release the underlying DbContext.
    /// </summary>
    public interface IEfRepositoryCreator<TSource> where TSource : class, new()
    {
        IBasicCrud<TSource> Create();

        Task<IBasicCrud<TSource>> CreateAsync();
    }
}
