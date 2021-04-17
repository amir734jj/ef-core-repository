using System;
using System.Collections.Generic;
using System.Linq;
using EfCoreRepository.Interfaces;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    internal class EntityProfileAuxiliary : IEntityProfileAuxiliary
    {
        public IList<TProperty> ModifyList<TProperty, TId>(IList<TProperty> entity, IList<TProperty> dto, Func<TProperty, TId> idSelector)
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

        public IList<TProperty> ModifyList<TProperty, TId>(IList<TProperty> entity, IList<TProperty> dto) where TId: struct where TProperty : class
        {
            return ModifyList(entity, dto, IdAccessExpression<TProperty, TId>().Compile());
        }
    }
}