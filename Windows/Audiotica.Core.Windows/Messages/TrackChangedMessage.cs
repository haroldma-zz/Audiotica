using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Messages
{
    public class TrackChangedMessage
    {
        public int TrackId;

        public TrackChangedMessage()
        {
        }

        public TrackChangedMessage(int trackId)
        {
            TrackId = trackId;
        }
    }
}