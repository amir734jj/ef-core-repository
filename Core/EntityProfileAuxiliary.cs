using System.Collections.Generic;
using System.Linq;
using EfCoreRepository.Interfaces;

namespace EfCoreRepository
{
    internal class EntityProfileAuxiliary<TProperty, TId> : IEntityProfileAuxiliary<TProperty, TId>
        where TProperty : class, IEntity<TId>
    {
        public List<TProperty> ModifyList(List<TProperty> entity, List<TProperty> dto)
        {
            entity ??= new List<TProperty>();
            dto ??= new List<TProperty>();
            
            // Apply addition
            foreach (var dtoPropValListItem in dto.Where(dtoPropValListItem =>
                !entity.Any(entityPropValListItem =>
                    Equals(entityPropValListItem.Id, dtoPropValListItem.Id))).ToList())
            {
                entity.Add(dtoPropValListItem);
            }

            // Apply deletion
            foreach (var entityPropValListItem in entity.Where(entityPropValListItem =>
                !dto.Any(dtoPropValListItem =>
                    Equals(entityPropValListItem.Id, dtoPropValListItem.Id))).ToList())
            {
                entity.Remove(entityPropValListItem);
            }

            return entity;
        }
    }
}