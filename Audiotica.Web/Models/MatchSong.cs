using System;
using System.Linq;
using Audiotica.Core.Common;
using Audiotica.Core.Extensions;
using Audiotica.Web.Http.Requets.MatchEngine.Meile.Models;
using Audiotica.Web.Http.Requets.MatchEngine.Netease.Models;
using Audiotica.Web.Http.Requets.MatchEngine.SoundCloud.Models;
using Google.Apis.YouTube.v3.Data;

namespace Audiotica.Web.Models
{
    public class MatchSong
    {
        public MatchSong()
        {
        }

        public MatchSong(SearchResult youtubeVideo)
        {
            Id = youtubeVideo.Id.VideoId;
            Title = youtubeVideo.Snippet.Title;
            FileAuthor = youtubeVideo.Snippet.ChannelTitle;
            ArtworkImage = new Uri(youtubeVideo.Snippet.Thumbnails.High.Url);
        }

        public MatchSong(SoundCloudSong soundCloudSong)
        {
            Id = soundCloudSong.Id.ToString();
            Title = soundCloudSong.Title;
            AudioUrl = $"{soundCloudSong.StreamUrl}?client_id={ApiKeys.SoundCloudId}";
            Duration = TimeSpan.FromMilliseconds(soundCloudSong.Duration);
            ByteSize = soundCloudSong.OriginalContentSize;
            FileAuthor = soundCloudSong.User.Username;

            if (!string.IsNullOrEmpty(soundCloudSong.ArtworkUrl))
                ArtworkImage = new Uri(soundCloudSong.ArtworkUrl);
        }

        public MatchSong(NeteaseDetailSong neteaseSong)
        {
            Id = neteaseSong.Id.ToString();
            Title = neteaseSong.Name;
            Artist = neteaseSong.Artists.FirstOrDefault().Name;
            AudioUrl = neteaseSong.Mp3Url;
            Duration = TimeSpan.FromMilliseconds(neteaseSong.Duration);
            BitRate = neteaseSong.BMusic.Bitrate;
            ByteSize = neteaseSong.BMusic.Size;
        }

        public MatchSong(MeileSong meileSong)
        {
            Id = meileSong.Id.ToString();
            Title = meileSong.Name;
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
        public string Title { get; set; }
    }
}