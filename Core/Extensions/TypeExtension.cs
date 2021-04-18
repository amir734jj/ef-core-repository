using System;

namespace EfCoreRepository.Extensions
{
    internal static class TypeExtension
    {
        public static bool IsTypeCompatibleForId(this Type t)
        {
            return t.IsPrimitive || t.IsValueType || t == typeof(string);
        }
    }
}