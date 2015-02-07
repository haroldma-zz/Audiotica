namespace Audiotica.Data.Model.Xbox
{
    public class Contributor
    {
        /// <summary>
        /// The type of contribution, such as "Main" or "Featured".
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// The contributing artist.
        /// </summary>
        public XboxArtist Artist { get; set; }
    }
}