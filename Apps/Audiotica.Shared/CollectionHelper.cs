#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Spotify.Models;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica
{
    public static class CollectionHelper
    {
        public static List<SimpleTrack> SpotifySavingTracks = new List<SimpleTrack>();
        public static List<LastTrack> LastfmSavingTracks = new List<LastTrack>();
        public static List<FullAlbum> SpotifySavingAlbums = new List<FullAlbum>();
        public static List<LastAlbum> LastfmSavingAlbums = new List<LastAlbum>();

        public static async Task SaveTrackAsync(ChartTrack chartTrack)
        {
            CurtainPrompt.Show("SongSavingFindingMp3".FromLanguageResource(), chartTrack.Name);
            var track = await App.Locator.Spotify.GetTrack(chartTrack.track_id);
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

            var collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(p => p.ProviderId.Contains(album.Id));

            var alreadySaved = collAlbum != null;
            var alreadySaving = SpotifySavingAlbums.FirstOrDefault(p => p.Id == album.Id) != null;

            if (alreadySaved)
            {
                var missingTracks = collAlbum.Songs.Count < album.Tracks.Items.Count;
                if (!missingTracks)
                {
                    CurtainPrompt.ShowError("EntryAlreadySaved".FromLanguageResource(), album.Name);
                    return;
                }
            }

            if (alreadySaving)
            {
                CurtainPrompt.ShowError("EntryAlreadySaving".FromLanguageResource(), album.Name);
                return;
            }

            SpotifySavingAlbums.Add(album);

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

            //save the rest at the rest time
            var songs = album.Tracks.Items.Skip(index).Select(track => _SaveTrackAsync(track, album));
            var results = await Task.WhenAll(songs);

            //now wait a split second before showing success message
            await Task.Delay(1000);

            var successCount = results.Count(p => p == SavingError.None || p == SavingError.AlreadyExists
                                                  || p == SavingError.AlreadySaving);
            var missingCount = successCount == 0 ? -1 : album.Tracks.Items.Count - (successCount + index);
            var success = missingCount == 0;
            var missing = missingCount > 0;

            if (success)
                CurtainPrompt.Show("EntrySaved".FromLanguageResource(), album.Name);
            else if (missing)
                CurtainPrompt.ShowError("AlbumMissingTracks".FromLanguageResource(), missingCount, album.Name);
            else
                CurtainPrompt.ShowError("EntrySavingError".FromLanguageResource(), album.Name);


            SpotifySavingAlbums.Remove(album);
        }

        #region Heper methods

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
            var alreadySaving = SpotifySavingTracks.FirstOrDefault(p => p.Id == track.Id) != null;

            if (alreadySaving)
            {
                return SavingError.AlreadySaving;
            }

            SpotifySavingTracks.Add(track);

            var result = await SpotifyHelper.SaveTrackAsync(track, album);

            ShowErrorResults(result, track.Name);

            SpotifySavingTracks.Remove(track);

            return result;
        }

        private static async Task<SavingError> _SaveTrackAsync(LastTrack track)
        {
            var alreadySaving = LastfmSavingTracks.FirstOrDefault(p => p.Id == track.Id) != null;

            if (alreadySaving)
            {
                return SavingError.AlreadySaving;
            }

            LastfmSavingTracks.Add(track);

            var result = await ScrobblerHelper.SaveTrackAsync(track);

            ShowErrorResults(result, track.Name);

            LastfmSavingTracks.Remove(track);

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

    }
}