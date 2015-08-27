using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.LyricsFind.Models;

namespace Audiotica.Web.Http.Requets.Metadata.LyricsFind
{
    public class LyricsFindRequest : RestObjectRequest<LyricsFindResponse>
    {
        public LyricsFindRequest(string title, string artist)
        {
            this.Url(
                "http://api.lyricfind.com/lyric.do")
                .Param("count", 1)
                .Param("reqtype", "offlineviews")
                .Param("output", "json")
                .Param("useragent", "Lyrically/3.1.0 (iPhone; iOS 8.4; Scale/2.00)")
                .Param("apikey", "e78b5a84c31a1e23e6997ad8908bbd4a")
                .Param("trackid", $"artistname:{Escape(artist)},trackname:{Escape(title)}");
        }

        private string Escape(string text)
        {
            return text.Replace(",", " %252");
        }
    }
}