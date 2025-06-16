using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using EfCoreRepository.Extensions;

namespace EfCoreRepository
{
    internal static class EntityUtility
    {
        private static readonly ConditionalWeakTable<Type, string> IdLookup = new();

        // Common property names for ID
        private static readonly string[] IdNames = ["_id", "id"];

        // Finds ID property of a class
        private static string FindIdPropertyInternal(Type type)
        {
            if (IdLookup.TryGetValue(type, out var value))
            {
                return value;
            }

            var properties = type
                .GetProperties(BindingFlags.Public |
                               BindingFlags.GetProperty |
                               BindingFlags.SetProperty |
                               BindingFlags.Instance);

            var keyProperty = properties.FirstOrDefault(x => x.GetCustomAttribute<KeyAttribute>() != null) ??
                              properties.FirstOrDefault(x => IdNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase) && x.PropertyType.IsTypeCompatibleForId());

            if (keyProperty == null)
            {
                throw new Exception(
                    $"Missing KEY attribute on the class declaration for nested entity: {type.Name}");
            }

            IdLookup.Add(type, keyProperty.Name);

            return keyProperty.Name;
        }
        
        // Finds ID property of a class
        public static string FindIdProperty(Type type)
        {
           return FindIdPropertyInternal(type);
        }
        
        // Finds ID property of a class
        public static string FindIdProperty<T>() where T : class
        {
            return FindIdPropertyInternal(typeof(T));
        }

        // Creates a lambda expression of x => x.Id == id
        public static Expression<Func<T, bool>> FilterExpression<T, TId>(params TId[] ids)
            where T : class
            where TId : struct
        {
            var parameter = Expression.Parameter(typeof(T));

            Expression body;

            switch (ids.Length)
            {
                case 0:
                    body = Expression.Constant(true);
                    break;
                case 1:
                    body = Expression.Equal(Expression.PropertyOrField(parameter, FindIdProperty<T>()),
                        Expression.Constant(ids.First()));
                    break;
                default:
                    var method = typeof(Enumerable)
                        .GetRuntimeMethods()
                        .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

                    var containsMethod = method.MakeGenericMethod(typeof(TId));
                    var containsInvoke = Expression
                        .Call(containsMethod, Expression.Constant(ids),
                            Expression.PropertyOrField(parameter, FindIdProperty<T>()));

                    body = containsInvoke;
                    break;
            }

            var expression = Expression.Lambda<Func<T, bool>>(body, parameter);

            return expression;
        }
    }
}