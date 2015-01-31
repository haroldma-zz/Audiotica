using System;
using System.Linq;

using Audiotica.Data.Model.SoundCloud;

using Google.Apis.YouTube.v3.Data;

namespace Audiotica.Data.Model
{
    public class WebSong
    {
        public WebSong()
        {
        }

        public WebSong(SearchResult youtubeVideo)
        {
            this.Id = youtubeVideo.Id.VideoId;
            this.Name = youtubeVideo.Snippet.Title;
            this.Provider = Mp3Provider.YouTube;
            this.ArtworkImage = new Uri(youtubeVideo.Snippet.Thumbnails.High.Url);
        }

        public WebSong(Mp3ClanSong mp3ClanSong)
        {
            this.Id = mp3ClanSong.tid;
            this.Name = mp3ClanSong.title;
            this.Artist = mp3ClanSong.artist;
            this.AudioUrl = mp3ClanSong.url;
            this.Provider = Mp3Provider.Mp3Clan;
            if (!string.IsNullOrEmpty(mp3ClanSong.duration))
            {
                // format is x:xx, to parse correctly making it 00:x:xx
                var prefix = "0:";

                if (mp3ClanSong.duration.Length <= 3)
                {
                    prefix += "0";
                }

                this.Duration = TimeSpan.Parse(prefix + mp3ClanSong.duration);
            }
        }

        public WebSong(SoundCloudSong soundCloudSong)
        {
            this.Id = soundCloudSong.id.ToString();
            this.Name = soundCloudSong.title;
            this.AudioUrl = soundCloudSong.stream_url;
            this.Provider = Mp3Provider.SoundCloud;
            this.Duration = TimeSpan.FromMilliseconds(soundCloudSong.duration);
            this.ByteSize = soundCloudSong.original_content_size;
            if (!string.IsNullOrEmpty(soundCloudSong.artwork_url))
            {
                this.ArtworkImage = new Uri(soundCloudSong.artwork_url);
            }
        }

        public WebSong(NeteaseDetailSong neteaseSong)
        {
            this.Id = neteaseSong.id.ToString();
            this.Name = neteaseSong.name;
            this.Artist = neteaseSong.artists.FirstOrDefault().name;
            this.AudioUrl = neteaseSong.mp3Url;
            this.Provider = Mp3Provider.Netease;
            this.Duration = TimeSpan.FromMilliseconds(neteaseSong.duration);
            this.BitRate = neteaseSong.bMusic.bitrate;
            this.ByteSize = neteaseSong.bMusic.size;
        }

        public WebSong(MeileSong meileSong)
        {
            this.Id = meileSong.id.ToString();
            this.Name = meileSong.name;
            this.Artist = meileSong.artistName;
            this.AudioUrl = meileSong.mp3;
            this.Provider = Mp3Provider.Meile;
            this.Duration = TimeSpan.FromSeconds(meileSong.duration);
            if (!string.IsNullOrEmpty(meileSong.normalCover))
            {
                this.ArtworkImage = new Uri(meileSong.normalCover);
            }
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Artist { get; set; }

        public string AudioUrl { get; set; }

        public int BitRate { get; set; }

        public double ByteSize { get; set; }

        public Uri ArtworkImage { get; set; }

        public TimeSpan Duration { get; set; }

        public Mp3Provider Provider { get; set; }

        public bool IsLinkDeath { get; set; }

        public bool IsMatch { get; set; }

        public bool IsBestMatch { get; set; }
    }
}