#region

using System;
using System.Linq;
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
                Name = lastArtist.Name,
                ProviderId = !string.IsNullOrEmpty(lastArtist.Mbid) ? ("mbid." + lastArtist.Mbid) : ("lastid." + lastArtist.Id),
            };
        }

        public static Album ToAlbum(this LastAlbum lastAlbum)
        {
            var album = new Album
            {
                ProviderId = !string.IsNullOrEmpty(lastAlbum.Mbid) ? ("mbid." + lastAlbum.Mbid) : ("lastid." + lastAlbum.Id),
                Name = lastAlbum.Name,
                ReleaseDate = lastAlbum.ReleaseDateUtc,
                Genre = lastAlbum.TopTags != null ? lastAlbum.TopTags.First().Name : ""
            };

            return album;
        }

        public static Song ToSong(this LastTrack track)
        {
            var song = new Song
            {
                ProviderId = !string.IsNullOrEmpty(track.Mbid) ? ("mbid." + track.Mbid) : ("lastid." + track.Id),
                Name = track.Name
            };
            return song;
        }
    }
}