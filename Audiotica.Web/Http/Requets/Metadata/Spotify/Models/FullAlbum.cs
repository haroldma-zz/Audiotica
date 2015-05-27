﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Audiotica.Data.Spotify.Models
{
    public class FullAlbum : SimpleAlbum
    {
        [JsonProperty("artists")]
        public List<SimpleArtist> Artists { get; set; }
        public SimpleArtist Artist { get { return Artists.FirstOrDefault(); } }
        [JsonProperty("genres")]
        public List<String> Genres { get; set; }
        [JsonProperty("popularity")]
        public int Popularity { get; set; }
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
        [JsonProperty("release_date_precision")]
        public String ReleaseDatePrecision { get; set; }
        [JsonProperty("tracks")]
        public Paging<SimpleTrack> Tracks { get; set; }
    }
}
