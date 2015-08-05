using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Database.Models;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;

namespace Audiotica.Factory
{
    public class TrackToWebSongConverter : IConverter<Track, WebSong>
    {
        private readonly IMetadataProvider[] _providers;

        public TrackToWebSongConverter(IMetadataProvider[] providers)
        {
            _providers = providers;
        }

        public async Task<Track> ConvertAsync(WebSong other)
        {
            var provider = _providers.FirstOrDefault(p => p.GetType() == other.MetadataProvider);

            if (other.IsPartial)
                other = await provider.GetSongAsync(other.Token);
            if (other.Album.IsPartial)
                other.Album = await provider.GetAlbumAsync(other.Album.Token);
            if (other.Album.Artist.IsPartial)
                other.Album.Artist = await provider.GetArtistAsync(other.Album.Artist.Token);
            if (other.Artists[0].Token != other.Album.Artist.Token 
                && other.Artists[0].IsPartial)
                other.Artists[0] = await provider.GetArtistAsync(other.Artists[0].Token);

            return new Track
            {
                Title = other.Title,
                Artists = string.Join(";", other.Artists.Select(p => p.Name)),
                AlbumTitle = other.Album.Name,
                AlbumArtist = other.Album.Artist.Name,
                ArtworkUri = other.Album.Artwork,
                ArtistArtworkUri = other.Artists[0].Artwork,
                DisplayArtist = other.Artists[0].Name,
                TrackNumber = other.TrackNumber,
                DiscNumber = other.DiscNumber,
                Year = other.Album.ReleasedDate?.Year ?? 0,
                TrackCount = other.Album.Tracks.Count,
                Genres = other.Genres,
                Type = Track.TrackType.Stream,
            };
        }
    }
}