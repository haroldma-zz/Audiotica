#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Exceptions;
using Audiotica.Core.Utilities;
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
            var preparedSong = track.ToSong();
            if (App.Locator.CollectionService.SongAlreadyExists(preparedSong.ProviderId, track.Name, album.Name, album.Artist.Name))
            {
                return SavingError.AlreadyExists;
            }

            var artist = track is FullTrack ? (track as FullTrack).Artist : track.Artist;

            string url;

            try
            {
                url = await Mp3MatchEngine.FindMp3For(track.Name, artist.Name).ConfigureAwait(false);
            }
            catch
            {
                return SavingError.Unknown;
            }

            if (string.IsNullOrEmpty(url))
                return SavingError.NoMatch;

            string artistArtwork = null;

            try
            {
                var lastArtist = await App.Locator.ScrobblerService.GetDetailArtist(artist.Name);
                artistArtwork = lastArtist.MainImage != null && lastArtist.MainImage.Largest != null 
                    ? lastArtist.MainImage.Largest.AbsoluteUri 
                    : null;
            }
            catch { }

            preparedSong.ArtistName = artist.Name;
            preparedSong.Album = album.ToAlbum();
            preparedSong.Artist = album.Artist.ToArtist();
            preparedSong.Album.PrimaryArtist = preparedSong.Artist;
            preparedSong.AudioUrl = url;

            try
            {
                await
                    App.Locator.CollectionService.AddSongAsync(preparedSong, 
                    album.Images[0].Url, 
                    artistArtwork).ConfigureAwait(false);
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