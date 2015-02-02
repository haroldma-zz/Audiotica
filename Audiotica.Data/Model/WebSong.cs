using System;
using System.Linq;
using Audiotica.Data.Model.SoundCloud;

namespace Audiotica.Data.Model
{
    public class WebSong
    {
        public WebSong()
        {
        }

        public WebSong(Mp3ClanSong mp3ClanSong)
        {
            Id = mp3ClanSong.tid;
            Name = mp3ClanSong.title;
            Artist = mp3ClanSong.artist;
            AudioUrl = mp3ClanSong.url;
            Provider = Mp3Provider.Mp3Clan;
            if (!string.IsNullOrEmpty(mp3ClanSong.duration))
            {
                // format is x:xx, to parse correctly making it 00:x:xx
                var prefix = "0:";

                if (mp3ClanSong.duration.Length <= 3)
                    prefix += "0";

                Duration = TimeSpan.Parse(prefix + mp3ClanSong.duration);
            }
        }

        public WebSong(SoundCloudSong soundCloudSong)
        {
            Id = soundCloudSong.id.ToString();
            Name = soundCloudSong.title;
            AudioUrl = soundCloudSong.stream_url;
            Provider = Mp3Provider.SoundCloud;
            Duration = TimeSpan.FromMilliseconds(soundCloudSong.duration);
            ByteSize = soundCloudSong.original_content_size;
            if (!string.IsNullOrEmpty(soundCloudSong.artwork_url))
                ArtworkImage = new Uri(soundCloudSong.artwork_url);
        }

        public WebSong(NeteaseDetailSong neteaseSong)
        {
            Id = neteaseSong.id.ToString();
            Name = neteaseSong.name;
            Artist = neteaseSong.artists.FirstOrDefault().name;
            AudioUrl = neteaseSong.mp3Url;
            Provider = Mp3Provider.Netease;
            Duration = TimeSpan.FromMilliseconds(neteaseSong.duration);
            BitRate = neteaseSong.bMusic.bitrate;
            ByteSize = neteaseSong.bMusic.size;
        }

        public WebSong(MeileSong meileSong)
        {
            Id = meileSong.id.ToString();
            Name = meileSong.name;
            Artist = meileSong.artistName;
            AudioUrl = meileSong.mp3;
            Provider = Mp3Provider.Meile;
            Duration = TimeSpan.FromSeconds(meileSong.duration);
            if (!string.IsNullOrEmpty(meileSong.normalCover))
                ArtworkImage = new Uri(meileSong.normalCover);
        }

        public string Artist { get; set; }
        public Uri ArtworkImage { get; set; }
        public string AudioUrl { get; set; }
        public int BitRate { get; set; }
        public double ByteSize { get; set; }
        public TimeSpan Duration { get; set; }
        public string Id { get; set; }
        public bool IsBestMatch { get; set; }
        public bool IsLinkDeath { get; set; }
        public bool IsMatch { get; set; }
        public string Name { get; set; }
        public Mp3Provider Provider { get; set; }
    }
}