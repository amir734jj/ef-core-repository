using System;
using System.Collections.Generic;
using System.Linq;

namespace EfCoreRepository.Abstracts
{
    public abstract class AbstractMappingUtility
    {
        // Utility that applies addition/deletion to the list
        protected static void ModifyList<TProperty, TId>(IList<TProperty> entity, IList<TProperty> dto,
            Func<TProperty, TId> idSelector)
            where TProperty : class
            where TId : struct
        {
            entity ??= new List<TProperty>();
            dto ??= new List<TProperty>();

            // Apply addition
            foreach (var dtoPropValListItem in dto.Where(dtoPropValListItem =>
                         !entity.Any(entityPropValListItem =>
                             Equals(idSelector(entityPropValListItem), idSelector(dtoPropValListItem)))).ToList())
            {
                entity.Add(dtoPropValListItem);
            }

            // Apply deletion
            foreach (var entityPropValListItem in entity.Where(entityPropValListItem =>
                         !dto.Any(dtoPropValListItem =>
                             Equals(idSelector(entityPropValListItem), idSelector(dtoPropValListItem)))).ToList())
            {
                entity.Remove(entityPropValListItem);
            }
        }
    }
}