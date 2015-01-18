using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Audiotica.Data.Collection.Model;

namespace Audiotica.Data.Model
{
    public class LocalSong
    {
        public LocalSong(string title, string artist, string album, string albumArtist)
        {
            Title = CleanText(title);
            ArtistName = CleanText(artist);
            AlbumName = CleanText(album);
            AlbumArtist = CleanText(albumArtist);

            if (!string.IsNullOrEmpty(ArtistName) || !string.IsNullOrEmpty(AlbumArtist))
                ArtistId = Convert.ToBase64String(Encoding.UTF8.GetBytes((AlbumArtist ?? ArtistName).ToLower()));
            if (!string.IsNullOrEmpty(AlbumName))
                AlbumId = Convert.ToBase64String(Encoding.UTF8.GetBytes(AlbumName.ToLower()));
            if (!string.IsNullOrEmpty(Title))
                Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(Title.ToLower())) + ArtistId + AlbumId;
        }

        public LocalSong(MusicProperties musicProps)
        {
            Title = CleanText(musicProps.Title);
            AlbumName = CleanText(musicProps.Album);
            AlbumArtist = CleanText(musicProps.AlbumArtist);
            ArtistName = CleanText(musicProps.Artist);

            BitRate = (int)musicProps.Bitrate;
            Duration = musicProps.Duration;
            Genre = musicProps.Genre.FirstOrDefault();
            TrackNumber = (int)musicProps.TrackNumber;

            if (!string.IsNullOrEmpty(ArtistName) || !string.IsNullOrEmpty(AlbumArtist))
                ArtistId = Convert.ToBase64String(Encoding.UTF8.GetBytes((AlbumArtist ?? ArtistName).ToLower()));
            if (!string.IsNullOrEmpty(AlbumName))
                AlbumId = Convert.ToBase64String(Encoding.UTF8.GetBytes(AlbumName.ToLower()));
            if (!string.IsNullOrEmpty(Title))
                Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(Title.ToLower())) + ArtistId + AlbumId;

            if (musicProps.Rating > 1)
                HeartState = HeartState.Like;
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            //[^0-9a-zA-Z]+
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public string Id { get; set; }
        public string ArtistId { get; set; }
        public string AlbumId { get; set; }

        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string AlbumArtist { get; set; }
        public string Genre { get; set; }
        public string FilePath { get; set; }
        public int BitRate { get; set; }
        public int TrackNumber { get; set; }
        public HeartState HeartState { get; set; }
        public Uri ArtworkImage { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
