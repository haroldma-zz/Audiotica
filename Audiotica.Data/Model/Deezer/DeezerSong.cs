namespace Audiotica.Data.Model.Deezer
{
    public class DeezerSong
    {
        public int id { get; set; }
        public bool readable { get; set; }
        public string title { get; set; }
        public string link { get; set; }
        public int duration { get; set; }
        public int rank { get; set; }
        public bool explicit_lyrics { get; set; }
        public string preview { get; set; }
        public DeezerArtist artist { get; set; }
        public DeezerAlbum album { get; set; }
        public string type { get; set; }
    }
}
