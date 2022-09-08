using System;
using System.Collections.Generic;

namespace EfCoreRepository.Extensions
{
    internal static class TypeExtension
    {
        public static bool IsTypeCompatibleForId(this Type t)
        {
            return t.IsPrimitive || t.IsValueType || t == typeof(string);
        }
        
        /// <summary>
        /// Utility function that returns true if property type if of IList
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsGenericList(this Type type)
        {
            if (!type.IsGenericType)
                return false;
            var genericArguments = type.GetGenericArguments();
            if (genericArguments.Length != 1)
                return false;

            var listType = typeof (IList<>).MakeGenericType(genericArguments);
            return listType.IsAssignableFrom(type);
        }
    }
}