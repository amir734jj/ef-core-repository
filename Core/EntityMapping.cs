using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EfCoreRepository.Abstracts;
using EfCoreRepository.Extensions;
using EfCoreRepository.Interfaces;

namespace EfCoreRepository
{
    internal class EntityMapping<TSource> : AbstractMappingUtility, IEntityMapping where TSource : class
    {
        private readonly Action<TSource, TSource> _manualUpdate;
        private readonly Func<IQueryable<TSource>, IQueryable<TSource>> _include;

        private readonly IDictionary<PropertyInfo, Action<TSource, TSource>> _updates;

        private EntityMapping()
        {
            _updates = new ConcurrentDictionary<PropertyInfo, Action<TSource, TSource>>();
        }

        public EntityMapping(
            ICollection<Type> entityTypes,
            IEnumerable<PropertyInfo> autoMappingProperties,
            Action<TSource, TSource> manualUpdate,
            Func<IQueryable<TSource>, IQueryable<TSource>> include) : this()
        {
            _manualUpdate = manualUpdate;
            _include = include;

            // Exclude:
            // 1) Ids
            // 2) other entity types
            foreach (var propertyInfo in autoMappingProperties
                         .Where(x => !x.Name.Equals(EntityUtility.FindIdProperty(typeof(TSource))))
                         .Where(x => !entityTypes.Contains(x.PropertyType)))
            {
                var paramExpr = Expression.Parameter(typeof(TSource));
                var bodyExpr = Expression.MakeMemberAccess(paramExpr, propertyInfo);
                var memberAccessorExpr = Expression.Lambda(bodyExpr, paramExpr);

                MapUntyped(propertyInfo, memberAccessorExpr);
            }
        }

        public void Update(object entityUntyped, object dtoUntyped)
        {
            if (entityUntyped is TSource entity && dtoUntyped is TSource dto)
            {
                foreach (var (_, update) in _updates)
                {
                    update(entity, dto);
                }

                _manualUpdate(entity, dto);
            }
            else
            {
                throw new ArgumentException($"Entity and/or Dto are not type of ${typeof(TSource).Name}");
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
                var idPropertyInfo = genericArgType.GetProperty(EntityUtility.FindIdProperty(genericArgType) ??
                                                                throw new Exception(
                                                                    $"Missing KEY attribute on the class declaration for nested entity: {genericArgType.Name}"))
                    !;
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

        public IQueryable Include(IQueryable queryableUntyped)
        {
            if (queryableUntyped is IQueryable<TSource> queryable)
            {
                return _include(queryable);
            }

            throw new ArgumentException($"Queryable is not type of ${typeof(IQueryable<TSource>).Name}");
        }
    }
}