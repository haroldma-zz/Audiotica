using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Messages
{
    public class AddToPlaylistMessage
    {
        public AddToPlaylistMessage(QueueTrack track, int position)
        {
            Track = track;
            Position = position;
        }

        public QueueTrack Track { get; set; }
        public int Position { get; set; }
    }
}