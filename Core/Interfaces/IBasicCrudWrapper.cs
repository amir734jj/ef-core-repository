namespace EfCoreRepository.Interfaces
{
    public interface IBasicCrudWrapper<TSource> : IBasicCrud<TSource> where TSource : class, IUntypedEntity
    {
        /// <summary>
        /// For complex and multi-action where we want to defer the save until the dispose takes place
        /// </summary>
        /// <returns></returns>
        IBasicCrudSession<TSource> Delayed();
        
        /// <summary>
        /// Avoids eager loading altogether for a lightweight session
        /// </summary>
        /// <returns></returns>
        IBasicCrudSession<TSource> Light();
    }
}