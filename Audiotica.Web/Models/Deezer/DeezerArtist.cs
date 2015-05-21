namespace Audiotica.Web.Models.Deezer
{
    public class DeezerArtist
    {
        public string BigPicture => Picture.Contains("?") ? Picture : Picture += "?size=big";
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tracklist { get; set; }
        public string Type { get; set; }
        public string Picture { get; set; }
    }
}