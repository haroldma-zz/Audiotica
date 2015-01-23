#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Data;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Spotify.Models;

#endregion

namespace Audiotica
{
    public static class SpotifyHelper
    {
        public static Artist ToArtist(this SimpleArtist simpleArtist)
        {
            return new Artist
            {
                Name = simpleArtist.Name.Trim().Replace("  ", " "),
                ProviderId = "spotify." + simpleArtist.Id
            };
        }

        public static Album ToAlbum(this FullAlbum fullAlbum)
        {
            var album = new Album
            {
                ProviderId = "spotify." + fullAlbum.Id,
                Name = fullAlbum.Name.Trim().Replace("  ", " "),
                ReleaseDate = GetDateTime(fullAlbum),
                Genre = fullAlbum.Genres != null ? fullAlbum.Genres.FirstOrDefault() : ""
            };

            return album;
        }

        private static DateTime GetDateTime(FullAlbum album)
        {
            if (album.ReleaseDatePrecision == "year")
            {
                var year = int.Parse(album.ReleaseDate);
                return new DateTime(year, 1, 1);
            }
            return DateTime.Parse(album.ReleaseDate);
        }

        public static Song ToSong(this SimpleTrack track)
        {
            var song = new Song
            {
                ProviderId = "spotify." + track.Id,
                Name = track.Name.Trim().Replace("  ", " "),
                TrackNumber = track.TrackNumber
            };
            return song;
        }

        public static async Task<SavingError> SaveTrackAsync(SimpleTrack track, FullAlbum album)
        {
            try
            {
                if (track == null || album == null)
                    return SavingError.Unknown;

                var preparedSong = track.ToSong();
                if (App.Locator.CollectionService.SongAlreadyExists(preparedSong.ProviderId, track.Name, album.Name,
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

                var url = await App.Locator.Mp3MatchEngine.FindMp3For(track.Name, artist.Name).ConfigureAwait(false);

                if (string.IsNullOrEmpty(url))
                    return SavingError.NoMatch;

                preparedSong.ArtistName = fullTrack != null
                    ? string.Join(", ", fullTrack.Artists.Select(p => p.Name))
                    : artist.Name;
                preparedSong.Album = album.ToAlbum();
                preparedSong.Artist = album.Artist.ToArtist();
                preparedSong.Album.PrimaryArtist = preparedSong.Artist;
                preparedSong.AudioUrl = url;


                await App.Locator.CollectionService.AddSongAsync(preparedSong,
                    album.Images[0].Url).ConfigureAwait(false);
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