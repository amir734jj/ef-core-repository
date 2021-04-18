using System;
using System.Collections.Generic;
using System.Linq;
using static EfCoreRepository.EntityUtility;

namespace EfCoreRepository
{
    public abstract class EntityProfile<TSource> where TSource : class
    {
        /// <summary>
        /// Updated entity given dto
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        public abstract void Update(TSource entity, TSource dto);

        /// <summary>
        /// Intercept the IQueryable to include
        /// </summary>
        /// <returns></returns>
        public abstract IQueryable<TSource> Include<TQueryable>(TQueryable queryable) where TQueryable : IQueryable<TSource>;
        
        protected void ModifyList<TList, TProperty, TId>(TList entity, TList dto, Func<TProperty, TId> idSelector)
            where TList: IList<TProperty>, new()
            where TProperty : class
            where TId: struct
        {
            entity ??= new TList();
            dto ??= new TList();
            
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

        protected void ModifyList<T>(List<T> entity, List<T> dto) where T : class
        {
            ModifyList(entity, dto, IdAccessExpression<T, int>().Compile());
        }
    }
}