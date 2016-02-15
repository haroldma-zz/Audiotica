using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Messages
{
    public class UpdateUrlMessage
    {
        public long Id { get; set; }

        public string Web { get; set; }

        public string Local { get; set; }

        public TrackType Type { get; set; }

        public UpdateUrlMessage(long id, string web, string local, TrackType type)
        {
            Id = id;
            Web = web;
            Local = local;
            Type = type;
        }
    }
}