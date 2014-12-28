using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Audiotica.Data.Model.Spotify.Models
{
    public class FullTrack : SimpleTrack
    {
        /// <summary>
        /// Simple-Album object of the track @<see cref="Paging"/>
        /// </summary>
        [JsonProperty("album")]
        public SimpleAlbum Album { get; set; }
        [JsonProperty("artists")]
        public List<SimpleArtist> Artists { get; set; }
        public new SimpleArtist Artist { get { return Artists.FirstOrDefault(); } }
        
        [JsonProperty("external_ids")]
        public Dictionary<String, String> ExternalIds { get; set; }
        [JsonProperty("popularity")]
        public int Popularity { get; set; }
    }
}
