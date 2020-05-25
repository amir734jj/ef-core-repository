using System;
using System.Collections.Generic;
using System.Linq;
using EfCoreRepository.Interfaces;

namespace EfCoreRepository
{
    internal class EntityProfileAuxiliary : IEntityProfileAuxiliary
    {
        public List<TProperty> ModifyList<TProperty, TId>(List<TProperty> entity, List<TProperty> dto, Func<TProperty, TId> idSelector)
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

            return entity;
        }

        public List<TProperty> ModifyList<TProperty, TId>(List<TProperty> entity, List<TProperty> dto) where TProperty : IEntity<TId>
        {
            return ModifyList(entity, dto, x => x.Id);
        }
    }
}