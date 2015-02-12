using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Data.Collection.Model
{
    public class CloudSong : Song
    {
        public new string Id { get; set; }
        public new string AlbumId { get; set; }
        public new string ArtistId { get; set; }

        [JsonIgnore]
        public new CloudArtist Artist { get; set; }
        [JsonIgnore]
        public new CloudAlbum Album { get; set; }
    }
}