using System;
using System.Collections.Generic;
using System.Text;
using Audiotica.Collection.Model;
using Microsoft.Xbox.Music.Platform.Contract.DataModel;

namespace Audiotica
{
    public static class XboxHelper
    {
        public static Artist ToArtist(this XboxArtist xboxArtist)
        {
            return new Artist
            {
                Name = xboxArtist.Name,
                XboxId = xboxArtist.Id
            };
        }

        public static Album ToAlbum(this XboxAlbum xboxAlbum)
        {
            var album = new Album
            {
                XboxId = xboxAlbum.Id,
                Name = xboxAlbum.Name
            };

            if (xboxAlbum.ReleaseDate != null)
                album.ReleaseDate = (DateTime) xboxAlbum.ReleaseDate;

            album.Genre = xboxAlbum.Genres[0];

            return album;
        }

        public static Song ToSong(this XboxTrack track)
        {
            return new Song
            {
                XboxId = track.Id,
                Name = track.Name,
                Artist = track.Artists[0].Artist.ToArtist(),
                Album = track.Album.ToAlbum(),
                TrackNumber = track.TrackNumber ?? 0
            };
        }
    }
}
