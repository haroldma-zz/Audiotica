using Audiotica.Web.Extensions;

namespace Audiotica.Web.Http.Requets.Metadata.Google
{
    internal class MusixMatchLyricsRequest : MusixMatchAuthRequest
    {
        public MusixMatchLyricsRequest(string track, string artist) : base("macro.subtitles.get")
        {
            this.QParam("q_artist", artist)
                .QParam("q_track", track);
        }
    }
}