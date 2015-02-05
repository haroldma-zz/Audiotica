using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Audiotica.Data.Collection.Model
{
    public class CloudArtist : Artist
    {
        public new string Id { get; set; }

        [JsonIgnore]
        public new List<CloudSong> Songs { get; set; }

        [JsonIgnore]
        public new List<CloudAlbum> Albums { get; set; }
    }
}