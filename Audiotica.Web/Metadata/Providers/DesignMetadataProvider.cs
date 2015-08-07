using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Web.Models;

namespace Audiotica.Web.Metadata.Providers
{
    public class DesignMetadataProvider : IMetadataProvider
    {
        public string DisplayName { get; } = "Design Time";
        public int Priority { get; } = 10;
        public bool IsEnabled { get; set; } = true;

        public Task<WebResults> SearchAsync(string query, WebResults.Type searchType = WebResults.Type.Song,
            int limit = 10, string pagingToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<WebAlbum> GetAlbumAsync(string albumToken)
        {
            throw new NotImplementedException();
        }

        public Task<WebSong> GetSongAsync(string songToken)
        {
            throw new NotImplementedException();
        }

        public Task<WebArtist> GetArtistAsync(string artistToken)
        {
            return Task.FromResult(CreateDummyArtists()[0]);
        }

        public Task<WebArtist> GetArtistByNameAsync(string artistName)
        {
            return Task.FromResult(CreateDummyArtists()[0]);
        }

        public Task<WebResults> GetTopSongsAsync(int limit = 50, string pageToken = null)
        {
            return Task.FromResult(CreateSongsResult(CreateDummySongs()));
        }

        public Task<WebResults> GetTopAlbumsAsync(int limit = 50, string pageToken = null)
        {
            return Task.FromResult(CreateAlbumsResult(CreateDummyAlbums()));
        }

        public Task<WebResults> GetTopArtistsAsync(int limit = 50, string pageToken = null)
        {
            return Task.FromResult(CreateArtistsResult(CreateDummyArtists()));
        }

        public Task<WebResults> GetArtistTopSongsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            return Task.FromResult(CreateSongsResult(CreateDummySongs()));
        }

        public Task<WebResults> GetArtistAlbumsAsync(string artistToken, int limit = 50, string pageToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Uri> GetArtworkAsync(string album, string artist)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLyricAsync(string song, string artist)
        {
            throw new NotImplementedException();
        }

        private WebArtist CreateArtist(string name, string artwork)
        {
            return new WebArtist(GetType())
            {
                Name = name,
                Artwork = new Uri(artwork)
            };
        }

        private WebAlbum CreateAlbum(string title, WebArtist artist, string artwork, List<WebSong> tracks = null)
        {
            return new WebAlbum(GetType())
            {
                Title = title,
                Artist = artist,
                Artwork = new Uri(artwork),
                Tracks = tracks
            };
        }

        private WebSong CreateSong(string title, string artistName, string albumName, string artwork,
            string artistArtwork)
        {
            var artist = CreateArtist(artistName, artistArtwork);
            return new WebSong(GetType())
            {
                Title = title,
                Album = CreateAlbum(albumName, artist, artwork),
                Artists = new List<WebArtist> {artist}
            };
        }

        private List<WebSong> CreateDummySongs()
        {
            var kauaiArtwork = "https://runthetrap.com/wp-content/uploads/2014/10/stream-childish-gambino-kauai-ep1.jpg";
            var gambinoArtwork =
                "http://static1.1.sqspcdn.com/static/f/362468/13350786/1311549110307/Childish-Gambino.jpg?token=%2F%2FxvckXhTw1vqbFHcrSvgEN5hhE%3D";
            return new List<WebSong>
            {
                CreateSong("Sober", "Childish Gambino", "Kauai", kauaiArtwork, gambinoArtwork)
            };
        }

        private List<WebArtist> CreateDummyArtists()
        {
            var gambinoArtwork =
                "http://static1.1.sqspcdn.com/static/f/362468/13350786/1311549110307/Childish-Gambino.jpg?token=%2F%2FxvckXhTw1vqbFHcrSvgEN5hhE%3D";
            return new List<WebArtist>
            {
                CreateArtist("Childish Gambino", gambinoArtwork),
            };
        }

        private List<WebAlbum> CreateDummyAlbums()
        {
            var kauaiArtwork = "https://runthetrap.com/wp-content/uploads/2014/10/stream-childish-gambino-kauai-ep1.jpg";
            var gambinoArtwork =
                "http://static1.1.sqspcdn.com/static/f/362468/13350786/1311549110307/Childish-Gambino.jpg?token=%2F%2FxvckXhTw1vqbFHcrSvgEN5hhE%3D";
            return new List<WebAlbum>
            {
                CreateAlbum("Kauai", CreateArtist("Childish Gambino", gambinoArtwork), kauaiArtwork, CreateDummySongs())
            };
        }

        private WebResults CreateAlbumsResult(List<WebAlbum> albums)
        {
            return new WebResults
            {
                Albums = albums
            };
        }

        private WebResults CreateSongsResult(List<WebSong> songs)
        {
            return new WebResults
            {
                Songs = songs
            };
        }

        private WebResults CreateArtistsResult(List<WebArtist> artist)
        {
            return new WebResults
            {
                Artists = artist
            };
        }
    }
}