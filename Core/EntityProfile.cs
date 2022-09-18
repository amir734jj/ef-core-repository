using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EfCoreRepository.Abstracts;
using EfCoreRepository.Interfaces;

namespace EfCoreRepository
{
    public abstract class EntityProfile<TSource> : AbstractMappingUtility, IEntityProfile 
        where TSource : class
    {
        private readonly IList<PropertyInfo> _properties = new List<PropertyInfo>();

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
        protected virtual IQueryable<TSource> Include<TQueryable>(TQueryable queryable)
            where TQueryable : IQueryable<TSource>
        {
            return queryable;
        }

        /// <summary>
        /// Utility function to map one property at a time
        /// </summary>
        /// <param name="accessor"></param>
        /// <typeparam name="TProperty"></typeparam>
        protected void Map<TProperty>(Expression<Func<TSource, TProperty>> accessor)
        {
            _properties.Add(PropertyInfoByLinqExpressionVisitor.Instance.GetPropertyInfo(accessor));
        }

        /// <summary>
        /// Utility function to map all properties automatically
        /// <param name="ignored">Ignored properties</param>
        /// </summary>
        protected void MapAll(params Expression<Func<TSource, object>>[] ignored)
        {
            foreach (var propertyInfo in typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(x => x.CanRead && x.CanWrite)
                         .Except(ignored.Select(x => PropertyInfoByLinqExpressionVisitor.Instance.GetPropertyInfo(x))))
            {
               _properties.Add(propertyInfo);
            }
        }

        /// <summary>
        /// Exports to mapping profile
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IEntityMapping ToEntityMapping(IList<Type> entityTypes)
        {
            return new EntityMapping<TSource>(entityTypes, _properties, Update, Include);
        }
    }
}