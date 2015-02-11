using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Audiotica.Controls;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Common;
using Audiotica.Core.WinRt.Utilities;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.RunTime;
using Audiotica.Data.Model.AudioticaCloud;
using Audiotica.Data.Spotify.Models;
using Audiotica.View;

using GalaSoft.MvvmLight.Threading;

using IF.Lastfm.Core.Objects;

using Microsoft.WindowsAzure.MobileServices;

using PCLStorage;

using SQLite;

using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.StartScreen;

using Xamarin;

using CreationCollisionOption = PCLStorage.CreationCollisionOption;
using NameCollisionOption = Windows.Storage.NameCollisionOption;

namespace Audiotica
{
    public static class CollectionHelper
    {
        private static readonly List<string> SpotifySavingTracks = new List<string>();

        private static readonly List<string> LastfmSavingTracks = new List<string>();

        private static readonly List<string> SpotifySavingAlbums = new List<string>();

        private static bool _currentlyPreparing;

        public static async Task CloudSync(bool refreshProfile = true)
        {
            var displayingStatus = App.Locator.CollectionService.Songs.Count == 0;
            try
            {
                // refreshing user profile info
                if (App.Locator.AudioticaService.IsAuthenticated)
                {
                    if (refreshProfile)
                    {
                        await App.Locator.AudioticaService.GetProfileAsync().ConfigureAwait(false);
                        IdentifyXamarin();
                    }

                    if (App.Locator.AudioticaService.CurrentUser != null
                        && App.Locator.AudioticaService.CurrentUser.Subscription != SubscriptionType.None)
                    {
                        var mobileService = new MobileServiceClient(
                            "https://audiotica-cloud.azure-mobile.net/", 
                            "AypzKLKRIDPGkXXzCGYGqjJNliXTwp74")
                        {
                            CurrentUser =
                                new MobileServiceUser(App.Locator.AudioticaService.CurrentUser.Id)
                                {
                                    MobileServiceAuthenticationToken =
                                        App.Locator.AudioticaService.AuthenticationToken
                                }
                        };
                        var sync = new CloudSyncService(
                            mobileService, 
                            App.Locator.CollectionService, 
                            App.Locator.AppSettingsHelper, 
                            App.Locator.SqlService, 
                            App.Locator.PclDispatcherHelper);

                        EventHandler handlerStarted = (sender, args) => UiBlockerUtility.Block("Syncing new songs...");
                        sync.OnLargeSyncStarted += handlerStarted;
                        EventHandler handlerFinish = (sender, args) => UiBlockerUtility.Unblock();
                        sync.OnLargeSyncFinished += handlerFinish;

                        if (displayingStatus)
                        {
                            await
                                App.Locator.PclDispatcherHelper.RunAsync(
                                    () => StatusBarHelper.ShowStatus("Starting sync...")).ConfigureAwait(false);
                        }

                        await sync.SyncAsync().ConfigureAwait(false);

                        sync.OnLargeSyncStarted -= handlerStarted;
                        sync.OnLargeSyncFinished -= handlerFinish;
                    }
                }
            }
            catch (Exception e)
            {
                Insights.Report(e, "Where", "CloudSync");
            }

            if (displayingStatus)
            {
                await App.Locator.PclDispatcherHelper.RunAsync(StatusBarHelper.HideStatus).ConfigureAwait(false);
            }
        }

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
                        {
                            await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                        }
                        else
                        {
                            App.Locator.AudioPlayerHelper.NextSong();
                        }
                    }

                    await App.Locator.CollectionService.DeleteSongAsync(song);
                    ExitIfAlbumEmpty(song.Album);
                }
                else if (item is Playlist)
                {
                    var playlist = (Playlist)item;
                    name = playlist.Name;

                    await App.Locator.CollectionService.DeletePlaylistAsync(playlist);
                }
                else if (item is Artist)
                {
                    var artist = (Artist)item;
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
                            {
                                await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                            }
                            else
                            {
                                App.Locator.AudioPlayerHelper.NextSong();
                            }
                        }

                        taskList.Add(App.Locator.CollectionService.DeleteSongAsync(song));
                    }

                    ExitIfArtistEmpty(artist);
                }
                else if (item is Album)
                {
                    var album = (Album)item;
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
                            {
                                await App.Locator.AudioPlayerHelper.ShutdownPlayerAsync();
                            }
                            else
                            {
                                App.Locator.AudioPlayerHelper.NextSong();
                            }
                        }

                        taskList.Add(App.Locator.CollectionService.DeleteSongAsync(song));
                    }

                    ExitIfAlbumEmpty(album);
                }

                if (showSuccessMessage)
                {
                    CurtainPrompt.Show("EntryDeletingSuccess".FromLanguageResource(), name);
                }
            }
            catch
            {
                CurtainPrompt.ShowError("EntryDeletingError".FromLanguageResource(), name);
            }
        }

        public static void IdentifyXamarin()
        {
            var user = App.Locator.AudioticaService.CurrentUser;
            if (user != null)
            {
                Insights.Identify(
                    user.Id, 
                    new Dictionary<string, string>
                    {
                        { Insights.Traits.Email, user.Email }, 
                        { "Subscription", user.Subscription.ToString() }, 
                        { "SubscriptionStatus", user.SubscriptionStatus.ToString() }, 
                        { Insights.Traits.Name, user.Username }
                    });
            }
        }

        public static void MatchSong(Song song)
        {
            TaskHelper.Enqueue(MatchSongAsync(song));
        }

        public static void MatchSongs()
        {
            var songs =
                App.Locator.CollectionService.Songs.Where(p => p.SongState == SongState.Matching)
                    .OrderBy(p => p.Name)
                    .ToList();

            foreach (var song in songs)
            {
                MatchSong(song);
            }
        }

        public static async Task MigrateAsync()
        {
            var songs = App.Locator.CollectionService.Songs.Where(p => p.SongState == SongState.Downloaded).ToList();
            var importedSongs =
                App.Locator.CollectionService.Songs.Where(
                    p => p.SongState == SongState.Local && !p.AudioUrl.Substring(1).StartsWith(":")).ToList();
            var songsFolder = await WinRtStorageHelper.GetFolderAsync("songs");

            if (songs.Count != 0 && songsFolder != null)
            {
                using (var handler = Insights.TrackTime("Migrate Downloads To Phone"))
                {
                    App.Locator.SqlService.BeginTransaction();

                    UiBlockerUtility.Block("Migrating downloaded songs to phone...");
                    foreach (var song in songs)
                    {
                        try
                        {
                            var filename = song.Name.CleanForFileName("Invalid Song Name");
                            if (song.ArtistName != song.Album.PrimaryArtist.Name)
                            {
                                filename = song.ArtistName.CleanForFileName("Invalid Artist Name") + "-" + filename;
                            }

                            var path = string.Format(
                                AppConstant.SongPath, 
                                song.Album.PrimaryArtist.Name.CleanForFileName("Invalid Artist Name"), 
                                song.Album.Name.CleanForFileName("Invalid Album Name"), 
                                filename);

                            var file = await WinRtStorageHelper.GetFileAsync(string.Format("songs/{0}.mp3", song.Id));

                            var folder =
                                await WinRtStorageHelper.EnsureFolderExistsAsync(path, KnownFolders.MusicLibrary);
                            await file.MoveAsync(folder, filename, NameCollisionOption.ReplaceExisting);

                            song.AudioUrl = Path.Combine(folder.Path, filename);
                            await App.Locator.SqlService.UpdateItemAsync(song);
                        }
                        catch (Exception e)
                        {
                            Insights.Report(
                                e, 
                                new Dictionary<string, string>
                                {
                                    { "Where", "Migrating Download" }, 
                                    { "SongName", song.Name }, 
                                    { "ArtistName", song.ArtistName }, 
                                    { "AlbumName", song.Album.Name }, 
                                    { "SongProviderId", song.ProviderId }, 
                                    { "SongAudioUrl", song.AudioUrl }
                                });
                        }
                    }

                    handler.Data.Add("TotalCount", songs.Count.ToString());
                    App.Locator.SqlService.Commit();
                }
            }

            if (importedSongs.Count > 0)
            {
                using (var handler = Insights.TrackTime("Migrate Outdated Imports"))
                {
                    UiBlockerUtility.Block("Deleting outdated song imports...");
                    App.Locator.SqlService.BeginTransaction();
                    foreach (var song in importedSongs)
                    {
                        try
                        {
                            await App.Locator.CollectionService.DeleteSongAsync(song);
                        }
                        catch (Exception e)
                        {
                            Insights.Report(
                                e, 
                                new Dictionary<string, string>
                                {
                                    { "Where", "Migrating Outdated Import" }, 
                                    { "SongName", song.Name }, 
                                    { "SongProviderId", song.ProviderId }, 
                                    { "SongAudioUrl", song.AudioUrl }
                                });
                        }
                    }

                    handler.Data.Add("TotalCount", importedSongs.Count.ToString());
                    App.Locator.SqlService.Commit();
                }
            }

            UiBlockerUtility.Unblock();

            if (songsFolder != null)
            {
                await songsFolder.DeleteAsync();
            }

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
                Insights.Track(
                    "Pin To Start", 
                    new Dictionary<string, string>
                    {
                        { "DisplayName", artist.Name }, 
                        { "ProviderId", artist.ProviderId }, 
                        { "Type", "Artist" }
                    });
                created =
                    await
                    CreatePin(
                        id, 
                        artist.Name, 
                        "artists/" + artist.Id, 
                        string.Format(AppConstant.ArtistsArtworkPath, artist.Id));
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
                Insights.Track(
                    "Pin To Start", 
                    new Dictionary<string, string>
                    {
                        { "DisplayName", album.Name }, 
                        { "ProviderId", album.ProviderId }, 
                        { "Type", "Album" }
                    });
                created =
                    await
                    CreatePin(
                        id, 
                        album.Name, 
                        "albums/" + album.Id, 
                        string.Format(AppConstant.ArtworkPath, album.Id));
            }
            else
            {
                var secondaryTile = new SecondaryTile(id);
                created = !await secondaryTile.RequestDeleteAsync();
            }

            return created;
        }

        public static void RequiresCollectionToLoad(Action action, bool block = true)
        {
            if (App.Locator.CollectionService.IsLibraryLoaded)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Insights.Report(e, "Where", "RequiresCollectionToLoad-Loaded");
                }
            }
            else
            {
                if (block)
                {
                    UiBlockerUtility.Block("Waiting for collection to load...");
                }

                App.Locator.CollectionService.LibraryLoaded += (sender, args) =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        Insights.Report(e, "Where", "RequiresCollectionToLoad");
                    }

                    if (block)
                    {
                        UiBlockerUtility.Unblock();
                    }
                };
            }
        }

        private static async Task<bool> CreatePin(string id, string displayName, string arguments, string artwork)
        {
            var tileActivationArguments = arguments;

            var image = new Uri(AppConstant.LocalStorageAppPath + artwork);

            var secondaryTile = new SecondaryTile(
                id, 
                displayName, 
                tileActivationArguments, 
                image, 
                TileSize.Square150x150) {
                                           ShortName = displayName 
                                        };

            secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;

            return await secondaryTile.RequestCreateAsync();
        }

        private static async Task MatchSongAsync(Song song)
        {
            using (var handler = Insights.TrackTime("Match Song"))
            {
                var url = await App.Locator.Mp3MatchEngine.FindMp3For(song.Name, song.Artist.Name).ConfigureAwait(false);

                if (string.IsNullOrEmpty(url) && song.ArtistName != song.Artist.Name)
                {
                    // try using the song artist name when is different than the album artist name
                    url = await App.Locator.Mp3MatchEngine.FindMp3For(song.Name, song.ArtistName).ConfigureAwait(false);
                }

                handler.Data.Add("FoundMatch", string.IsNullOrEmpty(url) ? "False" : "True");
                await App.Locator.PclDispatcherHelper.RunAsync(
                    () =>
                    {
                        if (string.IsNullOrEmpty(url))
                        {
                            song.SongState = SongState.NoMatch;
                        }
                        else
                        {
                            song.AudioUrl = url;
                            song.SongState = SongState.None;
                        }
                    });
                await App.Locator.SqlService.UpdateItemAsync(song);
            }
        }

        #region Saving

        public static async Task<SavingError> SaveTrackAsync(ChartTrack chartTrack)
        {
            if (App.Locator.CollectionService.SongAlreadyExists(
                "spotify." + chartTrack.track_id, 
                chartTrack.Name, 
                chartTrack.album_name, 
                chartTrack.ArtistName))
            {
                ShowResults(SavingError.AlreadyExists, chartTrack.Name);
                return SavingError.AlreadyExists;
            }

            using (
                var handle = Insights.TrackTime(
                    "Save Song", 
                    new Dictionary<string, string>
                    {
                        { "Type", "Spotify" }, 
                        { "Subtype", "Chart" }, 
                        { "ProviderId", chartTrack.track_id }, 
                        { "Name", chartTrack.Name }, 
                        { "ArtistName", chartTrack.ArtistName }
                    }))
            {
                var track = await App.Locator.Spotify.GetTrack(chartTrack.track_id);

                if (track != null)
                {
                    var album = await App.Locator.Spotify.GetAlbum(track.Album.Id);

                    var result = await SaveTrackAsync(track, album, false);

                    handle.Data.Add("SavingError", result.ToString());
                    return result;
                }

                ShowResults(SavingError.Network, chartTrack.Name);
                handle.Data.Add("SavingError", "Network");
                return SavingError.Network;
            }
        }

        public static async Task<SavingError> SaveTrackAsync(FullTrack track, bool manualMatch = false)
        {
            if (App.Locator.CollectionService.SongAlreadyExists(
                "spotify." + track.Id, 
                track.Name, 
                track.Album.Name, 
                track.Artist.Name))
            {
                ShowResults(SavingError.AlreadyExists, track.Name);
                return SavingError.AlreadyExists;
            }

            using (
                var handle = Insights.TrackTime(
                    "Save Song", 
                    new Dictionary<string, string>
                    {
                        { "Type", "Spotify" }, 
                        { "Subtype", "Full" }, 
                        { "ProviderId", track.Id }, 
                        { "Name", track.Name }, 
                        { "ArtistName", track.Artist.Name }
                    }))
            {
                SavingError result;

                try
                {
                    var album = await App.Locator.Spotify.GetAlbum(track.Album.Id);

                    UiBlockerUtility.Unblock();
                    result = await SaveTrackAsync(track, album, false);
                }
                catch
                {
                    UiBlockerUtility.Unblock();
                    result = SavingError.Network;
                }

                handle.Data.Add("SavingError", result.ToString());
                return result;
            }
        }

        public static async Task<SavingError> SaveTrackAsync(SimpleTrack track, FullAlbum album, bool trackTime = true)
        {
            var handle = Insights.TrackTime(
                "Save Song", 
                new Dictionary<string, string>
                {
                    { "Type", "Spotify" }, 
                    { "Subtype", "Simple" }, 
                    { "ProviderId", track.Id }, 
                    { "Name", track.Name }, 
                    { "ArtistName", track.Artist != null ? track.Artist.Name : null }
                });

            if (trackTime)
            {
                handle.Start();
            }

            var result = await _SaveTrackAsync(track, album);
            ShowResults(result, track.Name);

            if (trackTime)
            {
                handle.Data.Add("SavingError", result.ToString());
                handle.Stop();
            }

            return result;
        }

        public static async Task<SavingError> SaveTrackAsync(LastTrack track)
        {
            using (
                var handle = Insights.TrackTime(
                    "Save Song", 
                    new Dictionary<string, string>
                    {
                        { "Type", "LastFM" }, 
                        { "ProviderId", track.Id }, 
                        { "Name", track.Name }, 
                        { "ArtistName", track.ArtistName }
                    }))
            {
                var result = await _SaveTrackAsync(track);
                ShowResults(result, track.Name);

                handle.Data.Add("SavingError", result.ToString());

                return result;
            }
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

            using (
                var handler = Insights.TrackTime(
                    "Save Album", 
                    new Dictionary<string, string>
                    {
                        { "Type", "Spotify" }, 
                        { "ProviderId", album.Id }, 
                        { "Name", album.Name }, 
                        { "AritstName", album.Artist.Name }, 
                        { "TotalCount", album.Tracks.Items.Count.ToString() }
                    }))
            {
                var index = 0;

                if (!alreadySaved)
                {
                    SavingError result;
                    do
                    {
                        // first save one song (to avoid duplicate album creation)
                        result = await _SaveTrackAsync(album.Tracks.Items[index], album, false);
                        index++;
                    }
                    while (result != SavingError.None && index < album.Tracks.Items.Count);
                }

                bool success;
                var missing = false;

                if (album.Tracks.Items.Count > 1)
                {
                    App.Locator.SqlService.BeginTransaction();

                    // save the rest at the rest time
                    var songs = album.Tracks.Items.Skip(index).Select(track => _SaveTrackAsync(track, album, false));
                    var results = await Task.WhenAll(songs);

                    // now wait a split second before showing success message
                    await Task.Delay(1000);

                    var alreadySavedCount = results.Count(p => p == SavingError.AlreadyExists);
                    handler.Data.Add("AlreadySaved", alreadySavedCount.ToString());

                    var successCount =
                        results.Count(
                            p =>
                            p == SavingError.None || p == SavingError.AlreadyExists || p == SavingError.AlreadySaving);
                    var missingCount = successCount == 0 ? -1 : album.Tracks.Items.Count - (successCount + index);
                    success = missingCount == 0;
                    missing = missingCount > 0;

                    if (missing)
                    {
                        handler.Data.Add("MissingCount", missingCount.ToString());
                        CurtainPrompt.ShowError("AlbumMissingTracks".FromLanguageResource(), missingCount, album.Name);
                    }

                    App.Locator.SqlService.Commit();
                }
                else
                {
                    success = App.Locator.CollectionService.Albums.FirstOrDefault(p => p.ProviderId.Contains(album.Id))
                              != null;
                }

                if (success)
                {
                    CurtainPrompt.Show("EntrySaved".FromLanguageResource(), album.Name);
                }
                else if (!missing)
                {
                    CurtainPrompt.ShowError("EntrySavingError".FromLanguageResource(), album.Name);
                }

                SpotifySavingAlbums.Remove(album.Id);

                if (collAlbum == null)
                {
                    collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(
                        p => p.ProviderId.Contains(album.Id));
                    await SaveAlbumImageAsync(collAlbum, album.Images[0].Url);
                    await DownloadArtistsArtworkAsync();
                }
            }
        }

        public static async Task DownloadArtistsArtworkAsync(bool missingOnly = true)
        {
            var artists = App.Locator.CollectionService.Artists.ToList();

            if (missingOnly)
            {
                artists = artists.Where(p => !p.HasArtwork && !p.NoArtworkFound).ToList();
            }

            var tasks = artists.Select(
                artist => Task.Factory.StartNew(
                    async () =>
                    {
                        if (artist.ProviderId == "autc.unknown" || string.IsNullOrEmpty(artist.Name))
                        {
                            return;
                        }

                        // don't want to retry getting this pic while we're downloading it
                        var hadArtwork = artist.HasArtwork;
                        artist.HasArtwork = true;

                        try
                        {
                            var lastArtist = await App.Locator.ScrobblerService.GetDetailArtist(artist.Name);

                            if (lastArtist.MainImage == null || lastArtist.MainImage.Largest == null)
                            {
                                artist.HasArtwork = false;

                                // By setting no artwork found we know not to try again, saving precious data!
                                artist.NoArtworkFound = true;
                                await App.Locator.SqlService.UpdateItemAsync(artist);
                                return;
                            }

                            var artistFilePath = string.Format(AppConstant.ArtistsArtworkPath, artist.Id);

                            if (await
                                SaveImageAsync(artistFilePath, lastArtist.MainImage.Largest.AbsoluteUri)
                                    .ConfigureAwait(false))
                            {

                                if (!hadArtwork)
                                {
                                    artist.HasArtwork = true;
                                    await App.Locator.SqlService.UpdateItemAsync(artist).ConfigureAwait(false);
                                }

                                await DispatcherHelper.RunAsync(
                                    () =>
                                    {
                                        artist.Artwork =
                                            new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + artistFilePath));
                                        artist.Artwork.SetDecodedPixel(App.Locator.CollectionService.ScaledImageSize);
                                    });
                            }
                            else
                            {
                                artist.HasArtwork = false;
                            }
                        }
                        catch
                        {
                            artist.HasArtwork = false;
                        }
                    })).Cast<Task>().ToList();

            App.Locator.SqlService.BeginTransaction();
            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
            }

            App.Locator.SqlService.Commit();
        }

        public static async Task DownloadAlbumsArtworkAsync(bool missingOnly = true)
        {
            var albums = App.Locator.CollectionService.Albums.ToList();

            if (missingOnly)
            {
                albums = albums.Where(p => !p.HasArtwork && !p.NoArtworkFound).ToList();
            }

            var tasks = albums.Select(
                album => Task.Factory.StartNew(
                    async () =>
                    {
                        if (string.IsNullOrEmpty(album.Name))
                        {
                            return;
                        }

                        // don't want to retry getting this pic while we're downloading it
                        album.HasArtwork = true;

                        try
                        {
                            string artworkUrl;

                            // All spotify albums have artwork
                            if (album.ProviderId.StartsWith("spotify."))
                            {
                                var spotifyAlbum =
                                    await
                                    App.Locator.Spotify.GetAlbum(album.ProviderId.Replace("spotify.", string.Empty))
                                        .ConfigureAwait(false);

                                if (spotifyAlbum == null)
                                {
                                    album.HasArtwork = false;
                                    album.NoArtworkFound = true;
                                    await App.Locator.SqlService.UpdateItemAsync(album).ConfigureAwait(false);
                                    return;
                                }

                                artworkUrl = spotifyAlbum.Images[0].Url;
                            }
                            else
                            {
                                var results =
                                    await
                                    App.Locator.DeezerService.SearchAlbumsAsync(
                                        album.Name + " " + album.PrimaryArtist.Name).ConfigureAwait(false);
                                var deezerAlbum = results.data.FirstOrDefault();

                                if (deezerAlbum == null
                                    || (!album.Name.ToLower().Contains(deezerAlbum.title.ToLower())
                                        && (!album.PrimaryArtist.Name.ToLower()
                                                 .Contains(deezerAlbum.artist.name.ToLower())
                                            || deezerAlbum.bigCover == null)))
                                {
                                    album.HasArtwork = false;
                                    album.NoArtworkFound = true;
                                    await App.Locator.SqlService.UpdateItemAsync(album).ConfigureAwait(false);
                                    return;
                                }

                                artworkUrl = deezerAlbum.bigCover;
                            }

                            await SaveAlbumImageAsync(album, artworkUrl).ConfigureAwait(false);
                        }
                        catch
                        {
                            album.HasArtwork = false;
                        }
                    })).Cast<Task>().ToList();

            App.Locator.SqlService.BeginTransaction();
            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
            }

            App.Locator.SqlService.Commit();
        }

        public static async Task SaveAlbumImageAsync(Album album, string url)
        {
            var filePath = string.Format(AppConstant.ArtworkPath, album.Id);
            if (await SaveImageAsync(filePath, url).ConfigureAwait(false))
            {
                album.HasArtwork = true;
                await App.Locator.SqlService.UpdateItemAsync(album).ConfigureAwait(false);

                await DispatcherHelper.RunAsync(
                    () =>
                    {
                        album.Artwork = new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + filePath));
                        album.Artwork.SetDecodedPixel(App.Locator.CollectionService.ScaledImageSize);

                        album.MediumArtwork = new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + filePath));
                        album.MediumArtwork.SetDecodedPixel(App.Locator.CollectionService.ScaledImageSize/2);

                        album.SmallArtwork = new PclBitmapImage(new Uri(AppConstant.LocalStorageAppPath + filePath));
                        album.SmallArtwork.SetDecodedPixel(50);
                    });
            }
        }

        private static async Task<bool> SaveImageAsync(string filePath, string url)
        {
            try
            {
                var file =
                    await
                    StorageHelper.CreateFileAsync(filePath, option: CreationCollisionOption.ReplaceExisting)
                        .ConfigureAwait(false);

                using (var client = new HttpClient())
                {
                    using (var fileStream = await file.OpenAsync(FileAccess.ReadAndWrite))
                    {
                        var buffer = await client.GetByteArrayAsync(url).ConfigureAwait(false);
                        await fileStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Playing

        private const int MaxMassPlayQueueCount = 100;

        public const double MaxPlayQueueCount = 2000;

        public static async Task PlaySongsAsync(List<Song> songs, bool random = false, bool forceClear = false)
        {
            if (songs.Count == 0)
            {
                return;
            }

            var index = random ? (songs.Count == 1 ? 0 : new Random().Next(0, songs.Count - 1)) : 0;
            var song = songs[index];

            await PlaySongsAsync(song, songs, forceClear);
        }

        public static async Task PlaySongsAsync(Song song, List<Song> songs, bool forceClear = false)
        {
            if (song == null || songs == null || songs.Count == 0)
            {
                return;
            }

            if (!song.IsMatched)
            {
                return;
            }

            songs = songs.Where(p => p.IsMatched).ToList();

            if (songs.Count == 0)
            {
                CurtainPrompt.ShowError("The songs haven't been matched yet.");
                return;
            }

            var skip = songs.IndexOf(song);
            var ordered = songs.Skip(skip).ToList();
            ordered.AddRange(songs.Take(skip));

            var overflow = songs.Count - MaxMassPlayQueueCount;
            if (overflow > 0)
            {
                for (var i = 0; i < overflow; i++)
                {
                    ordered.Remove(ordered.LastOrDefault());
                }
            }

            var playbackQueue = App.Locator.CollectionService.PlaybackQueue.ToList();

            var sameLength = _currentlyPreparing || songs.Count < playbackQueue.Count
                             || playbackQueue.Count >= MaxMassPlayQueueCount;
            var containsSong = playbackQueue.FirstOrDefault(p => p.SongId == song.Id) != null;
            var createQueue = forceClear || (!sameLength || !containsSong);

            if (_currentlyPreparing && createQueue)
            {
                // cancel the previous
                _currentlyPreparing = false;

                // wait for it to stop
                await Task.Delay(50);
            }

            if (!createQueue)
            {
                App.Locator.AudioPlayerHelper.PlaySong(playbackQueue.First(p => p.SongId == song.Id));
            }
            else
            {
                using (Insights.TrackTime("Create Queue", "Count", ordered.Count.ToString()))
                {
                    _currentlyPreparing = true;

                    try
                    {
                        await App.Locator.CollectionService.ClearQueueAsync().ConfigureAwait(false);
                    }
                    catch (SQLiteException)
                    {
                        // retry
                        try
                        {
                            App.Locator.CollectionService.ClearQueueAsync().Wait();
                        }
                        catch (SQLiteException)
                        {
                            // quit
                            CurtainPrompt.ShowError(
                                "Problem clearing the queue. You seem to be running low on storage.");
                            return;
                        }
                    }

                    var queueSong = await App.Locator.CollectionService.AddToQueueAsync(song).ConfigureAwait(false);
                    App.Locator.AudioPlayerHelper.PlaySong(queueSong);

                    App.Locator.SqlService.BeginTransaction();
                    for (var index = 1; index < ordered.Count; index++)
                    {
                        if (!_currentlyPreparing)
                        {
                            break;
                        }

                        var s = ordered[index];
                        await App.Locator.CollectionService.AddToQueueAsync(s).ConfigureAwait(false);
                    }

                    App.Locator.SqlService.Commit();

                    _currentlyPreparing = false;
                }
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

                        // only add if is not there already
                        if (playlist.Songs.FirstOrDefault(p => p.SongId == song.Id) == null)
                        {
                            await App.Locator.CollectionService.AddToPlaylistAsync(playlist, song).ConfigureAwait(false);
                        }

                        if (App.Locator.Player.CurrentQueue != null || !App.Locator.Settings.AddToInsert)
                        {
                            continue;
                        }

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
            var originalCount = songs.Count;
            songs = songs.Where(p => p.IsMatched).ToList();

            if (originalCount > 0 && songs.Count == 0)
            {
                CurtainPrompt.ShowError("The songs haven't been matched yet.");
                return;
            }

            if (originalCount == 0)
            {
                CurtainPrompt.ShowError("Item has no songs.");
                return;
            }

            using (var handle = Insights.TrackTime("AddListToQueue", "Count", songs.Count.ToString()))
            {
                App.Locator.SqlService.BeginTransaction();
                for (var i = 0; i < songs.Count; i++)
                {
                    var song = songs[i];
                    var playIfNotActive = i
                                          == (App.Locator.Settings.AddToInsert
                                              && App.Locator.Player.CurrentQueue != null
                                                  ? songs.Count - 1
                                                  : 0);

                    // the last song insert it into the shuffle list (the others shuffle them around)
                    await
                        AddToQueueAsync(song, i == songs.Count - 1, playIfNotActive, i == 0, ignoreInsertMode)
                            .ConfigureAwait(false);
                }

                App.Locator.SqlService.Commit();
                handle.Data.Add("FinalCount", App.Locator.CollectionService.PlaybackQueue.Count.ToString());
            }

            var overflow = App.Locator.CollectionService.CurrentPlaybackQueue.Count - MaxPlayQueueCount;
            if (overflow > 0)
            {
                for (var i = 0; i < overflow; i++)
                {
                    var queueToRemove = App.Locator.CollectionService.CurrentPlaybackQueue.FirstOrDefault();
                    if (queueToRemove == App.Locator.Player.CurrentQueue)
                    {
                        queueToRemove = App.Locator.CollectionService.CurrentPlaybackQueue[1];
                    }

                    await App.Locator.CollectionService.DeleteFromQueueAsync(queueToRemove).ConfigureAwait(false);
                }
            }
        }

        public static async Task<QueueSong> AddToQueueAsync(
            Song song, 
            bool shuffleInsert = true, 
            bool playIfNotActive = true, 
            bool clearIfNotActive = true, 
            bool ignoreInsertMode = false)
        {
            if (!song.IsMatched)
            {
                CurtainPrompt.ShowError("Can't add unmatch songs to queue.");
            }

            QueueSong queueSong;
            using (var handle = Insights.TrackTime("Add Song To Queue"))
            {
                if (_currentlyPreparing)
                {
                    CurtainPrompt.ShowError("GenericTryAgain".FromLanguageResource());
                    return null;
                }

                if (!App.Locator.Player.IsPlayerActive && clearIfNotActive)
                {
                    await App.Locator.CollectionService.ClearQueueAsync();
                }

                var insert = App.Locator.AppSettingsHelper.Read("AddToInsert", true, SettingsStrategy.Roaming)
                             && !ignoreInsertMode;

                queueSong =
                    await
                    App.Locator.CollectionService.AddToQueueAsync(
                        song, 
                        insert ? App.Locator.Player.CurrentQueue : null, 
                        shuffleInsert).ConfigureAwait(false);

                if (!App.Locator.Player.IsPlayerActive && playIfNotActive)
                {
                    App.Locator.AudioPlayerHelper.PlaySong(queueSong);
                    DispatcherHelper.RunAsync(() => App.Locator.Player.CurrentQueue = queueSong);
                }

                handle.Data.Add("FinalCount", App.Locator.CollectionService.PlaybackQueue.Count.ToString());
            }

            var overflow = App.Locator.CollectionService.CurrentPlaybackQueue.Count - MaxPlayQueueCount;
            if (overflow > 0)
            {
                for (var i = 0; i < overflow; i++)
                {
                    var queueToRemove = App.Locator.CollectionService.CurrentPlaybackQueue.FirstOrDefault();
                    if (queueToRemove == App.Locator.Player.CurrentQueue || queueToRemove == queueSong)
                    {
                        queueToRemove = App.Locator.CollectionService.CurrentPlaybackQueue[1];
                    }

                    await App.Locator.CollectionService.DeleteFromQueueAsync(queueToRemove).ConfigureAwait(false);
                }
            }

            return queueSong;
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

        private static async Task<SavingError> _SaveTrackAsync(
            SimpleTrack track, 
            FullAlbum album, 
            bool onFinishDownloadArtwork = true)
        {
            if (track == null || album == null)
            {
                return SavingError.Unknown;
            }

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
            {
                App.Locator.SqlService.BeginTransaction();
            }

            var result = await SpotifyHelper.SaveTrackAsync(track, album);

            if (startTransaction)
            {
                App.Locator.SqlService.Commit();
            }

            ShowErrorResults(result.Error, track.Name);

            SpotifySavingTracks.Remove(track.Id);

            if (!onFinishDownloadArtwork)
            {
                return result.Error;
            }

            if (result.Song != null))
            {
                if (!result.Song.Album.HasArtwork && !result.Song.Album.NoArtworkFound)
                {
                    SaveAlbumImageAsync(result.Song.Album, album.Images[0].Url);
                }

                DownloadArtistsArtworkAsync();
            }

            return result.Error;
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
            {
                App.Locator.SqlService.BeginTransaction();
            }

            var result = await ScrobblerHelper.SaveTrackAsync(track);

            if (startTransaction)
            {
                App.Locator.SqlService.Commit();
            }

            ShowErrorResults(result.Error, track.Name);

            LastfmSavingTracks.Remove(track.Id);

            if (!result.Song.Album.HasArtwork && !result.Song.Album.NoArtworkFound)
            {
                if (track.Images != null && track.Images.Largest != null)
                {
                    SaveAlbumImageAsync(result.Song.Album, track.Images.Largest.AbsoluteUri);
                }
                else
                {
                    DownloadAlbumsArtworkAsync();
                }
            }

            DownloadArtistsArtworkAsync();
            return result.Error;
        }

        private static void ShowErrorResults(SavingError result, string trackName)
        {
            switch (result)
            {
                case SavingError.Network:
                    CurtainPrompt.Show("SongSavingNetworkError".FromLanguageResource(), trackName);
                    break;
                case SavingError.Unknown:
                    CurtainPrompt.ShowError("EntrySavingError".FromLanguageResource(), trackName);
                    break;
            }
        }

        #endregion
    }
}