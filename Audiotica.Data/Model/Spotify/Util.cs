using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;

namespace Audiotica.Data.Model.Spotify
{
    public static class Util
    {
        public static string GetSearchValue(this SearchType en, String separator)
        {
            IEnumerable<StringAttribute> attributes =
            Enum.GetValues(typeof(SearchType))
            .Cast<SearchType>()
            .Where(v => en.HasFlag(v))
            .Select(v => typeof(SearchType).GetRuntimeField(v.ToString()))
            .Select(f => f.GetCustomAttributes(typeof(StringAttribute), false).FirstOrDefault())
            .Cast<StringAttribute>();

            var list = new List<String>();
            attributes.ToObservable().Subscribe(x => list.Add(x.Text));
            return string.Join(" ", list);
        }
        public static string GetAlbumValue(this AlbumType en, String separator)
        {
            IEnumerable<StringAttribute> attributes =
            Enum.GetValues(typeof(AlbumType))
            .Cast<AlbumType>()
            .Where(v => en.HasFlag(v))
            .Select(v => typeof(AlbumType).GetRuntimeField(v.ToString()))
            .Select(f => f.GetCustomAttributes(typeof(StringAttribute), false).FirstOrDefault())
            .Cast<StringAttribute>();

            var list = new List<String>();
            attributes.ToObservable().Subscribe((element) => list.Add(element.Text));
            return string.Join(" ", list);
        }
    }
}
