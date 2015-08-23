using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Audiotica.Core.Extensions
{
    public static class TypeExtensions
    {
        public static List<Type> GetImplementations(this IEnumerable<Type> types, Type desiredType,
            bool excludeAbstracts = true, params Type[] excludeTypes)
        {
            return
                types.Select(p => p.GetTypeInfo())
                    .GetImplementations(desiredType, excludeAbstracts, excludeTypes)
                    .ToList();
        }

        public static List<Type> GetImplementations(this IEnumerable<TypeInfo> types, Type desiredType,
            bool excludeAbstracts = true, params Type[] excludeTypes)
        {
            return
                types.GetImplementations(desiredType.GetTypeInfo(), excludeAbstracts, excludeTypes)
                    .Select(p => p.AsType())
                    .ToList();
        }

        public static List<TypeInfo> GetImplementations(this IEnumerable<TypeInfo> types, TypeInfo desiredType,
            bool excludeAbstracts = true, params Type[] excludeTypes)
        {
            return types.Where(p =>
                desiredType.IsAssignableFrom(p) &&
                (!excludeAbstracts || (!p.IsAbstract && !p.IsInterface)) &&
                !p.Equals(desiredType)
                && (excludeTypes.Length == 0 || excludeTypes.All(m => m != p.AsType()))).ToList();
        }

        /// <summary>
        ///     Sets the property from "from" to "to" using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="to">To.</param>
        /// <param name="from">From.</param>
        /// <param name="excludePropertys">The exclude propertys.</param>
        public static void SetFrom<T>(this T to, T from, params string[] excludePropertys)
        {
            var propertys = to.GetType().GetRuntimeProperties();
            foreach (
                var prop in
                    propertys.Where(prop => prop.CanWrite && prop.CanRead && !excludePropertys.Contains(prop.Name)))
            {
                prop.SetValue(to, prop.GetValue(@from));
            }
        }

        public static T As<T>(this object obj)
        {
            return (T) obj;
        }
    }
}