using System;
using System.Threading.Tasks;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Models;

namespace Audiotica.Web.Metadata.Providers
{
    /// <summary>
    ///     This base provider is use for lyrics, hence the CollectionSize and CollectionType are set to none.
    /// </summary>
    public abstract class MetadataProviderLyricsOnlyBase : MetadataProviderBase
    {
        protected MetadataProviderLyricsOnlyBase(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public override ProviderCollectionSize CollectionSize => ProviderCollectionSize.None;
        public override ProviderCollectionType CollectionType => ProviderCollectionType.None;

        public override Task<WebResults> SearchAsync(string query, WebResults.Type searchType = WebResults.Type.Song,
            int limit = 20, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public override Task<WebAlbum> GetAlbumAsync(string albumToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WebSong> GetSongAsync(string songToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WebArtist> GetArtistAsync(string artistToken)
        {
            throw new NotImplementedException();
        }

        public override Task<WebArtist> GetArtistByNameAsync(string artistName)
        {
            throw new NotImplementedException();
        }

        public override Task<WebResults> GetTopSongsAsync(int limit = 20, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public override Task<WebResults> GetTopAlbumsAsync(int limit = 20, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public override Task<WebResults> GetTopArtistsAsync(int limit = 20, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public override Task<WebResults> GetArtistTopSongsAsync(string artistToken, int limit = 20,
            string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public override Task<WebResults> GetArtistAlbumsAsync(string artistToken, int limit = 20,
            string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public abstract override Task<string> GetLyricAsync(string song, string artist);
    }
}