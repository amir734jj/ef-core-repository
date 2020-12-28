namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudWrapper<TSource> : IBasicCrud<TSource> where TSource : class, IUntypedEntity
    {
        /// <summary>
        /// For complex and multi-action where we want to defer the save until the dispose
        /// </summary>
        /// <returns></returns>
        IBasicCrudSession<TSource> Session();
    }
}