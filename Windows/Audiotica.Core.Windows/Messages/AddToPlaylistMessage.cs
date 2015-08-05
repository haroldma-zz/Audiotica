using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Messages
{
    public class AddToPlaylistMessage
    {
        public QueueTrack Track { get; set; }

        public AddToPlaylistMessage(QueueTrack track)
        {
            Track = track;
        }
    }
}