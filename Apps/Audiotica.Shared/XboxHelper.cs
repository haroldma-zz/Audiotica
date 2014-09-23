#region

using System;
using Audiotica.Data.Collection.Model;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

#endregion

namespace Audiotica
{
    public static class XboxHelper
    {
        public static Artist ToArtist(this XboxArtist xboxArtist)
        {
            return new Artist
            {
                Name = xboxArtist.Name,
                ProviderId = xboxArtist.Id
            };
        }

        public static Album ToAlbum(this XboxAlbum xboxAlbum)
        {
            var album = new Album
            {
                ProviderId = xboxAlbum.Id,
                Name = xboxAlbum.Name
            };

            if (xboxAlbum.ReleaseDate != null)
                album.ReleaseDate = (DateTime) xboxAlbum.ReleaseDate;

            album.Genre = xboxAlbum.Genres != null ? xboxAlbum.Genres[0] : "";

            return album;
        }

        public static Song ToSong(this XboxTrack track)
        {
            var song = new Song
            {
                ProviderId = track.Id,
                Name = track.Name,
                Artist = track.Artists[0].Artist.ToArtist(),
                Album = track.Album.ToAlbum(),
                TrackNumber = track.TrackNumber ?? 0
            };
            song.Album.PrimaryArtist = song.Artist;
            return song;
        }
    }
}