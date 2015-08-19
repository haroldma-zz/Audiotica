using System;
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
        private readonly IStorageUtility _storageUtility;

        public LibraryService(SQLiteConnection sqLiteConnection, IStorageUtility storageUtility,
            IDispatcherUtility dispatcherUtility)
        {
            _sqLiteConnection = sqLiteConnection;
            _storageUtility = storageUtility;
            _dispatcherUtility = dispatcherUtility;
        }

        public void Dispose()
        {
            _sqLiteConnection.Dispose();
        }

        public bool IsLoaded { get; private set; }

        public OptimizedObservableCollection<Track> Tracks { get; } =
            new OptimizedObservableCollection<Track>();

        public OptimizedObservableCollection<Album> Albums { get; } =
            new OptimizedObservableCollection<Album>();

        public OptimizedObservableCollection<Artist> Artists { get; } =
            new OptimizedObservableCollection<Artist>();

        public OptimizedObservableCollection<Playlist> Playlists { get; } =
            new OptimizedObservableCollection<Playlist>();

        public Track Find(long id)
        {
            if (!IsLoaded) throw new NotLoadedException();
            return Tracks.FirstOrDefault(p => p.Id == id);
        }

        public Track Find(Track track)
        {
            if (!IsLoaded) throw new NotLoadedException();
            return Tracks.FirstOrDefault(p => TrackComparer.AreEqual(p, track));
        }

        public void Load()
        {
            if (IsLoaded) return;

            CreateTables();
            LoadTracks();
            LoadPlaylists();

            IsLoaded = true;
        }

        public void AddTrack(Track track)
        {
            var existing = Find(track) != null;
            if (existing) throw new LibraryException("Track already saved.");

            track.Id = 0;
            _sqLiteConnection.Insert(track);

            _dispatcherUtility.RunAsync(() =>
            {
                CreateRelatedObjects(track);
                track.IsFromLibrary = true;
                Tracks.Add(track);
            });
        }

        public void UpdateTrack(Track track)
        {
            _sqLiteConnection.Update(track);
        }

        public Task LoadAsync()
        {
            return Task.Factory.StartNew(Load);
        }

        public Task AddTrackAsync(Track track)
        {
            return Task.Factory.StartNew(() => AddTrack(track));
        }

        public Task UpdateTrackAsync(Track track)
        {
            return Task.Factory.StartNew(() => UpdateTrack(track));
        }

        private void CreateTables()
        {
            // The only object used so far.
            _sqLiteConnection.CreateTable<Track>();
        }

        private void LoadPlaylists()
        {
        }

        private void LoadTracks()
        {
            var tracks = _sqLiteConnection.Table<Track>().ToList();

            foreach (var track in tracks)
            {
                if (track.DisplayArtist == "Mac Miller")
                {
                    
                }
                track.IsFromLibrary = true;
                CreateRelatedObjects(track);
                Tracks.Add(track);
            }
        }

        private void CreateRelatedObjects(Track track)
        {
            var displaySameAsAlbumArtist = track.DisplayArtist.EqualsIgnoreCase(track.AlbumArtist);

            var albumArtist = Artists.FirstOrDefault(p =>
                p.Name.EqualsIgnoreCase(track.AlbumArtist));
            if (albumArtist == null)
            {
                albumArtist = new Artist
                {
                    Name = track.AlbumArtist,
                    ArtworkUri = track.AlbumArtist == track.DisplayArtist ? track.ArtistArtworkUri : null
                };
                Artists.Add(albumArtist);
            }
            else if (albumArtist.ArtworkUri == null && displaySameAsAlbumArtist)
                albumArtist.ArtworkUri = track.ArtistArtworkUri;

            albumArtist.Tracks.Add(track);
            CreateAlbum(track, albumArtist);

            if (!displaySameAsAlbumArtist)
                CreateDisplayArtist(track);

            CreateSecondaryArtists(track);
        }

        private void CreateDisplayArtist(Track track)
        {
            var displayArtist = Artists.FirstOrDefault(p =>
                p.Name.EqualsIgnoreCase(track.DisplayArtist));
            if (displayArtist == null)
            {
                displayArtist = new Artist
                {
                    Name = track.DisplayArtist,
                    ArtworkUri = track.ArtistArtworkUri
                };
                Artists.Add(displayArtist);
            }
            else if (displayArtist.ArtworkUri == null)
                displayArtist.ArtworkUri = track.ArtistArtworkUri;

            displayArtist.Tracks.Add(track);
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
                album.ArtworkUri = track.ArtworkUri;

            album.Tracks.Add(track);
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