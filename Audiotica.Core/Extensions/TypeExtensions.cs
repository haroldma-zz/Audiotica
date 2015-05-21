using System.Linq;
using System.Reflection;

namespace Audiotica.Core.Extensions
{
    public static class TypeExtensions
    {
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
            foreach (var prop in propertys.Where(prop => prop.CanWrite && prop.CanRead && !excludePropertys.Contains(prop.Name)))
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