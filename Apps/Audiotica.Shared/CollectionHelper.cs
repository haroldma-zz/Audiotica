#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Audiotica.Controls;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using Audiotica.Data.Collection.SqlHelper;
using Audiotica.Data.Spotify.Models;
using Audiotica.View;
using GalaSoft.MvvmLight.Threading;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica
{
    public static class CollectionHelper
    {
        public static List<string> SpotifySavingTracks = new List<string>();
        public static List<string> LastfmSavingTracks = new List<string>();
        public static List<string> SpotifySavingAlbums = new List<string>();
        public static List<long> LastfmSavingAlbums = new List<long>();
        private static bool _currentlyPreparing;

        #region Saving

        public static async Task SaveTrackAsync(ChartTrack chartTrack)
        {
            CurtainPrompt.Show("SongSavingFindingMp3".FromLanguageResource(), chartTrack.Name);
            var track = await App.Locator.Spotify.GetTrack(chartTrack.track_id);
            var album = await App.Locator.Spotify.GetAlbum(track.Album.Id);

            await SaveTrackAsync(track, album, false);
        }

        public static async Task SaveTrackAsync(FullTrack track)
        {
            CurtainPrompt.Show("SongSavingFindingMp3".FromLanguageResource(), track.Name);
            var album = await App.Locator.Spotify.GetAlbum(track.Album.Id);

            await SaveTrackAsync(track, album, false);
        }

        public static async Task SaveTrackAsync(SimpleTrack track, FullAlbum album, bool showFindingMessage = true)
        {
            if (showFindingMessage)
                CurtainPrompt.Show("SongSavingFindingMp3".FromLanguageResource(), track.Name);

            var result = await _SaveTrackAsync(track, album);
            ShowResults(result, track.Name);
        }

        public static async Task SaveTrackAsync(LastTrack track, bool showFindingMessage = true)
        {
            if (showFindingMessage)
                CurtainPrompt.Show("SongSavingFindingMp3".FromLanguageResource(), track.Name);

            var result = await _SaveTrackAsync(track);
            ShowResults(result, track.Name);
        }

        public static async Task SaveAlbumAsync(FullAlbum album)
        {
            if (album.Tracks.Items.Count == 0)
            {
                CurtainPrompt.ShowError("AlbumNoTracks".FromLanguageResource());
                return;
            }

            var alreadySaving = SpotifySavingAlbums.FirstOrDefault(p => p == album.Id) != null;

            if (alreadySaving)
            {
                CurtainPrompt.ShowError("EntryAlreadySaving".FromLanguageResource(), album.Name);
                return;
            }

            SpotifySavingAlbums.Add(album.Id);

            while (!App.Locator.CollectionService.IsLibraryLoaded)
            {
            }

            var collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(p => p.ProviderId.Contains(album.Id));

            var alreadySaved = collAlbum != null;

            if (alreadySaved)
            {
                var missingTracks = collAlbum.Songs.Count < album.Tracks.Items.Count;
                if (!missingTracks)
                {
                    CurtainPrompt.ShowError("EntryAlreadySaved".FromLanguageResource(), album.Name);
                    SpotifySavingAlbums.Remove(album.Id);
                    return;
                }
            }

            CurtainPrompt.Show("EntrySaving".FromLanguageResource(), album.Name);

            var index = 0;

            if (!alreadySaved)
            {
                SavingError result;
                do
                {
                    //first save one song (to avoid duplicate album creation)
                    result = await _SaveTrackAsync(album.Tracks.Items[index], album);
                    index++;
                } while (result != SavingError.None && index < album.Tracks.Items.Count);
            }

            bool success;
            var missing = false;

            if (album.Tracks.Items.Count > 1)
            {
                App.Locator.SqlService.DbConnection.BeginTransaction();
                //save the rest at the rest time
                var songs = album.Tracks.Items.Skip(index).Select(track => _SaveTrackAsync(track, album));
                var results = await Task.WhenAll(songs);

                //now wait a split second before showing success message
                await Task.Delay(1000);

                var successCount = results.Count(p => p == SavingError.None || p == SavingError.AlreadyExists
                                                      || p == SavingError.AlreadySaving);
                var missingCount = successCount == 0 ? -1 : album.Tracks.Items.Count - (successCount + index);
                success = missingCount == 0;
                missing = missingCount > 0;

                if (missing)
                    CurtainPrompt.ShowError("AlbumMissingTracks".FromLanguageResource(), missingCount, album.Name);
                App.Locator.SqlService.DbConnection.Commit();
            }
            else
                success = App.Locator.CollectionService.Albums.FirstOrDefault(p => p.ProviderId.Contains(album.Id)) !=
                          null;

            if (success)
                CurtainPrompt.Show("EntrySaved".FromLanguageResource(), album.Name);
            else if (!missing)
                CurtainPrompt.ShowError("EntrySavingError".FromLanguageResource(), album.Name);


            SpotifySavingAlbums.Remove(album.Id);
        }

        public static async Task DownloadArtistsArtworkAsync(bool missingOnly = true)
        {
            var artists = App.Locator.CollectionService.Artists.ToList();

            if (missingOnly)
                artists = artists.Where(p => !p.HasArtwork).ToList();

            var tasks = artists.Select(artist => Task.Factory.StartNew(async () =>
            {
                if (artist.ProviderId == "autc.unknown" || string.IsNullOrEmpty(artist.Name))
                    return;

                //don't want to retry getting this pic while we're downloading it
                var hadArtwork = artist.HasArtwork;
                artist.HasArtwork = true;


                try
                {
                    var lastArtist = await App.Locator.ScrobblerService.GetDetailArtist(artist.Name);

                    if (lastArtist.MainImage == null || lastArtist.MainImage.Largest == null) return;

                    var artistFilePath = string.Format(CollectionConstant.ArtistsArtworkPath, artist.Id);
                    var file =
                        await
                            StorageHelper.CreateFileAsync(artistFilePath,
                                option: CreationCollisionOption.ReplaceExisting);

                    using (var client = new HttpClient())
                    {
                        using (var stream = await client.GetStreamAsync(lastArtist.MainImage.Largest))
                        {
                            using (var fileStream = await file.OpenStreamForWriteAsync())
                            {
                                await stream.CopyToAsync(fileStream);
                            }
                        }
                    }

                    if (!hadArtwork)
                    {
                        artist.HasArtwork = true;
                        await App.Locator.SqlService.UpdateItemAsync(artist);
                    }

                    await DispatcherHelper.RunAsync(() =>
                        artist.Artwork =
                            new BitmapImage(new Uri(CollectionConstant.LocalStorageAppPath + artistFilePath))
                            {
                                DecodePixelHeight = App.Locator.CollectionService.ScaledImageSize
                            });
                }
                catch
                {
                    artist.HasArtwork = false;
                }
            })).Cast<Task>().ToList();

            await Task.WhenAll(tasks);
        }

        #endregion

        #region Playing

        //haven't tested with more than this
        private const int MaxMassPlayQueueCount = 100;
        public const double MaxPlayQueueCount = 2000;

        public static async Task PlaySongsAsync(List<Song> songs, bool random = false, bool forceClear = false)
        {
            if (songs.Count == 0) return;

            var index = random ? (songs.Count == 1 ? 0 : new Random().Next(0, songs.Count - 1)) : 0;
            var song = songs[index];

            await PlaySongsAsync(song, songs, forceClear);
        }

        public static async Task PlaySongsAsync(Song song, List<Song> songs, bool forceClear = false)
        {
            if (song == null || songs == null || songs.Count == 0) return;

            var skip = songs.IndexOf(song);
            var ordered = songs.Skip(skip).ToList();
            ordered.AddRange(songs.Take(skip));

            var overflow = songs.Count - MaxMassPlayQueueCount;
            if (overflow > 0)
                for (var i = 0; i < overflow; i++)
                    ordered.Remove(ordered.LastOrDefault());

            var playbackQueue = App.Locator.CollectionService.PlaybackQueue.ToList();

            var sameLength = _currentlyPreparing || songs.Count < playbackQueue.Count ||
                             playbackQueue.Count >= MaxMassPlayQueueCount;
            var containsSong = playbackQueue.FirstOrDefault(p => p.SongId == song.Id) != null;
            var createQueue = forceClear || (!sameLength
                              || !containsSong);

            if (_currentlyPreparing && createQueue)
            {
                //cancel the previous
                _currentlyPreparing = false;

                //split second for it to stop
                await Task.Delay(500);
            }

            if (!createQueue)
            {
                App.Locator.AudioPlayerHelper.PlaySong(playbackQueue.First(p => p.SongId == song.Id));
            }

            else
            {
                _currentlyPreparing = true;

                await App.Locator.CollectionService.ClearQueueAsync().ConfigureAwait(false);
                var queueSong = await App.Locator.CollectionService.AddToQueueAsync(song).ConfigureAwait(false);
                App.Locator.AudioPlayerHelper.PlaySong(queueSong);

                await Task.Delay(500).ConfigureAwait(false);

                if (!_currentlyPreparing)
                    return;

                App.Locator.SqlService.DbConnection.BeginTransaction();
                for (var index = 1; index < ordered.Count; index++)
                {
                    if (!_currentlyPreparing)
                        break;
                    var s = ordered[index];
                    await App.Locator.CollectionService.AddToQueueAsync(s).ConfigureAwait(false);
                }
                App.Locator.SqlService.DbConnection.Commit();

                _currentlyPreparing = false;
            }
        }

        public static void AddToPlaylistDialog(List<Song> songs)
        {
            UiBlockerUtility.BlockNavigation();
            var picker = new PlaylistPicker(songs)
            {
                Action = async playlist =>
                {
                    App.SupressBackEvent -= AppOnSupressBackEvent;
                    UiBlockerUtility.Unblock();
                    ModalSheetUtility.Hide();
                    for (var i = 0; i < songs.Count; i++)
                    {
                        var song = songs[i];

                        //only add if is not there already
                        if (playlist.Songs.FirstOrDefault(p => p.SongId == song.Id) == null)
                            await App.Locator.CollectionService.AddToPlaylistAsync(playlist, song).ConfigureAwait(false);

                        if (App.Locator.Player.CurrentQueue != null || !App.Locator.Settings.AddToInsert) continue;

                        songs.RemoveAt(0);
                        songs.Reverse();
                        songs.Insert(0, song);
                    }
                }
            };

            App.SupressBackEvent += AppOnSupressBackEvent;
            ModalSheetUtility.Show(picker);
        }

        private static void AppOnSupressBackEvent(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            App.SupressBackEvent -= AppOnSupressBackEvent;
            UiBlockerUtility.Unblock();
            ModalSheetUtility.Hide();
        }

        public static async Task AddToQueueAsync(List<Song> songs, bool ignoreInsertMode = false)
        {
            for (var i = 0; i < songs.Count; i++)
            {
                var song = songs[i];
                var playIfNotActive = i == (App.Locator.Settings.AddToInsert
                                            && App.Locator.Player.CurrentQueue != null
                    ? songs.Count - 1
                    : 0);

                //the last song insert it into the shuffle list (the others shuffle them around)
                await AddToQueueAsync(song, i == songs.Count - 1,
                    playIfNotActive, i == 0, ignoreInsertMode).ConfigureAwait(false);
            }
        }

        public static async Task AddToQueueAsync(Song song, bool shuffleInsert = true, bool playIfNotActive = true,
            bool clearIfNotActive = true, bool ignoreInsertMode = false)
        {
            if (_currentlyPreparing)
            {
                CurtainPrompt.ShowError("GenericTryAgain".FromLanguageResource());
                return;
            }

            if (!App.Locator.Player.IsPlayerActive && clearIfNotActive)
                await App.Locator.CollectionService.ClearQueueAsync();

            var overflow = App.Locator.CollectionService.CurrentPlaybackQueue.Count - MaxPlayQueueCount;
            if (overflow > 0)
                for (var i = 0; i < overflow; i++)
                {
                    var queueToRemove = App.Locator.CollectionService.CurrentPlaybackQueue.LastOrDefault();
                    if (queueToRemove == App.Locator.Player.CurrentQueue)
                        queueToRemove =
                            App.Locator.CollectionService.CurrentPlaybackQueue[
                                App.Locator.CollectionService.CurrentPlaybackQueue.Count - 2];

                    await App.Locator.CollectionService.DeleteFromQueueAsync(queueToRemove).ConfigureAwait(false);
                }

            var insert = AppSettingsHelper.Read("AddToInsert", true, SettingsStrategy.Roaming) && !ignoreInsertMode;

            var queueSong = await App.Locator.CollectionService.AddToQueueAsync(song,
                insert ? App.Locator.Player.CurrentQueue : null, shuffleInsert).ConfigureAwait(false);

            if (!App.Locator.Player.IsPlayerActive && playIfNotActive)
            {
                App.Locator.AudioPlayerHelper.PlaySong(queueSong);
                DispatcherHelper.RunAsync(() => App.Locator.Player.CurrentQueue = queueSong);
            }
        }

        #endregion

        #region Heper methods

        private static void ExitIfArtistEmpty(Artist artist)
        {
            if (App.Navigator.CurrentPage is CollectionArtistPage && artist.Songs.Count == 0)
            {
                App.Navigator.GoBack();
            }
        }

        private static void ExitIfAlbumEmpty(Album album)
        {
            if (App.Navigator.CurrentPage is CollectionAlbumPage && album.Songs.Count == 0)
            {
                App.Navigator.GoBack();
            }
            ExitIfArtistEmpty(album.PrimaryArtist);
        }

        private static void ShowResults(SavingError result, string trackName)
        {
            switch (result)
            {
                case SavingError.AlreadySaving:
                    CurtainPrompt.ShowError("EntryAlreadySaving".FromLanguageResource(), trackName);
                    break;
                case SavingError.AlreadyExists:
                    CurtainPrompt.ShowError("EntryAlreadySaved".FromLanguageResource(), trackName);
                    break;
                case SavingError.None:
                    CurtainPrompt.Show("EntrySaved".FromLanguageResource(), trackName);
                    break;
            }
        }

        private static async Task<SavingError> _SaveTrackAsync(SimpleTrack track, FullAlbum album)
        {
            var alreadySaving = SpotifySavingTracks.FirstOrDefault(p => p == track.Id) != null;

            if (alreadySaving)
            {
                return SavingError.AlreadySaving;
            }

            SpotifySavingTracks.Add(track.Id);

            while (!App.Locator.CollectionService.IsLibraryLoaded)
            {
            }

            var startTransaction = !App.Locator.SqlService.DbConnection.IsInTransaction;

            if (startTransaction)
                App.Locator.SqlService.DbConnection.BeginTransaction();

            var result = await SpotifyHelper.SaveTrackAsync(track, album);

            if (startTransaction)
                App.Locator.SqlService.DbConnection.Commit();

            ShowErrorResults(result, track.Name);

            SpotifySavingTracks.Remove(track.Id);

            return result;
        }

        private static async Task<SavingError> _SaveTrackAsync(LastTrack track)
        {
            var alreadySaving = LastfmSavingTracks.FirstOrDefault(p => p == track.Id) != null;

            if (alreadySaving)
            {
                return SavingError.AlreadySaving;
            }

            LastfmSavingTracks.Add(track.Id);

            while (!App.Locator.CollectionService.IsLibraryLoaded)
            {
            }

            var startTransaction = !App.Locator.SqlService.DbConnection.IsInTransaction;

            if (startTransaction)
                App.Locator.SqlService.DbConnection.BeginTransaction();

            var result = await ScrobblerHelper.SaveTrackAsync(track);

            if (startTransaction)
                App.Locator.SqlService.DbConnection.Commit();

            ShowErrorResults(result, track.Name);

            LastfmSavingTracks.Remove(track.Id);

            return result;
        }

        private static void ShowErrorResults(SavingError result, string trackName)
        {
            switch (result)
            {
                case SavingError.Network:
                    CurtainPrompt.Show("SongSavingNetworkError".FromLanguageResource(), trackName);
                    break;
                case SavingError.NoMatch:
                    CurtainPrompt.ShowError("SongSavingNoMatch".FromLanguageResource(), trackName);
                    break;
                case SavingError.Unknown:
                    CurtainPrompt.ShowError("EntrySavingError".FromLanguageResource(), trackName);
                    break;
            }
        }

        #endregion

        public static async Task DeleteEntryAsync(BaseEntry item, bool showSuccessMessage = true)
        {
            var name = "unknown";

            try
            {
                if (item is Song)
                {
                    var song = item as Song;
                    name = song.Name;

                    var playbackQueue = App.Locator.CollectionService.PlaybackQueue;
                    var queue = playbackQueue.FirstOrDefault(p => p.SongId == song.Id);

                    if (queue != null)
                    {
                        if (playbackQueue.Count == 1)
                            await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                        else
                            App.Locator.AudioPlayerHelper.NextSong();
                    }

                    await App.Locator.CollectionService.DeleteSongAsync(song);
                    ExitIfAlbumEmpty(song.Album);
                }

                else if (item is Playlist)
                {
                    var playlist = item as Playlist;
                    name = playlist.Name;

                    await App.Locator.CollectionService.DeletePlaylistAsync(playlist);
                }

                else if (item is Artist)
                {
                    var artist = item as Artist;
                    name = artist.Name;

                    App.Locator.CollectionService.Artists.Remove(artist);

                    var artistSongs = artist.Songs.ToList();
                    var taskList = new List<Task>();

                    foreach (var song in artistSongs)
                    {
                        var playbackQueue = App.Locator.CollectionService.PlaybackQueue;
                        var queue = playbackQueue.FirstOrDefault(p => p.SongId == song.Id);

                        if (queue != null)
                        {
                            if (playbackQueue.Count == 1)
                                await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                            else
                                App.Locator.AudioPlayerHelper.NextSong();
                        }

                        taskList.Add(App.Locator.CollectionService.DeleteSongAsync(song));
                    }

                    ExitIfArtistEmpty(artist);
                }

                else if (item is Album)
                {
                    var album = item as Album;
                    name = album.Name;

                    App.Locator.CollectionService.Albums.Remove(album);

                    var albumSongs = album.Songs.ToList();
                    var taskList = new List<Task>();

                    foreach (var song in albumSongs)
                    {
                        var playbackQueue = App.Locator.CollectionService.PlaybackQueue;
                        var queue = playbackQueue.FirstOrDefault(p => p.SongId == song.Id);

                        if (queue != null)
                        {
                            if (playbackQueue.Count == 1)
                                await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                            else
                                App.Locator.AudioPlayerHelper.NextSong();
                        }

                        taskList.Add(App.Locator.CollectionService.DeleteSongAsync(song));
                    }

                    ExitIfAlbumEmpty(album);
                }

                if (showSuccessMessage)
                    CurtainPrompt.Show("EntryDeletingSuccess".FromLanguageResource(), name);
            }
            catch
            {
                CurtainPrompt.ShowError("EntryDeletingError".FromLanguageResource(), name);
            }
        }

        public static async Task MigrateAsync()
        {
            var songs = App.Locator.CollectionService.Songs.Where(p => p.SongState == SongState.Downloaded).ToList();
            var importedSongs = App.Locator.CollectionService.Songs.Where(p =>
                p.SongState == SongState.Local && !p.AudioUrl.Substring(1).StartsWith(":")).ToList();
            var songsFolder = await StorageHelper.GetFolderAsync("songs");

            if (songs.Count != 0 && songsFolder != null)
            {
                App.Locator.SqlService.DbConnection.BeginTransaction();
                UiBlockerUtility.Block("Migrating downloaded songs to SD card...");
                foreach (var song in songs)
                {
                    try
                    {
                        var path = "Audiotica/" + song.Album.PrimaryArtist.Name + "/" + song.Album.Name;
                        var filename = string.Format("{0}.mp3", song.Name);
                        if (song.ArtistName != song.Album.PrimaryArtist.Name)
                            filename = song.ArtistName + "-" + filename;

                        var file = await StorageHelper.GetFileAsync(string.Format("songs/{0}.mp3", song.Id));

                        var folder = await StorageHelper.EnsureFolderExistsAsync(path, KnownFolders.MusicLibrary);
                        await file.CopyAsync(folder, filename, NameCollisionOption.ReplaceExisting);

                        song.AudioUrl = Path.Combine(folder.Path, filename);
                        await App.Locator.SqlService.UpdateItemAsync(song);
                    }
                    catch
                    {
                    }
                }

                App.Locator.SqlService.DbConnection.Commit();
            }

            if (importedSongs.Count > 0)
            {
                UiBlockerUtility.Block("Deleting outdated song imports...");
                App.Locator.SqlService.DbConnection.BeginTransaction();
                foreach (var importedSong in importedSongs)
                {
                    await App.Locator.CollectionService.DeleteSongAsync(importedSong);
                }
                App.Locator.SqlService.DbConnection.Commit();
            }

            UiBlockerUtility.Unblock();

            if (songsFolder != null)
                await songsFolder.DeleteAsync();

            if (importedSongs.Count > 0)
            {
                CurtainPrompt.Show("You'll need to re-scan, since some imported songs were outdated.");
            }
        }

        public static async Task<bool> PinToggleAsync(Artist artist)
        {
            bool created;
            var id = "artist." + artist.Id;

            if (!SecondaryTile.Exists(id))
            {
                created = await CreatePin(id, artist.Name, "artists/" + artist.Id,
                    string.Format(CollectionConstant.ArtistsArtworkPath, artist.Id));
            }
            else
            {
                var secondaryTile = new SecondaryTile(id);
                created = !await secondaryTile.RequestDeleteAsync();
            }

            return created;
        }

        public static async Task<bool> PinToggleAsync(Album album)
        {
            bool created;
            var id = "album." + album.Id;

            if (!SecondaryTile.Exists(id))
            {
                created = await CreatePin(id, album.Name, "albums/" + album.Id,
                    string.Format(CollectionConstant.ArtworkPath, album.Id));
            }
            else
            {
                var secondaryTile = new SecondaryTile(id);
                created = !await secondaryTile.RequestDeleteAsync();
            }

            return created;
        }

        private static async Task<bool> CreatePin(string id, string displayName, string arguments, string artwork)
        {
            var tileActivationArguments = arguments;
            var image =
                new Uri(CollectionConstant.LocalStorageAppPath + artwork);

            var secondaryTile = new SecondaryTile(id,
                displayName,
                tileActivationArguments,
                image,
                TileSize.Square150x150)
            {
                ShortName = displayName
            };
            secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;

            return await secondaryTile.RequestCreateAsync();
        }
    }
}