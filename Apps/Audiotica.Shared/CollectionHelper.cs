#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Collection.SqlHelper;
using Audiotica.Data.Model.Spotify.Models;

#endregion

namespace Audiotica
{
    public static class CollectionHelper
    {
        public static List<SimpleTrack> Saving = new List<SimpleTrack>();

        public static async Task SaveTrackAsync(SimpleTrack track, FullAlbum album)
        {
            CurtainToast.Show("Finding mp3 for '{0}'.", track.Name);
            var result = await _SaveTrackAsync(track, album);

            switch (result)
            {
                case SavingError.AlreadySaving:
                    CurtainToast.ShowError("Already saving '{0}'.", track.Name);
                    break;
                case SavingError.AlreadyExists:
                    CurtainToast.ShowError("Song '{0}' already saved.", track.Name);
                    break;
            }
        }

        private static async Task<SavingError> _SaveTrackAsync(SimpleTrack track, FullAlbum album)
        {
            var alreadySaving = Saving.FirstOrDefault(p => p.Id == track.Id) != null;

            if (alreadySaving)
            {
                return SavingError.AlreadySaving;
            }

            Saving.Add(track);

            var result = await SpotifyHelper.SaveTrackAsync(track, album);

            switch (result)
            {
                case SavingError.Network:
                    CurtainToast.Show("Network error finding mp3 for '{0}'.", track.Name);
                    break;
                case SavingError.NoMatch:
                    CurtainToast.ShowError("No mp3 found for '{0}'.", track.Name);
                    break;
                case SavingError.None:
                    CurtainToast.Show("Song '{0}' saved.", track.Name);
                    break;
                case SavingError.Unknown:
                    CurtainToast.ShowError("Problem saving song '{0}'", track.Name);
                    break;
            }

            Saving.Remove(track);

            return result;
        }

        public static async Task SaveAlbumAsync(FullAlbum album)
        {
            CurtainToast.Show("Saving album '{0}'.", album.Name);

            var songs = album.Tracks.Items.Select(track => _SaveTrackAsync(track, album));
            await Task.WhenAll(songs);

            CurtainToast.Show("Album '{0}' saved.", album.Name);
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