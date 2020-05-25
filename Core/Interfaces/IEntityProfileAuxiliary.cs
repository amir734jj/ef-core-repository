using System;
using System.Collections.Generic;

namespace EfCoreRepository.Interfaces
{
    public interface IEntityProfileAuxiliary
    {
        /// <summary>
        /// ID Aware update entities
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        /// <param name="idSelector"></param>
        /// <returns></returns>
        List<TProperty> ModifyList<TProperty, TId>(List<TProperty> entity, List<TProperty> dto, Func<TProperty, TId> idSelector);
        
        List<TProperty> ModifyList<TProperty, TId>(List<TProperty> entity, List<TProperty> dto) where TProperty: IEntity<TId>;
    }
}