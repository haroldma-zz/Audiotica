#region

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Data.Spotify.Models;

#endregion

namespace Audiotica
{
    public static class CollectionHelper
    {
        public static List<SimpleTrack> SavingTracks = new List<SimpleTrack>();
        public static List<FullAlbum> SavingAlbums = new List<FullAlbum>();

        public static async Task SaveTrackAsync(ChartTrack chartTrack)
        {
            CurtainToast.Show("Finding mp3 for \"{0}\".", chartTrack.Name);
            var track = await App.Locator.Spotify.GetTrack(chartTrack.track_id);
            var album = await App.Locator.Spotify.GetAlbum(track.Album.Id);

            await SaveTrackAsync(track, album, false);
        }

        public static async Task SaveTrackAsync(SimpleTrack track, FullAlbum album, bool showFindingMessage = true)
        {
            if (showFindingMessage)
                CurtainToast.Show("Finding mp3 for \"{0}\".", track.Name);

            var result = await _SaveTrackAsync(track, album);

            switch (result)
            {
                case SavingError.AlreadySaving:
                    CurtainToast.ShowError("Already saving \"{0}\".", track.Name);
                    break;
                case SavingError.AlreadyExists:
                    CurtainToast.ShowError("Already saved \"{0}\".", track.Name);
                    break;
                case SavingError.None:
                    CurtainToast.Show("Saved song \"{0}\".", track.Name);
                    break;
            }
        }

        private static async Task<SavingError> _SaveTrackAsync(SimpleTrack track, FullAlbum album)
        {
            var alreadySaving = SavingTracks.FirstOrDefault(p => p.Id == track.Id) != null;

            if (alreadySaving)
            {
                return SavingError.AlreadySaving;
            }

            SavingTracks.Add(track);

            var result = await SpotifyHelper.SaveTrackAsync(track, album);

            switch (result)
            {
                case SavingError.Network:
                    CurtainToast.Show("Network error finding mp3 for \"{0}\".", track.Name);
                    break;
                case SavingError.NoMatch:
                    CurtainToast.ShowError("No mp3 found for \"{0}\".", track.Name);
                    break;
                case SavingError.Unknown:
                    CurtainToast.ShowError("Problem saving song \"{0}\"", track.Name);
                    break;
            }

            SavingTracks.Remove(track);

            return result;
        }

        public static async Task SaveAlbumAsync(FullAlbum album)
        {
            if (album.Tracks.Items.Count == 0)
            {
                CurtainToast.ShowError("Album has no tracks.");
                return;
            }

            var collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(p => p.ProviderId.Contains(album.Id));

            var alreadySaved = collAlbum != null && collAlbum.Songs.Count >= album.Tracks.Items.Count;
            var alreadySaving = SavingAlbums.FirstOrDefault(p => p.Id == album.Id) != null;

            if (alreadySaved)
            {
                CurtainToast.ShowError("Already saved \"{0}\".", album.Name);
                return;
            }

            if (alreadySaving)
            {
                CurtainToast.ShowError("Already saving \"{0}\".", album.Name);
                return;
            }

            SavingAlbums.Add(album);

            CurtainToast.Show("Saving album \"{0}\".", album.Name);

            SavingError result;
            var index = 0;

            do
            {
                //first save one song (to avoid duplicate album creation)
                result = await _SaveTrackAsync(album.Tracks.Items[index], album);
                index++;
            } while (result != SavingError.None && index < album.Tracks.Items.Count);

            //save the rest at the rest time
            var songs = album.Tracks.Items.Skip(index + 1).Select(track => _SaveTrackAsync(track, album));
            await Task.WhenAll(songs);

            //now wait a split second before showing success message
            await Task.Delay(1000);

            if (collAlbum == null)
            {
                collAlbum = App.Locator.CollectionService.Albums.FirstOrDefault(p => p.ProviderId.Contains(album.Id));
            }

            var missing = collAlbum == null ? -1 : album.Tracks.Items.Count - collAlbum.Songs.Count;

            if (missing == 0)
                CurtainToast.Show("Saved album \"{0}\".", album.Name);
            else if (missing < 0)
                CurtainToast.ShowError("Failed to save album \"{0}\".", album.Name);
            else
                CurtainToast.ShowError("Couldn't save {0} song(s) of \"{1}\".", missing, album.Name);

            SavingAlbums.Remove(album);
        }
    }

    public enum SavingError
    {
        None,
        AlreadyExists,
        NoMatch,
        Network,
        AlreadySaving,
        Unknown
    }
}