using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Messages
{
    public class TrackChangedMessage
    {
        public string QueueId;

        public TrackChangedMessage()
        {
        }

        public TrackChangedMessage(string queueId)
        {
            QueueId = queueId;
        }
    }
}