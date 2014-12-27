#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
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

        public static async Task SaveTrackAsync(LastTrack track)
        {
            var url = await Mp3MatchEngine.FindMp3For(track);

            if (string.IsNullOrEmpty(url))
                CurtainToast.ShowError("NoMatchFoundToast".FromLanguageResource());

            else
            {
                var preparedSong = await PrepareTrackForDownloadAsync(track);
                preparedSong.Song.AudioUrl = url;

                try
                {
                    await App.Locator.CollectionService.AddSongAsync(preparedSong.Song, preparedSong.ArtworkUrl);
                    CurtainToast.Show("SongSavedToast".FromLanguageResource());
                }
                catch (Exception e)
                {
                    CurtainToast.ShowError(e.Message);
                }
            }
        }

        internal static async Task<PreparedSong> PrepareTrackForDownloadAsync(LastTrack lastTrack)
        {
            var track = await App.Locator.ScrobblerService.GetDetailTrack(lastTrack.Name, lastTrack.ArtistName);
            var preparedSong = new PreparedSong {Song = track.ToSong()};
            LastArtist artist;

            preparedSong.Song.ArtistName = track.ArtistName;

            if (!string.IsNullOrEmpty(lastTrack.AlbumName + track.AlbumName))
            {
                var lastAlbum = await App.Locator.ScrobblerService.GetDetailAlbum(
                    string.IsNullOrEmpty(lastTrack.AlbumName) ? track.AlbumName : lastTrack.AlbumName, track.ArtistName);

                if (track.ArtistMbid == null)
                    artist = await App.Locator.ScrobblerService.GetDetailArtist(track.ArtistName);
                else
                    artist = await App.Locator.ScrobblerService.GetDetailArtistByMbid(track.ArtistMbid);

                preparedSong.Song.Album = lastAlbum.ToAlbum();
                preparedSong.Song.Album.PrimaryArtist = artist.ToArtist();

                if (lastAlbum.Images != null && lastAlbum.Images.Largest != null)
                    preparedSong.ArtworkUrl = lastAlbum.Images.Largest.AbsoluteUri;
            }

            else
                artist = await App.Locator.ScrobblerService.GetDetailArtist(track.ArtistName);

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