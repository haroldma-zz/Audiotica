using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Core.Interfaces.Utilities;
using Audiotica.Web.Enums;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;

namespace Audiotica.Web.Metadata.Providers
{
    public abstract class MetadataProviderBase : IMetadataProvider
    {
        private readonly ISettingsUtility _settingsUtility;

        protected MetadataProviderBase(ISettingsUtility settingsUtility)
        {
            _settingsUtility = settingsUtility;
        }

        public abstract ProviderSpeed Speed { get; }
        public abstract ProviderCollectionSize CollectionSize { get; }
        public abstract ProviderCollectionType CollectionType { get; }
        public abstract string DisplayName { get; }
        public int Priority => 10;

        public bool IsEnabled
        {
            get { return _settingsUtility.Read($"metadata_provider_enabled_{DisplayName}", true); }

            set { _settingsUtility.Write($"metadata_provider_enabled_{DisplayName}", value); }
        }

        public abstract Task<WebResults> SearchAsync(string query,
            WebResults.Type searchType = WebResults.Type.Song, int limit = 10, string pagingToken = null);

        public abstract Task<WebAlbum> GetAlbumAsync(string albumToken);
        public abstract Task<WebSong> GetSongAsync(string songToken);
        public abstract Task<WebArtist> GetArtistAsync(string artistToken);
        public abstract Task<List<WebSong>> GetArtistTopSongsAsync(string artistToken);
        public abstract Task<List<WebAlbum>> GetArtistTopAlbumsAsync(string artistToken);

        public virtual async Task<Uri> GetArtworkAsync(string album, string artist)
        {
            var results = await SearchAsync(album.Append(artist), WebResults.Type.Album, 1);
            var albumMatch = results.Albums?.FirstOrDefault();

            if (albumMatch == null)
                return null;

            if (albumMatch.Name.ToAudioticaSlug() != album.ToAudioticaSlug() ||
                albumMatch.Artist.Name.ToAudioticaSlug() != artist.ToAudioticaSlug())
                return null;

            return albumMatch.Artwork;
        }

        public abstract Task<string> GetLyricAsync(string song, string artist);
    }
}