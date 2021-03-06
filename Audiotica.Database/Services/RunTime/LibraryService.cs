﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Extensions;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using SQLite.Net;

namespace Audiotica.Database.Services.RunTime
{
    public class LibraryService : ILibraryService, IDisposable
    {
        private readonly IDispatcherUtility _dispatcherUtility;
        private readonly SQLiteConnection _sqLiteConnection;

        public LibraryService(
            SQLiteConnection sqLiteConnection,
            IDispatcherUtility dispatcherUtility)
        {
            _sqLiteConnection = sqLiteConnection;
            _dispatcherUtility = dispatcherUtility;
        }

        public event EventHandler Loaded;

        public OptimizedObservableCollection<Album> Albums { get; } =
            new OptimizedObservableCollection<Album>();

        public OptimizedObservableCollection<Artist> Artists { get; } =
            new OptimizedObservableCollection<Artist>();

        public bool IsLoaded { get; private set; }

        public OptimizedObservableCollection<Playlist> Playlists { get; } =
            new OptimizedObservableCollection<Playlist>();

        public OptimizedObservableCollection<Track> Tracks { get; } =
            new OptimizedObservableCollection<Track>();

        public void AddTrack(Track track)
        {
            var existing = Find(track) != null;
            if (existing)
            {
                throw new LibraryException("Track already saved.");
            }

            track.Id = 0;
            _sqLiteConnection.Insert(track);

            _dispatcherUtility.Run(() =>
                {
                    CreateRelatedObjects(track);
                    track.IsFromLibrary = true;
                    Tracks.Add(track);
                });
        }

        public Task AddTrackAsync(Track track)
        {
            return Task.Factory.StartNew(() => AddTrack(track));
        }

        public void DeleteTrack(Track track)
        {
            _sqLiteConnection.Delete(track);
            _dispatcherUtility.Run(() =>
                {
                    DeleteRelatedObjects(track);
                    Tracks.Remove(track);
                    track.IsFromLibrary = false;
                });
        }

        public async Task DeleteTrackAsync(Track track)
        {
            await Task.Factory.StartNew(() => _sqLiteConnection.Delete(track));
            await _dispatcherUtility.RunAsync(() =>
                {
                    DeleteRelatedObjects(track);
                    Tracks.Remove(track);
                    track.IsFromLibrary = false;
                });
        }

        public void Dispose()
        {
            _sqLiteConnection.Dispose();
        }

        public Track Find(long id)
        {
            if (!IsLoaded)
            {
                throw new NotLoadedException();
            }
            return Tracks.FirstOrDefault(p => p.Id == id);
        }

        public Track Find(Track track)
        {
            if (!IsLoaded)
            {
                throw new NotLoadedException();
            }
            return Tracks.FirstOrDefault(p => TrackComparer.AreEqual(p, track));
        }

        public void Load()
        {
            if (IsLoaded)
            {
                return;
            }

            CreateTables();
            LoadTracks();
            LoadPlaylists();

            IsLoaded = true;
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        public Task LoadAsync()
        {
            return Task.Factory.StartNew(Load);
        }

        public void UpdateTrack(Track track)
        {
            _sqLiteConnection.Update(track);
        }

        public Task UpdateTrackAsync(Track track)
        {
            return Task.Factory.StartNew(() => UpdateTrack(track));
        }

        private void CreateAlbum(Track track, Artist albumArtist)
        {
            var album = Albums.FirstOrDefault(p =>
                p.Title.EqualsIgnoreCase(track.AlbumTitle) && p.Artist == albumArtist);
            if (album == null)
            {
                album = new Album
                {
                    Title = track.AlbumTitle,
                    Artist = albumArtist,
                    ArtworkUri = track.ArtworkUri,
                    Year = track.Year,
                    Copyright = track.Copyright,
                    Publisher = track.Publisher
                };
                Albums.Add(album);
                albumArtist.Albums.Add(album);
            }
            else if (album.ArtworkUri == null)
            {
                album.ArtworkUri = track.ArtworkUri;
            }

            var sort = album.Tracks.ToList();
            sort.Add(track);
            sort.Sort(
                (track1, track2) =>
                    (int)track1.TrackNumber + (int)track1.DiscCount - (int)(track2.TrackNumber + track2.DiscCount));
            var index = sort.IndexOf(track);
            album.Tracks.Insert(index, track);
        }

        private void CreateDisplayArtist(Track track)
        {
            var displayArtist = Artists.FirstOrDefault(p =>
                p.Name.EqualsIgnoreCase(track.DisplayArtist));
            var newRelation = displayArtist == null;

            if (newRelation)
            {
                displayArtist = new Artist
                {
                    Name = track.DisplayArtist,
                    ArtworkUri = track.ArtistArtworkUri
                };
            }
            else if (displayArtist.ArtworkUri == null)
            {
                displayArtist.ArtworkUri = track.ArtistArtworkUri;
            }

            displayArtist.Tracks.Add(track);
            if (newRelation)
            {
                Artists.Add(displayArtist);
            }
        }

        private void CreateRelatedObjects(Track track)
        {
            var displaySameAsAlbumArtist = track.DisplayArtist.EqualsIgnoreCase(track.AlbumArtist);

            var albumArtist = Artists.FirstOrDefault(p =>
                p.Name.EqualsIgnoreCase(track.AlbumArtist));
            var newRelation = albumArtist == null;

            if (newRelation)
            {
                albumArtist = new Artist
                {
                    Name = track.AlbumArtist,
                    ArtworkUri = track.AlbumArtist == track.DisplayArtist ? track.ArtistArtworkUri : null
                };
            }
            else if (albumArtist.ArtworkUri == null && displaySameAsAlbumArtist)
            {
                albumArtist.ArtworkUri = track.ArtistArtworkUri;
            }

            albumArtist.Tracks.Add(track);
            if (newRelation)
            {
                Artists.Add(albumArtist);
            }

            CreateAlbum(track, albumArtist);

            if (!displaySameAsAlbumArtist)
            {
                CreateDisplayArtist(track);
            }

            CreateSecondaryArtists(track);
        }

        private void CreateSecondaryArtists(Track track)
        {
            var artistAppearing = track.Artists.Split(';').Select(p => p.Trim());

            foreach (var artistName in artistAppearing
                .Where(p => !p.EqualsIgnoreCase(track.DisplayArtist)
                    && !p.EqualsIgnoreCase(track.AlbumArtist)))
            {
                var artist = Artists.FirstOrDefault(p =>
                    p.Name.EqualsIgnoreCase(artistName));
                if (artist == null)
                {
                    artist = new Artist
                    {
                        Name = artistName
                    };
                    Artists.Add(artist);
                }

                artist.TracksThatAppearsIn.Add(track);
            }
        }

        private void CreateTables()
        {
            // The only object used so far.
            _sqLiteConnection.CreateTable<Track>();
        }

        private void DeleteAlbum(Track track, Artist albumArtist)
        {
            var album = Albums.FirstOrDefault(p =>
                p.Title.EqualsIgnoreCase(track.AlbumTitle) && p.Artist == albumArtist);
            if (album == null)
            {
                return;
            }
            album.Tracks.Remove(track);
            if (album.Tracks.Count == 0)
            {
                Albums.Remove(album);
                albumArtist.Albums.Remove(album);
            }
        }

        private void DeleteDisplayArtist(Track track)
        {
            var displayArtist = Artists.FirstOrDefault(p =>
                p.Name.EqualsIgnoreCase(track.DisplayArtist));

            displayArtist.Tracks.Remove(track);
            if (displayArtist.TracksThatAppearsIn.Count == 0 && displayArtist.Tracks.Count == 0)
            {
                Artists.Remove(displayArtist);
            }
        }

        private void DeleteRelatedObjects(Track track)
        {
            var displaySameAsAlbumArtist = track.DisplayArtist.EqualsIgnoreCase(track.AlbumArtist);

            var albumArtist = Artists.FirstOrDefault(p =>
                p.Name.EqualsIgnoreCase(track.AlbumArtist));

            if (albumArtist != null)
            {
                albumArtist.Tracks.Remove(track);
                if (albumArtist.Tracks.Count == 0 && albumArtist.TracksThatAppearsIn.Count == 0)
                {
                    Artists.Remove(albumArtist);
                }
            }

            DeleteAlbum(track, albumArtist);

            if (!displaySameAsAlbumArtist)
            {
                DeleteDisplayArtist(track);
            }

            DeleteSecondaryArtists(track);
        }

        private void DeleteSecondaryArtists(Track track)
        {
            var artistAppearing = track.Artists.Split(';').Select(p => p.Trim());

            foreach (var artist in artistAppearing
                .Where(p => !p.EqualsIgnoreCase(track.DisplayArtist)
                    && !p.EqualsIgnoreCase(track.AlbumArtist)).Select(artistName => Artists.First(p =>
                        p.Name.EqualsIgnoreCase(artistName))))
            {
                artist.TracksThatAppearsIn.Remove(track);

                if (artist.TracksThatAppearsIn.Count == 0 && artist.Tracks.Count == 0)
                {
                    Artists.Remove(artist);
                }
            }
        }

        private void LoadPlaylists()
        {
        }

        private void LoadTracks()
        {
            var tracks = _sqLiteConnection.Table<Track>().ToList();

            foreach (var track in tracks)
            {
                track.IsFromLibrary = true;
                CreateRelatedObjects(track);
                Tracks.Add(track);
            }
        }
    }

    public class NotLoadedException : LibraryException
    {
        public NotLoadedException() : base("LibraryNotLoaded")
        {
        }
    }

    public class LibraryException : AppException
    {
        public LibraryException(string message) : base(message)
        {
        }
    }
}