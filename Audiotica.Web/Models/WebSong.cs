namespace Audiotica.Web.Models
{
    public class WebSong
    {
        public string Token { get; set; }
        public string Title { get; set; }
        public WebArtist Artist { get; set; }
        public WebAlbum Album { get; set; }
        public int TrackNumber { get; set; }
    }
}