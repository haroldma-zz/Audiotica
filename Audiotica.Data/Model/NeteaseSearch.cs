using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model
{
    public class NeteaseArtist
    {
        public int id { get; set; }
        public string name { get; set; }
        public object picUrl { get; set; }
        public List<object> alias { get; set; }
        public int albumSize { get; set; }
        public int picId { get; set; }
    }

    public class NeteaseArtist2
    {
        public int id { get; set; }
        public string name { get; set; }
        public object picUrl { get; set; }
        public List<object> alias { get; set; }
        public int albumSize { get; set; }
        public int picId { get; set; }
    }

    public class NeteaseAlbum
    {
        public int id { get; set; }
        public string name { get; set; }
        public NeteaseArtist2 artist { get; set; }
        public object publishTime { get; set; }
        public int size { get; set; }
        public int copyrightId { get; set; }
        public int status { get; set; }
    }

    public class NeteaseSong
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<NeteaseArtist> artists { get; set; }
        public NeteaseAlbum album { get; set; }
        public int duration { get; set; }
        public int copyrightId { get; set; }
        public int status { get; set; }
        public List<object> alias { get; set; }
        public int mvid { get; set; }
        public List<string> lyrics { get; set; }
    }

    public class NeteaseResult
    {
        public List<NeteaseSong> songs { get; set; }
    }

    public class NeteaseRoot
    {
        public NeteaseResult result { get; set; }
    }
}
