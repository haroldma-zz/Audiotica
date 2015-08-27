using System.Collections.Generic;

namespace Audiotica.Web.Http.Requets.MatchEngine.Netease.Models
{
    public class NeteaseSong
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<NeteaseArtist> Artists { get; set; }
        public NeteaseAlbum Album { get; set; }
        public int Duration { get; set; }
        public int CopyrightId { get; set; }
        public int Status { get; set; }
        public List<object> Alias { get; set; }
        public int Mvid { get; set; }
        public List<string> Lyrics { get; set; }
    }
}