#region

using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Exceptions;
using Audiotica.Data;
using Audiotica.Data.Collection.Model;
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica
{
    public static class ScrobblerHelper
    {
        public static Artist ToArtist(this LastArtist lastArtist)
        {
            return new Artist
            {
                Name = lastArtist.Name.Trim().Replace("  ", " "),
                ProviderId =
                    !string.IsNullOrEmpty(lastArtist.Mbid) ? ("mbid." + lastArtist.Mbid) : ("lastid." + lastArtist.Id),
            };
        }

        public static Album ToAlbum(this LastAlbum lastAlbum)
        {
            var album = new Album
            {
                ProviderId =
                    !string.IsNullOrEmpty(lastAlbum.Mbid) ? ("mbid." + lastAlbum.Mbid) : ("lastid." + lastAlbum.Id),
                Name = lastAlbum.Name.Trim().Replace("  ", " "),
                ReleaseDate = lastAlbum.ReleaseDateUtc.DateTime,
                Genre = lastAlbum.TopTags != null ? lastAlbum.TopTags.First().Name : ""
            };

            return album;
        }

        public static Song ToSong(this LastTrack track)
        {
            var song = new Song
            {
                ProviderId = !string.IsNullOrEmpty(track.Mbid) ? ("mbid." + track.Mbid) : ("lastid." + track.Id),
                Name = track.Name.Trim().Replace("  ", " ")
            };
            return song;
        }

        public static async Task<SavingError> SaveTrackAsync(LastTrack track)
        {
            try
            {
                PreparedSong preparedSong = null;
                var providerId = track.ToSong().ProviderId;
                if (providerId == "lastid.")
                {
                    preparedSong = await PrepareTrackForDownloadAsync(track);
                    providerId = preparedSong.Song.ProviderId;
                }

                if (App.Locator.CollectionService.SongAlreadyExists(providerId, track.Name, track.AlbumName,
                    track.ArtistName))
                {
                    return SavingError.AlreadyExists;
                }

                var url =
                    await App.Locator.Mp3MatchEngine.FindMp3For(track.Name, track.ArtistName).ConfigureAwait(false);

                if (string.IsNullOrEmpty(url))
                    return SavingError.NoMatch;

                if (preparedSong == null)
                    preparedSong = await PrepareTrackForDownloadAsync(track);
                preparedSong.Song.AudioUrl = url;


                await App.Locator.CollectionService.AddSongAsync(preparedSong.Song, preparedSong.ArtworkUrl)
                    .ConfigureAwait(false);
                CollectionHelper.DownloadArtistsArtworkAsync();
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

        internal static async Task<PreparedSong> PrepareTrackForDownloadAsync(LastTrack lastTrack)
        {
            var track =
                await
                    App.Locator.ScrobblerService.GetDetailTrack(lastTrack.Name, lastTrack.ArtistName)
                        .ConfigureAwait(false);
            var preparedSong = new PreparedSong {Song = track.ToSong()};
            LastArtist artist;

            preparedSong.Song.ArtistName = track.ArtistName;

            if (!string.IsNullOrEmpty(lastTrack.AlbumName + track.AlbumName))
            {
                var lastAlbum = await App.Locator.ScrobblerService.GetDetailAlbum(
                    string.IsNullOrEmpty(lastTrack.AlbumName) ? track.AlbumName : lastTrack.AlbumName, track.ArtistName);

                artist = await App.Locator.ScrobblerService.GetDetailArtist(track.ArtistName).ConfigureAwait(false);

                preparedSong.Song.Album = lastAlbum.ToAlbum();
                preparedSong.Song.Album.PrimaryArtist = artist.ToArtist();

                if (lastAlbum.Images != null && lastAlbum.Images.Largest != null)
                    preparedSong.ArtworkUrl = lastAlbum.Images.Largest.AbsoluteUri;
            }

            else
                artist = await App.Locator.ScrobblerService.GetDetailArtist(track.ArtistName).ConfigureAwait(false);

            if (string.IsNullOrEmpty(preparedSong.ArtworkUrl))
                preparedSong.ArtworkUrl = (track.Images != null && track.Images.Largest != null)
                    ? track.Images.Largest.AbsoluteUri
                    : null;

            preparedSong.Song.Artist = artist.ToArtist();

            return preparedSong;
        }

        internal class PreparedSong
        {
            public Song Song { get; set; }
            public string ArtworkUrl { get; set; }
        }
    }
}