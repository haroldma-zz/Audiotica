using System;
using System.Linq;
using Audiotica.Web.Extensions;
using Audiotica.Web.Http.Requets.Metadata.Spotify.Models;

namespace Audiotica.Web.Http.Requets.Metadata.Spotify
{
    public class SpotifyArtistAlbumsRequest : RestObjectRequest<Paging<SimpleAlbum>>
    {
        public SpotifyArtistAlbumsRequest(string id)
        {
            this.ConfigureSpotify("artists/{id}/albums")
                .UrlParam("id", id).Market("US").Offset(0).Limit(50);
        }

        public SpotifyArtistAlbumsRequest Offset(int offset)
        {
            return this.QParam("offset", offset);
        }

        public SpotifyArtistAlbumsRequest Limit(int limit)
        {
            return this.QParam("limit", limit);
        }

        public SpotifyArtistAlbumsRequest Types(AlbumType type)
        {
            var types =
                Enum.GetValues(typeof (AlbumType))
                    .Cast<AlbumType>()
                    .Where(v => type.HasFlag(v))
                    .Select(p => p.ToString().ToLower())
                    .ToList();

            return this.QParam("album_type", string.Join(",", types));
        }

        public SpotifyArtistAlbumsRequest Market(string market)
        {
            return this.QParam("market", market);
        }
    }

    [Flags]
    public enum AlbumType
    {
        Album = 1,
        Single = 2,
        Compilation = 4,
        // ReSharper disable once InconsistentNaming
        Appears_On = 8
    }
}