using System;
using System.Threading.Tasks;
using Audiotica.Web.Enums;
using Audiotica.Web.Models;

namespace Audiotica.Web.Metadata.Interfaces
{
    public interface IMetadataProvider : IConfigurableProvider
    {
        ProviderSpeed Speed { get; }
        ProviderCollectionSize CollectionSize { get; }
        ProviderCollectionType CollectionQuality { get; }
    }

    public interface IBasicMetadataProvider : IMetadataProvider
    {
        Task<WebAlbum> GetAlbumAsync(string albumToken);
        Task<WebSong> GetSongAsync(string songToken);
        Task<WebArtist> GetArtistAsync(string artistToken);
        Task<WebArtist> GetArtistByNameAsync(string artistName);
        Task<Uri> GetArtworkAsync(string album, string artist);
        Task<Uri> GetArtistArtworkAsync(string artist);
    }

    public interface IExtendedMetadataProvider : IBasicMetadataProvider
    {
        Task<WebResults> GetRelatedArtistsAsync(string artistToken, int limit = 50, string pageToken = null);
        Task<WebResults> GetArtistTopSongsAsync(string artistToken, int limit = 50, string pageToken = null);
        Task<WebResults> GetArtistAlbumsAsync(string artistToken, int limit = 50, string pageToken = null);
        Task<WebResults> GetArtistNewAlbumsAsync(string artistToken, int limit = 50, string pageToken = null);
    }

    public interface IChartMetadataProvider : IBasicMetadataProvider
    {
        Task<WebResults> GetTopSongsAsync(int limit = 50, string pageToken = null);
        Task<WebResults> GetTopAlbumsAsync(int limit = 50, string pageToken = null);
        Task<WebResults> GetTopArtistsAsync(int limit = 50, string pageToken = null);
    }

    public interface ISearchMetadataProvider : IBasicMetadataProvider
    {
        Task<WebResults> SearchAsync(string query, WebResults.Type searchType = WebResults.Type.Song,
            int limit = 10, string pageToken = null);
    }

    public interface ILyricsMetadataProvider : IMetadataProvider
    {
        Task<string> GetLyricAsync(string song, string artist);
    }
}