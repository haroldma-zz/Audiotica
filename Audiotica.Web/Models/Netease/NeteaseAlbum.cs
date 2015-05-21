namespace Audiotica.Web.Models.Netease
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