#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Data.Service.Interfaces;
using Audiotica.Data.Spotify;
using Audiotica.Data.Spotify.Models;

#endregion

namespace Audiotica.Data.Service.RunTime
{
    public class SpotifyService : ISpotifyService
    {
        private readonly SpotifyWebApi _spotify;

        public SpotifyService(SpotifyWebApi spotify)
        {
            _spotify = spotify;
        }

        public Task<List<ChartTrack>> GetViralTracksAsync(string market = "US", string time = "weekly")
        {
            return _spotify.GetViralTracks(market, time);
        }

        public Task<List<ChartTrack>> GetMostStreamedTracksAsync(string market = "US", string time = "weekly")
        {
            return _spotify.GetMostStreamedTracks(market, time);
        }

        public Task<FullArtist> GetArtistAsync(string id)
        {
            return _spotify.GetArtist(id);
        }

        public async Task<List<FullTrack>> GetArtistTracksAsync(string id)
        {
            var tracks = (await _spotify.GetArtistsTopTracks(id, "US")).Tracks;
            RemoveDuplicates(tracks);
            return tracks;
        }

        public async Task<Paging<SimpleAlbum>> GetArtistAlbumsAsync(string id)
        {
            var albumPaging = await _spotify.GetArtistsAlbums(id, AlbumType.ALBUM | AlbumType.COMPILATION | AlbumType.SINGLE, limit: 50, market: "US");
            RemoveDuplicates(albumPaging.Items);
            return albumPaging;
        }

        public Task<FullAlbum> GetAlbumAsync(string id)
        {
            return _spotify.GetAlbum(id);
        }

        public Task<Paging<SimpleTrack>> GetAlbumTracksAsync(string id)
        {
            return _spotify.GetAlbumTracks(id);
        }

        public async Task<Paging<FullTrack>> SearchTracksAsync(string query, int limit = 20, int offset = 0)
        {
            var results = await _spotify.SearchItems(query, SearchType.TRACK, limit, offset);
            RemoveDuplicates(results.Tracks.Items);
            return results.Tracks;
        }

        public async Task<Paging<FullArtist>> SearchArtistsAsync(string query, int limit = 20, int offset = 0)
        {
            var results = await _spotify.SearchItems(query, SearchType.ARTIST, limit, offset);
            return results.Artists;
        }

        public async Task<Paging<SimpleAlbum>> SearchAlbumsAsync(string query, int limit = 20, int offset = 0)
        {
            var results = await _spotify.SearchItems(query, SearchType.ALBUM, limit, offset);
            RemoveDuplicates(results.Albums.Items);
            return results.Albums;
        }

        public void RemoveDuplicates(List<SimpleAlbum> albums)
        {
            var toRemove = new Dictionary<string, List<SimpleAlbum>>();

            foreach (var album in albums)
            {
                var duplicate = albums.Where(p => p.Name == album.Name).ToList();

                if (duplicate.Count <= 1) continue;

                //the first album should be kept
                duplicate.Remove(album);

                //mark the rest for deletion
                if (!toRemove.ContainsKey(album.Name))
                    toRemove.Add(album.Name, duplicate);
            }
            
            foreach (var album in toRemove.SelectMany(remove => remove.Value))
            {
                albums.Remove(album);
            }
        }

        public void RemoveDuplicates(List<FullTrack> tracks)
        {
            var toRemove = new Dictionary<string, List<FullTrack>>();

            foreach (var track in tracks)
            {
                var duplicate = tracks.Where(p => 
                    p.Name == track.Name 
                    && p.Artist.Name == track.Artist.Name
                    && p.Album.Name == track.Album.Name).ToList();

                if (duplicate.Count <= 1) continue;

                //the first album should be kept
                duplicate.Remove(track);

                var key = track.Name + track.Artist.Name + track.Album.Name;

                //mark the rest for deletion
                if (!toRemove.ContainsKey(key))
                    toRemove.Add(key, duplicate);
            }

            foreach (var album in toRemove.SelectMany(remove => remove.Value))
            {
                tracks.Remove(album);
            }
        }
    }
}