namespace Audiotica.Web.Http.Requets.MatchEngine.Netease.Models
{
    public class NeteaseAlbum
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public NeteaseArtist Artist { get; set; }
        public object PublishTime { get; set; }
        public int Size { get; set; }
        public int CopyrightId { get; set; }
        public int Status { get; set; }
    }
}