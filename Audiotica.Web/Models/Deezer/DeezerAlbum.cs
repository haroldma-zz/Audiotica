namespace Audiotica.Web.Models.Deezer
{
    public class DeezerAlbum
    {
        public string BigCover => Cover.Contains("?") ? Cover : Cover += "?size=big";
        public int Id { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }
        public string Tracklist { get; set; }
        public string Type { get; set; }
        public DeezerArtist Artist { get; set; }
    }
}