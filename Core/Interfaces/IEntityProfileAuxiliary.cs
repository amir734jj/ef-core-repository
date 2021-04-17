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
        IList<TProperty> ModifyList<TProperty, TId>(IList<TProperty> entity, IList<TProperty> dto,
            Func<TProperty, TId> idSelector);

        IList<TProperty> ModifyList<TProperty, TId>(IList<TProperty> entity, IList<TProperty> dto)
            where TProperty : class
            where TId : struct;
    }
}