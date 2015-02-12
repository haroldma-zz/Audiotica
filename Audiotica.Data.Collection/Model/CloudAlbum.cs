using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Data.Collection.Model
{
    public class CloudAlbum : Album
    {
        public new string Id { get; set; }
        public new string PrimaryArtistId { get; set; }
        [JsonIgnore]
        public new List<CloudSong> Songs { get; set; }

        [JsonIgnore]
        public new CloudArtist PrimaryArtist { get; set; }
    }
}