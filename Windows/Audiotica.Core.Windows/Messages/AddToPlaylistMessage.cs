using System.Collections.Generic;
using Audiotica.Database.Models;
using Newtonsoft.Json;

namespace Audiotica.Core.Windows.Messages
{
    public class AddToPlaylistMessage
    {
        public AddToPlaylistMessage()
        {    
        }

        public AddToPlaylistMessage(QueueTrack track, int position)
        {
            Tracks = new List<QueueTrack> {track};
            Position = position;
        }

        public AddToPlaylistMessage(List<QueueTrack> tracks, int position)
        {
            Tracks = tracks;
            Position = position;
        }

        public List<QueueTrack> Tracks { get; set; }
        
        public int Position { get; set; }
    }
}