using System;
using System.Collections.Generic;
using Audiotica.Core.Common;

namespace Audiotica.Web.Models
{
    public class WebAlbum : WebItemWithTracks, IConvertibleObject
    {
        public WebAlbum(Type provider) : base(provider)
        {
        }

        public string Title { get; set; }
        public WebArtist Artist { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public Uri Artwork { get; set; }
        public List<string> Genres { get; set; }
        public object PreviousConversion { get; set; }

        public class Comparer : IEqualityComparer<WebAlbum>
        {
            public bool Equals(WebAlbum x, WebAlbum y)
            {
                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(WebAlbum obj)
            {
                return (obj.Title + obj.Artist?.Name + obj.ReleasedDate).GetHashCode();
            }
        }
    }
}