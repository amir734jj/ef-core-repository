namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudType<TSource, in TId> : IBasicCrud<TSource, TId> where TSource : class, IEntity<TId>
    {
        /// <summary>
        /// For complex and multi-action
        /// </summary>
        /// <returns></returns>
        IBasicCrudSession<TSource, TId> Session();
    }
}