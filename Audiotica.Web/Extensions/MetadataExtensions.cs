using System.Collections.Generic;
using System.Linq;
using Audiotica.Web.Metadata.Interfaces;

namespace Audiotica.Web.Extensions
{
    public static class MetadataExtensions
    {
        public static List<T> FilterAndSort<T>(this IEnumerable<IMetadataProvider> providers)
        {
            return providers.Where(p => p.IsEnabled)
                .OrderByDescending(p => p.Priority)
                .Where(p => p is T)
                .Cast<T>()
                .ToList();
        }
    }
}