using System.Collections.Generic;

namespace Core.Interfaces
{
    public interface IEntityProfileAuxiliary<TProperty, TId>
        where TProperty : class, IEntity<TId>
    {
        /// <summary>
        /// ID Aware update entities
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        List<TProperty> ModifyList(List<TProperty> entity, List<TProperty> dto);
    }
}