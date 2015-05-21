namespace Audiotica.Web.Models.Netease
{
    public class NeteaseDetailSong : NeteaseSong
    {
        public bool Starred { get; set; }
        public double Popularity { get; set; }
        public int StarredNum { get; set; }
        public int PlayedNum { get; set; }
        public int DayPlays { get; set; }
        public int HearTime { get; set; }
        public int Position { get; set; }
        public string CommentThreadId { get; set; }
        public string CopyFrom { get; set; }
        public NeteaseMusicDetails HMusic { get; set; }
        public NeteaseMusicDetails MMusic { get; set; }
        public NeteaseMusicDetails LMusic { get; set; }
        public NeteaseMusicDetails BMusic { get; set; }
        public int Score { get; set; }
        public object Ringtone { get; set; }
        public string Mp3Url { get; set; }

        public class NeteaseMusicDetails
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public int Size { get; set; }
            public string Extension { get; set; }
            public long DfsId { get; set; }
            public int Bitrate { get; set; }
            public int PlayTime { get; set; }
            public double VolumeDelta { get; set; }
        }
    }
}