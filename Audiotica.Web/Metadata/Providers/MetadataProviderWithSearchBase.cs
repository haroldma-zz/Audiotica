using System;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;

namespace Audiotica.Web.Metadata.Providers
{
    public abstract class MetadataProviderWithSearchBase : MetadataProviderBase, ISearchMetadataProvider
    {
        protected MetadataProviderWithSearchBase(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public abstract Task<WebAlbum> GetAlbumAsync(string albumToken);
        public abstract Task<WebSong> GetSongAsync(string songToken);
        public abstract Task<WebArtist> GetArtistAsync(string artistToken);
        public abstract Task<WebArtist> GetArtistByNameAsync(string artistName);

        public abstract Task<WebResults> SearchAsync(string query, WebResults.Type searchType = WebResults.Type.Song,
            int limit = 20, string pageToken = null);

        public virtual async Task<Uri> GetArtworkAsync(string album, string artist)
        {
            var results = await SearchAsync(album.Append(artist), WebResults.Type.Album, 1);
            var albumMatch = results.Albums?.FirstOrDefault();

            if (albumMatch == null)
                return null;

            if (albumMatch.Title.ToAudioticaSlug() != album.ToAudioticaSlug() ||
                albumMatch.Artist.Name.ToAudioticaSlug() != artist.ToAudioticaSlug())
                return null;

            return albumMatch.Artwork;
        }

        public virtual async Task<Uri> GetArtistArtworkAsync(string artist)
        {
            var results = await SearchAsync(artist, WebResults.Type.Artist, 1);
            var artistMatch = results.Artists?.FirstOrDefault();

            if (artistMatch == null)
                return null;

            return artistMatch.Name.ToAudioticaSlug() != artist.ToAudioticaSlug() ? null : artistMatch.Artwork;
        }
    }
}