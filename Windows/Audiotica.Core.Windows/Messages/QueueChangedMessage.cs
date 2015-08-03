using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Messages
{
    public class QueueChangedMessage
    {
        public QueueItem Queue;

        public QueueChangedMessage()
        {
        }

        public QueueChangedMessage(QueueItem queue)
        {
            Queue = queue;
        }
    }
}