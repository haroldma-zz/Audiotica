#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Audiotica.Data.Collection.Model;
using SQLitePCL;

#endregion

namespace Audiotica.Data.Collection.RunTime
{
    public class SqlService : ISqlService
    {
        private readonly SQLiteConnection db;

        public SqlService()
        {
            db = new SQLiteConnection("autc_collection.db");
        }

        public void Initialize()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS
                                Artist (Id          INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                         Name    VARCHAR( 140 ),                                               
                                         XboxId    VARCHAR( 50 ),
                                         LastFmId VARCHAR( 50 ))
                            ;";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"CREATE TABLE IF NOT EXISTS
                                Album (Id          INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                         Name    VARCHAR( 140 ),                                               
                                         XboxId    VARCHAR( 50 ),
                                         LastFmId VARCHAR( 50 ),
                                         Genre VARCHAR( 50 ),
                                         ReleaseDate DATETIME,
                                         PrimaryArtistId INTEGER,
                                         FOREIGN KEY(PrimaryArtistId) REFERENCES Artist(Id) ON DELETE CASCADE
                            );";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"CREATE TABLE IF NOT EXISTS
                                Song (Id      INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                            Name    VARCHAR( 140 ),
                                            AudioUrl    VARCHAR( 280 ),
                                            ArtistId    INTEGER,                                            
                                            AlbumId    INTEGER,                                              
                                            XboxId    VARCHAR( 50 ),
                                            LastFmId VARCHAR( 50 ),
                                            TrackNumber    INTEGER,                                            
                                            PlayCount    INTEGER,                                            
                                            SongState    INTEGER,                                                                                    
                                            HeartState    INTEGER,  
                                            CreatedAt     DATETIME DEFAULT CURRENT_TIMESTAMP,
                                            FOREIGN KEY(ArtistId) REFERENCES Artist(Id) ON DELETE CASCADE,
                                            FOREIGN KEY(AlbumId) REFERENCES Album(Id) ON DELETE CASCADE
                            );"; //if a coresponding artisrt or album is delete, all of the track should be too
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"CREATE TABLE IF NOT EXISTS
                                QueueSong (Id          INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                         SongId    INTEGER,                                               
                                         PrevId    INTEGER,                                               
                                         NextId    INTEGER,    
                                         FOREIGN KEY(SongId) REFERENCES Song(Id) ON DELETE CASCADE
                            );";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"CREATE TABLE IF NOT EXISTS
                                Playlist (Id          INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                         Name    VARCHAR( 50 )                      
                            );";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"CREATE TABLE IF NOT EXISTS
                                PlaylistSong (Id          INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                         SongId    INTEGER,                                               
                                         PrevId    INTEGER,                                               
                                         NextId    INTEGER,    
                                         PlaylistId    INTEGER,    
                                         FOREIGN KEY(SongId) REFERENCES Song(Id) ON DELETE CASCADE
                                         FOREIGN KEY(PlaylistId) REFERENCES Playlist(Id) ON DELETE CASCADE
                            );";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            // Turn on Foreign Key constraints
            sql = @"PRAGMA foreign_keys = ON";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }
        }

        public Task InitializeAsync()
        {
            return Task.Factory.StartNew(Initialize);
        }

        public void ResetData()
        {
            var sql = @"DELETE FROM Song";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"DELETE FROM Album";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"DELETE FROM Artist";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"DELETE FROM QueueSong";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }

            sql = @"DELETE FROM PlaylistSong";
            using (var statement = db.Prepare(sql))
            {
                statement.Step();
            }
        }

        public async Task InsertSongAsync(Song song)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var custstmt =
                        db.Prepare(
                            "INSERT INTO Song (Name, AudioUrl, ArtistId, AlbumId, XboxId, LastFmId, TrackNumber, PlayCount, SongState, HeartState) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)")
                        )
                    {
                        custstmt.Bind(1, song.Name);
                        custstmt.Bind(2, song.AudioUrl);
                        custstmt.Bind(3, song.ArtistId);
                        custstmt.Bind(4, song.AlbumId);
                        custstmt.Bind(5, song.XboxId);
                        custstmt.Bind(6, song.LastFmId);
                        custstmt.Bind(7, song.TrackNumber);
                        custstmt.Bind(8, song.PlayCount);
                        custstmt.Bind(9, (int) song.SongState);
                        custstmt.Bind(10, (int) song.HeartState);
                        custstmt.Step();

                        Debug.WriteLine("INSERT completed OK for song " + song.Name);
                    }
                }
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }

            using (var idstmt = db.Prepare("SELECT last_insert_rowid()"))
            {
                idstmt.Step();
                {
                    Debug.WriteLine("INSERT ID for song " + song.Name + ": " + (long) idstmt[0]);
                    song.Id = (long) idstmt[0];
                }
            }
        }

        public async Task InsertArtistAsync(Artist artist)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var projstmt = db.Prepare("INSERT INTO Artist (Name, XboxId, LastFmId) VALUES (?, ?, ?)"))
                    {
                        // Reset the prepared statement so we can reuse it.
                        projstmt.ClearBindings();
                        projstmt.Reset();

                        projstmt.Bind(1, artist.Name);
                        projstmt.Bind(2, artist.XboxId);
                        projstmt.Bind(3, artist.LastFmId);

                        projstmt.Step();
                        Debug.WriteLine("INSERT queue completed OK for " + artist.Name);
                    }
                }
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }

            using (var idstmt = db.Prepare("SELECT last_insert_rowid()"))
            {
                idstmt.Step();
                {
                    Debug.WriteLine("INSERT ID for artist " + artist.Name + ": " + (long) idstmt[0]);
                    artist.Id = (long) idstmt[0];
                }
            }
        }

        public async Task InsertAlbumAsync(Album album)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (
                        var projstmt =
                            db.Prepare(
                                "INSERT INTO Album (Name, XboxId, LastFmId, Genre, ReleaseDate, PrimaryArtistId) VALUES (?, ?, ?, ?, ?, ?)")
                        )
                    {
                        // Reset the prepared statement so we can reuse it.
                        projstmt.ClearBindings();
                        projstmt.Reset();

                        projstmt.Bind(1, album.Name);
                        projstmt.Bind(2, album.XboxId);
                        projstmt.Bind(3, album.LastFmId);
                        projstmt.Bind(4, album.Genre);
                        projstmt.Bind(5, album.ReleaseDate.ToString());
                        projstmt.Bind(6, album.PrimaryArtistId);

                        projstmt.Step();
                        Debug.WriteLine("INSERT Album completed OK for " + album.Name);
                    }
                }
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }

            using (var idstmt = db.Prepare("SELECT last_insert_rowid()"))
            {
                idstmt.Step();
                {
                    Debug.WriteLine("INSERT ID for album " + album.Name + ": " + (long) idstmt[0]);
                    album.Id = (long) idstmt[0];
                }
            }
        }

        public async Task InsertQueueSongAsync(QueueSong queue)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (
                        var projstmt =
                            db.Prepare("INSERT INTO QueueSong (SongId, NextId, PrevId) VALUES (?, ?, ?)"))
                    {
                        // Reset the prepared statement so we can reuse it.
                        projstmt.ClearBindings();
                        projstmt.Reset();

                        projstmt.Bind(1, queue.SongId);
                        projstmt.Bind(2, queue.NextId);
                        projstmt.Bind(3, queue.PrevId);

                        projstmt.Step();
                        Debug.WriteLine("INSERT queuesong completed OK for " + queue.SongId);
                    }
                }
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }

            using (var idstmt = db.Prepare("SELECT last_insert_rowid()"))
            {
                idstmt.Step();
                {
                    Debug.WriteLine("INSERT ID for queuesong " + queue.SongId + ": " + (long) idstmt[0]);
                    queue.Id = (long) idstmt[0];
                }
            }
        }

        public Task InsertPlaylistSongAsync(PlaylistSong song)
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemAsync(long id, string table)
        {
            return Task.Run(() =>
            {
                using (
                    var projstmt =
                        db.Prepare("DELETE FROM " + table + " WHERE Id = ?"))
                {
                    // Reset the prepared statement so we can reuse it.
                    projstmt.ClearBindings();
                    projstmt.Reset();

                    projstmt.Bind(1, id);

                    projstmt.Step();
                    Debug.WriteLine("DELETE completed OK for " + id);
                }
            }
                );
        }

        public Task UpdateQueueSongAsync(QueueSong queue)
        {
            return Task.Run(() =>
            {
                using (
                    var projstmt =
                        db.Prepare("UPDATE QueueSong SET SongId = ?, NextId = ?, PrevId = ? WHERE Id = ?"))
                {
                    // Reset the prepared statement so we can reuse it.
                    projstmt.ClearBindings();
                    projstmt.Reset();

                    projstmt.Bind(1, queue.SongId);
                    projstmt.Bind(2, queue.NextId);
                    projstmt.Bind(3, queue.PrevId);
                    projstmt.Bind(4, queue.Id);

                    projstmt.Step();
                    Debug.WriteLine("UPDATE queuesong completed OK for " + queue.SongId);
                }
            });
        }

        public Task<List<Song>> GetSongsAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var songs = new List<Song>();

                using (var statement = db.Prepare("SELECT * FROM Song"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var song = new Song
                        {
                            Id = (long) statement["Id"],
                            Name = (string) statement["Name"],
                            ArtistId = (long) statement["ArtistId"],
                            AlbumId = (long) statement["AlbumId"],
                            LastFmId = (string) statement["LastFmId"],
                            XboxId = (string) statement["XboxId"],
                            HeartState = (HeartState) (long) statement["HeartState"],
                            SongState = (SongState) (long) statement["SongState"],
                            AudioUrl = (string) statement["AudioUrl"],
                            TrackNumber = (long) statement["TrackNumber"],
                            PlayCount = (long) statement["PlayCount"]
                        };
                        Debug.WriteLine("Selected Song name:" + song.Name);
                        songs.Add(song);
                    }
                }

                return songs;
            });
        }

        public Task<List<Artist>> GetArtistsAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var artists = new List<Artist>();

                using (var statement = db.Prepare("SELECT * FROM Artist"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var artist = new Artist
                        {
                            Id = (long) statement["Id"],
                            Name = (string) statement["Name"],
                            LastFmId = (string) statement["LastFmId"],
                            XboxId = (string) statement["XboxId"]
                        };
                        Debug.WriteLine("Selected artist name:" + artist.Name);
                        artists.Add(artist);
                    }
                }

                return artists;
            });
        }

        public Task<List<Album>> GetAlbumsAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var albums = new List<Album>();

                using (var statement = db.Prepare("SELECT * FROM Album"))
                {
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        var album = new Album()
                        {
                            Id = (long) statement["Id"],
                            Name = (string) statement["Name"],
                            LastFmId = (string) statement["LastFmId"],
                            XboxId = (string) statement["XboxId"],
                            PrimaryArtistId = (long) statement["PrimaryArtistId"],
                            Genre = (string) statement["Genre"],
                            ReleaseDate = DateTime.Parse((string) statement["ReleaseDate"])
                        };
                        Debug.WriteLine("Selected album name:" + album.Name);
                        albums.Add(album);
                    }
                }

                return albums;
            });
        }

        public List<QueueSong> GetQueueSongs()
        {
            var queueSongs = new List<QueueSong>();

            using (var statement = db.Prepare("SELECT * FROM QueueSong"))
            {
                while (statement.Step() == SQLiteResult.ROW)
                {
                    var queueSong = new QueueSong()
                    {
                        Id = (long) statement["Id"],
                        SongId = (long) statement["SongId"],
                        NextId = (long) statement["NextId"],
                        PrevId = (long) statement["PrevId"],
                    };
                    Debug.WriteLine("Selected queuesong id:" + queueSong.Id);
                    queueSongs.Add(queueSong);
                }
            }

            return queueSongs;
        }

        public Task<List<QueueSong>> GetQueueSongsAsync()
        {
            return Task.FromResult(GetQueueSongs());
        }

        public Task DeleteTableAsync(string table)
        {
            return Task.Run(() =>
            {
                using (
                    var projstmt =
                        db.Prepare("DELETE FROM " + table))
                {
                    projstmt.Step();
                    Debug.WriteLine("DELETE " + table + " completed OK");
                }

                using (//reset id seed
                   var projstmt =
                       db.Prepare("DELETE FROM sqlite_sequence  WHERE name = '" + table + "'"))
                {
                    projstmt.Step();
                }
            });
        }
    }
}
