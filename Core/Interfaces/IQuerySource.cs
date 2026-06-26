using System.Linq;

namespace EfCoreRepository.Interfaces
{
    /// <summary>
    /// Read-only access to the underlying entity query roots. It is handed to
    /// <see cref="IEfRepository.Query{TProject}"/> so callers can compose arbitrary
    /// queries - including joins across multiple entity sets that have no navigation
    /// property between them - without exposing the full <c>DbContext</c>.
    /// </summary>
    public interface IQuerySource
    {
        /// <summary>
        /// Returns the queryable root for <typeparamref name="TEntity"/>.
        /// </summary>
        IQueryable<TEntity> Set<TEntity>() where TEntity : class;
    }
}
