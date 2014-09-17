using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Audiotica.Collection.Model;
using Audiotica.Core.Utilities;
using SQLite;

namespace Audiotica.Collection.RunTime
{
    public class CollectionService : ICollectionService
    {
        private readonly ISqlService _service;

        public CollectionService(ISqlService service)
        {
            _service = service;
            Songs = new ObservableCollection<Song>();
            Artists = new ObservableCollection<Artist>();
            Albums = new ObservableCollection<Album>();
        }


        public ObservableCollection<Song> Songs { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }

        public void LoadLibrary()
        {
            var songs = new ObservableCollection<Song>(_service.Connection.Table<Song>());
            var albums = new ObservableCollection<Album>(_service.Connection.Table<Album>());
            var artists = new ObservableCollection<Artist>(_service.Connection.Table<Artist>());

            foreach (var song in songs)
            {
                song.Artist = artists.FirstOrDefault(p => p.Id == song.ArtistId);
                song.Album = albums.FirstOrDefault(p => p.Id == song.AlbumId);
            }

            foreach (var album in albums)
            {
                album.Songs = songs.Where(p => p.AlbumId == album.Id).OrderBy(p => p.TrackNumber).ToList();
                album.PrimaryArtist = artists.FirstOrDefault(p => p.Id == album.PrimaryArtistId);
            }

            foreach (var artist in artists)
            {
                artist.Songs = songs.Where(p => p.ArtistId == artist.Id).ToList();
                artist.Albums = albums.Where(p => p.PrimaryArtistId == artist.Id).ToList();
            }

            Songs = songs;
            Artists = artists;
            Albums = albums;
        }

        public Task LoadLibraryAsync()
        {
            //just return non async as a task
            return Task.Factory.StartNew(LoadLibrary);
        }

        public async Task AddSongAsync(Song song, string artworkUrl)
        {
            //Validating song
            if (song.Album == null)
                throw new Exception("Song must have album, use CreateSingleAlbumEntry unknown from ");
            if (song.Artist == null)
                throw new Exception("Song must have artist, use CreateUnknowArtistEntry for unknown");

            #region create artist

            var artist = Artists.FirstOrDefault(entry => entry.XboxId == song.Artist.XboxId);

            if (artist == null)
            {
                await _service.InsertAsync(song.Artist);
                song.Album.PrimaryArtistId = song.Artist.Id;
                Artists.Add(song.Artist);
            }

            else
                song.Artist = artist;

            #endregion

            #region create album

            var album = Albums.FirstOrDefault(p => p.XboxId == song.Album.XboxId);

            if (album != null)
                song.Album = album;
            else
            {
                await _service.InsertAsync(song.Album);
                Albums.Add(song.Album);
                song.Artist.Albums.Add(song.Album);
            }

            #endregion

            #region Download artwork

            //Use the album if one is available
            var filePath = string.Format(CollectionConstant.ArtworkPath, song.Album.Id);

            //Check if the album artwork has already been downloaded
            var artworkExists = await StorageHelper.FileExistsAsync(filePath);

            if (!artworkExists)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var stream = await client.GetStreamAsync(artworkUrl);
                        using (
                            var fileStream =
                                await
                                    (await StorageHelper.CreateFileAsync(filePath)).OpenStreamForWriteAsync()
                            )
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Some shit happened saving the artwork, here: " + e);
                }
            }

            #endregion

            //Update ids
            song.AlbumId = song.Album.Id;
            song.ArtistId = song.Artist.Id;

            //Insert to db
            await _service.InsertAsync(song);

            if (artist == null)
                song.Artist.Songs.Add(song);
            if (album == null)
                song.Album.Songs.Add(song);

            Songs.Add(song);
        }

        public Task DeleteSongAsync(Song song)
        {
            throw new NotImplementedException();
        }
    }
}
