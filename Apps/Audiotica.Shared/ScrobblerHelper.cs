#region

using System.Threading.Tasks;

using Audiotica.Core.Exceptions;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;

using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica
{
    public static class ScrobblerHelper
    {
        public static async Task<SavingError> SaveTrackAsync(LastTrack track)
        {
            try
            {
                Song preparedSong = null;
                var providerId = track.ToSong().ProviderId;
                if (providerId == "lastid.")
                {
                    preparedSong = await PrepareTrackForDownloadAsync(track);
                    providerId = preparedSong.ProviderId;
                }

                if (App.Locator.CollectionService.SongAlreadyExists(
                    providerId, 
                    track.Name, 
                    track.AlbumName, 
                    track.ArtistName))
                {
                    return SavingError.AlreadyExists;
                }

                if (preparedSong == null)
                {
                    preparedSong = await PrepareTrackForDownloadAsync(track);
                }

                await App.Locator.CollectionService.AddSongAsync(preparedSong).ConfigureAwait(false);

                CollectionHelper.MatchSong(preparedSong);
                return SavingError.None;
            }
            catch (NetworkException)
            {
                return SavingError.Network;
            }
            catch
            {
                return SavingError.Unknown;
            }
        }

        internal static async Task<Song> PrepareTrackForDownloadAsync(LastTrack lastTrack)
        {
            var track =
                await
                App.Locator.ScrobblerService.GetDetailTrack(lastTrack.Name, lastTrack.ArtistName).ConfigureAwait(false);
            LastArtist artist;

            var preparedSong = track.ToSong();
            preparedSong.ArtistName = track.ArtistName;

            if (!string.IsNullOrEmpty(lastTrack.AlbumName + track.AlbumName))
            {
                var lastAlbum =
                    await
                    App.Locator.ScrobblerService.GetDetailAlbum(
                        string.IsNullOrEmpty(lastTrack.AlbumName) ? track.AlbumName : lastTrack.AlbumName, 
                        track.ArtistName);

                artist = await App.Locator.ScrobblerService.GetDetailArtist(track.ArtistName).ConfigureAwait(false);

                preparedSong.Album = lastAlbum.ToAlbum();
                preparedSong.Album.PrimaryArtist = artist.ToArtist();
            }
            else
            {
                artist = await App.Locator.ScrobblerService.GetDetailArtist(track.ArtistName).ConfigureAwait(false);
            }

            preparedSong.Artist = artist.ToArtist();

            return preparedSong;
        }
    }
}