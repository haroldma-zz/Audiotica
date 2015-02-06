#region

using System.Linq;
using System.Threading.Tasks;

using Audiotica.Core.Exceptions;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Spotify.Models;

#endregion

namespace Audiotica
{
    public static class SpotifyHelper
    {
        public static async Task<SavingError> SaveTrackAsync(SimpleTrack track, FullAlbum album)
        {
            try
            {
                if (track == null || album == null)
                {
                    return SavingError.Unknown;
                }

                var preparedSong = track.ToSong();
                if (App.Locator.CollectionService.SongAlreadyExists(
                    preparedSong.ProviderId, 
                    track.Name, 
                    album.Name, 
                    album.Artist != null ? album.Artist.Name : track.Artist.Name))
                {
                    return SavingError.AlreadyExists;
                }

                var fullTrack = track as FullTrack;
                if (fullTrack == null)
                {
                    try
                    {
                        fullTrack = await App.Locator.Spotify.GetTrack(track.Id);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                var artist = fullTrack != null ? fullTrack.Artist : track.Artist;

                preparedSong.ArtistName = fullTrack != null
                                              ? string.Join(", ", fullTrack.Artists.Select(p => p.Name))
                                              : artist.Name;
                preparedSong.Album = album.ToAlbum();
                preparedSong.Artist = album.Artist.ToArtist();
                preparedSong.Album.PrimaryArtist = preparedSong.Artist;
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
    }
}