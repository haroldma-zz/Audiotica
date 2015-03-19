using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.WinRt.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using Audiotica.Data.Model.AudioticaCloud;
using Audiotica.Data.Service.RunTime;

namespace Audiotica.WindowsPhone.Player
{
    internal class RadioStationManager
    {
        private readonly AudioticaService _audioticaService;
        private readonly Func<SqlService> _bgFunc;
        private readonly int _dbId;
        private readonly string _id;
        private readonly Func<SqlService> _sqlFunc;

        public RadioStationManager(string id, int dbId, Func<SqlService> sqlFunc, Func<SqlService> bgFunc)
        {
            _id = id;
            _dbId = dbId;
            _sqlFunc = sqlFunc;
            _bgFunc = bgFunc;
            _audioticaService = new AudioticaService(new PclCredentialHelper(), new AppSettingsHelper());
            QueueSongs = new List<QueueSong>();
            QueueSongsLookup = new Dictionary<int, QueueSong>();
        }

        public Dictionary<int, QueueSong> QueueSongsLookup { get; set; }
        public List<QueueSong> QueueSongs { get; set; }
        public List<RadioSong> Songs { get; set; }

        public async Task LoadTracksAsync()
        {
            var resp = await _audioticaService.StationLookahead(_id);
            if (resp.Success)
                Songs = resp.Data.Songs;
        }

        public async Task UpdateQueueAsync()
        {
            using (var bg = _bgFunc())
            {
                await bg.DeleteTableAsync<QueueSong>();
            }

            using (var service = _sqlFunc())
            {
                foreach (var radioSong in Songs)
                {
                    var song = await service.SelectFirstAsync<Song>(p => p.ProviderId == "gn." + radioSong.Id);

                    var artist =
                        await
                            service.SelectFirstAsync<Artist>(
                                p => p.Name.ToLower() == radioSong.AlbumArtistName.ToLower());
                    var album =
                        await
                            service.SelectFirstAsync<Album>(p => p.Name.ToLower() == radioSong.AlbumName.ToLower());

                    // Already added
                    if (song != null)
                    {
                        song.Artist = artist;
                        song.Album = album;
                        await AddToQueueAsync(song);
                        continue;
                    }

                    song = new Song
                    {
                        Name = radioSong.Name,
                        ArtistName = radioSong.ArtistName,
                        IsTemp = true,
                        RadioId = _dbId,
                        TrackNumber = radioSong.TrackNumber,
                        ProviderId = "gn." + radioSong.Id,
                        SongState = SongState.BackgroundMatching,
                        Album = new Album
                        {
                            Name = radioSong.AlbumName,
                            ProviderId = "gn." + radioSong.AlbumId
                        },
                        Artist = new Artist
                        {
                            Name = radioSong.AlbumArtistName,
                            ProviderId = "gn.track." + radioSong.Id
                        }
                    };

                    if (artist == null)
                    {
                        await service.InsertAsync(song.Artist);
                        song.ArtistId = song.Artist.Id;

                        if (album != null)
                            song.Album = album;
                        song.Album.PrimaryArtistId = song.Artist.Id;
                        song.Album.PrimaryArtist = song.Artist;
                    }
                    else
                    {
                        song.Artist = artist;
                        song.ArtistId = artist.Id;
                    }

                    if (album == null)
                    {
                        await service.InsertAsync(song.Album);
                        song.AlbumId = song.Album.Id;
                    }
                    else
                    {
                        song.Album = album;
                        song.AlbumId = album.Id;
                    }

                    await service.InsertAsync(song);
                    await AddToQueueAsync(song);
                }
            }
        }

        public async Task<bool> MatchSongAsync(Song song)
        {
            var match  = await _audioticaService.GetMatchesAsync(song.Name, song.Artist.Name);

            if (!match.Success || match.Data.Count <= 0) return false;

            using (var sql = _sqlFunc())
            {
                song.SongState = SongState.None;
                song.AudioUrl = match.Data[0].AudioUrl;
                await sql.UpdateItemAsync(song);
            }

            return true;
        }

        private async Task AddToQueueAsync(Song song)
        {
            if (QueueSongs.Any(p => p.SongId == song.Id)) return;

            var prev = QueueSongs.LastOrDefault();

            // Create the new queue entry
            var newQueue = new QueueSong
            {
                SongId = song.Id,
                PrevId = prev == null ? 0 : prev.Id,
                Song = song
            };

            using (var bg = _bgFunc())
            {
                await bg.InsertAsync(newQueue);
                if (prev != null)
                {
                    prev.NextId = newQueue.Id;
                    await bg.UpdateItemAsync(prev);
                }

                QueueSongsLookup.Add(newQueue.Id, newQueue);
                QueueSongs.Add(newQueue);
            }
        }

        public async Task LikeAsync(string trackId)
        {
            var resp = await _audioticaService.StationEvent(_id, RadioEvent.Like, trackId);
            if (resp.Success)
                Songs = resp.Data.Songs;
        }

        public async Task DislikeAsync(string trackId)
        {
            var resp = await _audioticaService.StationEvent(_id, RadioEvent.Dislike, trackId);
            if (resp.Success)
                Songs = resp.Data.Songs;
        }

        public async Task SkippedAsync(string trackId)
        {
            var resp = await _audioticaService.StationEvent(_id, RadioEvent.Skipped, trackId);
            if (resp.Success)
                Songs = resp.Data.Songs;
        }

        public async Task PlayedAsync(string trackId)
        {
            var resp = await _audioticaService.StationEvent(_id, RadioEvent.Played, trackId);
            if (resp.Success)
                Songs = resp.Data.Songs;
        }
    }
}