namespace Audiotica.Data.Model.Deezer
{
    public class DeezerAlbum
    {
        public int id { get; set; }
        public string title { get; set; }
        public string cover { get; set; }

        public string bigCover
        {
            get { return cover += "?size=big"; }
        }

        public string tracklist { get; set; }
        public string type { get; set; }

        public DeezerArtist artist { get; set; }
    }
}