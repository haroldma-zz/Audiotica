using System;
using System.Linq;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Web.Models.Meile;
using Audiotica.Web.Models.Mp3Clan;
using Audiotica.Web.Models.Netease;
using Audiotica.Web.Models.SoundCloud;
using Google.Apis.YouTube.v3.Data;

namespace Audiotica.Web.Models
{
    public class WebSong
    {
        public WebSong()
        {
        }

        public WebSong(SearchResult youtubeVideo)
        {
            Id = youtubeVideo.Id.VideoId;
            Name = youtubeVideo.Snippet.Title;
            FileAuthor = youtubeVideo.Snippet.ChannelTitle;
            ArtworkImage = new Uri(youtubeVideo.Snippet.Thumbnails.High.Url);
        }

        public WebSong(Mp3ClanSong mp3ClanSong)
        {
            Id = mp3ClanSong.Id;
            Name = mp3ClanSong.Title;
            Artist = mp3ClanSong.Artist;
            AudioUrl = mp3ClanSong.Url;
            if (string.IsNullOrEmpty(mp3ClanSong.Duration)) return;

            // format is x:xx, to parse correctly making it 00:x:xx
            var prefix = "0:";

            if (mp3ClanSong.Duration.Length <= 3)
                prefix += "0";

            Duration = TimeSpan.Parse(prefix + mp3ClanSong.Duration);
        }

        public WebSong(SoundCloudSong soundCloudSong)
        {
            Id = soundCloudSong.Id.ToString();
            Name = soundCloudSong.Title;
            AudioUrl = $"{soundCloudSong.StreamUrl}?client_id={ApiKeys.SoundCloudId}";
            Duration = TimeSpan.FromMilliseconds(soundCloudSong.Duration);
            ByteSize = soundCloudSong.OriginalContentSize;
            FileAuthor = soundCloudSong.User.Username;

            if (!string.IsNullOrEmpty(soundCloudSong.ArtworkUrl))
                ArtworkImage = new Uri(soundCloudSong.ArtworkUrl);
        }

        public WebSong(NeteaseDetailSong neteaseSong)
        {
            Id = neteaseSong.Id.ToString();
            Name = neteaseSong.Name;
            Artist = neteaseSong.Artists.FirstOrDefault().Name;
            AudioUrl = neteaseSong.Mp3Url;
            Duration = TimeSpan.FromMilliseconds(neteaseSong.Duration);
            BitRate = neteaseSong.BMusic.Bitrate;
            ByteSize = neteaseSong.BMusic.Size;
        }

        public WebSong(MeileSong meileSong)
        {
            Id = meileSong.Id.ToString();
            Name = meileSong.Name;
            Artist = meileSong.ArtistName;
            AudioUrl = meileSong.Mp3;
            Duration = TimeSpan.FromSeconds(meileSong.Duration);
            if (!string.IsNullOrEmpty(meileSong.NormalCover))
                ArtworkImage = new Uri(meileSong.NormalCover);
        }

        public string FormattedBytes => ByteSize.ToFormattedBytes();
        public string Artist { get; set; }
        public Uri ArtworkImage { get; set; }
        public string AudioUrl { get; set; }
        public int BitRate { get; set; }
        public long ByteSize { get; set; }
        public TimeSpan Duration { get; set; }
        public string Id { get; set; }
        public string FileAuthor { get; set; }
        public bool IsBestMatch { get; set; }
        public bool IsLinkDeath { get; set; }
        public bool IsMatch { get; set; }
        public string Name { get; set; }
    }
}