using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EfCoreRepository.Extensions;

namespace EfCoreRepository
{
    public abstract class EntityProfile<TSource> where TSource : class
    {
        private readonly IDictionary<PropertyInfo, Action<TSource, TSource>> _updates =
            new ConcurrentDictionary<PropertyInfo, Action<TSource, TSource>>();

        /// <summary>
        /// Updated entity given dto
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        public void UpdateInternal(TSource entity, TSource dto)
        {
            foreach (var (_, update) in _updates)
            {
                update(entity, dto);
            }

            Update(entity, dto);
        }

        /// <summary>
        /// Updated entity given dto
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        protected virtual void Update(TSource entity, TSource dto)
        {
        }

        /// <summary>
        /// Intercept the IQueryable to include
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<TSource> Include<TQueryable>(TQueryable queryable)
            where TQueryable : IQueryable<TSource>
        {
            return queryable;
        }

        /// <summary>
        /// Utility that applies addition/deletion to the list
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        /// <param name="idSelector"></param>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TId"></typeparam>
        protected void ModifyList<TProperty, TId>(IList<TProperty> entity, IList<TProperty> dto,
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

        /// <summary>
        /// Internal utility function that creates map function given property info and access expression
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="accessor"></param>
        private void MapUntyped(PropertyInfo propertyInfo, Expression accessor)
        {
            var param1Expr = Expression.Parameter(typeof(TSource));
            var param2Expr = Expression.Parameter(typeof(TSource));
            var param1AccessExpr = Expression.Invoke(accessor, param1Expr);
            var param2AccessExpr = Expression.Invoke(accessor, param2Expr);

            if (propertyInfo.PropertyType.IsGenericList())
            {
                var genericArgType = propertyInfo.PropertyType.GetGenericArguments()[0];
                var idPropertyInfo = genericArgType.GetProperty(EntityUtility.FindIdPropertyInternal(genericArgType))!;
                var genericMethod = GetType()
                    .GetMethod(nameof(ModifyList), BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.MakeGenericMethod(genericArgType, idPropertyInfo.PropertyType);

                var genericParamExpr = Expression.Parameter(genericArgType);
                var genericBodyExpr = Expression.MakeMemberAccess(genericParamExpr, idPropertyInfo);
                var memberAccessExpr = Expression.Lambda(genericBodyExpr, genericParamExpr);

                var lambdaExpr = Expression.Call(Expression.Constant(this), genericMethod!, param1AccessExpr,
                    param2AccessExpr, Expression.Constant(memberAccessExpr.Compile()));

                var wrapperLambdaExpr = Expression.Lambda<Action<TSource, TSource>>(lambdaExpr, param1Expr, param2Expr);
                _updates.Add(propertyInfo, wrapperLambdaExpr.Compile());
            }
            else
            {
                var bodyExpr = Expression.Call(param1Expr, propertyInfo.GetSetMethod(), param2AccessExpr);

                var lambdaExpr = Expression.Lambda<Action<TSource, TSource>>(bodyExpr, param1Expr, param2Expr);
                _updates.Add(propertyInfo, lambdaExpr.Compile());
            }
        }

        /// <summary>
        /// Utility function to map one property at a time
        /// </summary>
        /// <param name="accessor"></param>
        /// <typeparam name="TProperty"></typeparam>
        protected void Map<TProperty>(Expression<Func<TSource, TProperty>> accessor)
        {
            MapUntyped((PropertyInfo)(accessor.Body as MemberExpression)!.Member, accessor);
        }

        /// <summary>
        /// Utility function to map all properties automatically
        /// <param name="ignored">Ignored properties</param>
        /// </summary>
        protected void MapAll(params Expression<Func<TSource, object>>[] ignored)
        {
            foreach (var propertyInfo in typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(x => x.CanRead && x.CanWrite)
                         .Where(x => !x.Name.Equals(EntityUtility.FindIdPropertyInternal(typeof(TSource))))
                         .Except(ignored.Select(x => (PropertyInfo)(x.Body as MemberExpression)!.Member)))
            {
                var paramExpr = Expression.Parameter(typeof(TSource));
                var bodyExpr = Expression.MakeMemberAccess(paramExpr, propertyInfo);
                var memberAccessorExpr = Expression.Lambda(bodyExpr, paramExpr);

                MapUntyped(propertyInfo, memberAccessorExpr);
            }
        }
    }
}