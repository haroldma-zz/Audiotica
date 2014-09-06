using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Model
{
    public class MMusic
    {
        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public string extension { get; set; }
        public long dfsId { get; set; }
        public int bitrate { get; set; }
        public int playTime { get; set; }
        public double volumeDelta { get; set; }
    }

    public class LMusic
    {
        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public string extension { get; set; }
        public long dfsId { get; set; }
        public int bitrate { get; set; }
        public int playTime { get; set; }
        public double volumeDelta { get; set; }
    }

    public class BMusic
    {
        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public string extension { get; set; }
        public long dfsId { get; set; }
        public int bitrate { get; set; }
        public int playTime { get; set; }
        public double volumeDelta { get; set; }
    }

    public class NeteaseDetailSong : NeteaseSong
    {
        public bool starred { get; set; }
        public double popularity { get; set; }
        public int starredNum { get; set; }
        public int playedNum { get; set; }
        public int dayPlays { get; set; }
        public int hearTime { get; set; }
        public int position { get; set; }
        public string commentThreadId { get; set; }
        public string copyFrom { get; set; }
        public object hMusic { get; set; }
        public MMusic mMusic { get; set; }
        public LMusic lMusic { get; set; }
        public BMusic bMusic { get; set; }
        public int score { get; set; }
        public object ringtone { get; set; }
        public string mp3Url { get; set; }
    }

    public class NeteaseDetailRoot
    {
        public List<NeteaseDetailSong> songs { get; set; }
        public int code { get; set; }
    }
}
