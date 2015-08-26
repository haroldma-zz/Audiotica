using Audiotica.Core.Extensions;
using Audiotica.Database.Models;

namespace Audiotica.Database.Extensions
{
    public static class TrackExtensions
    {
        public static string GetAlbumHash(this Track track)
        {
            var id = track.AlbumTitle + track.AlbumArtist;
            return id.ToLower().ToSha1();
        }

        public static string GetArtistHash(this Track track)
        {
            var id = track.DisplayArtist;
            return id.ToLower().ToSha1();
        }
    }
}