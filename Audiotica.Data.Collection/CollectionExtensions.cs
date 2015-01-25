using System;
using System.Linq;
using Audiotica.Data.Collection.Model;
using Audiotica.Data.Spotify.Models;
using IF.Lastfm.Core.Objects;

namespace Audiotica.Data.Collection
{
    public static class CollectionExtensions
    {
        public static Artist ToArtist(this LastArtist lastArtist)
        {
            return new Artist
            {
                Name = lastArtist.Name.Trim().Replace("  ", " "),
                ProviderId =
                    !string.IsNullOrEmpty(lastArtist.Mbid) ? ("mbid." + lastArtist.Mbid) : ("lastid." + lastArtist.Id)
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
            if (album.ReleaseDatePrecision != "year") return DateTime.Parse(album.ReleaseDate);
            var year = int.Parse(album.ReleaseDate);
            return new DateTime(year, 1, 1);
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
    }
}