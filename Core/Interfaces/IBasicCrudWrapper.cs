namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudWrapper<TSource, in TId> : IBasicCrud<TSource, TId> where TSource : class, IEntity<TId>
    {
        /// <summary>
        /// For complex and multi-action where we want to defer the save until the dispose
        /// </summary>
        /// <returns></returns>
        IBasicCrudSession<TSource, TId> Session();
    }
}