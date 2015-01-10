namespace Audiotica.Data.Model.Deezer
{
    public class DeezerArtist
    {
        public int id { get; set; }
        public string name { get; set; }
        public string tracklist { get; set; }
        public string type { get; set; }
        public string picture { get; set; }
        public string bigPicture
        {
            get { return picture += "?size=big"; }
        }
    }
}