using System.Collections.Generic;

namespace Audiotica.Web.Models.Netease
{
    public class NeteaseArtist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public object PicUrl { get; set; }
        public List<object> Alias { get; set; }
        public int AlbumSize { get; set; }
        public int PicId { get; set; }
    }
}
