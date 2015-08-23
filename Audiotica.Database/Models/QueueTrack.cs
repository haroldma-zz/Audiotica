using System;

namespace Audiotica.Database.Models
{
    /// <summary>
    ///     A wrapper for sending a Track to the background player, not used in db.
    /// </summary>
    public class QueueTrack
    {
        public QueueTrack()
        {
        }

        public QueueTrack(Track track)
        {
            Id = Guid.NewGuid().ToString("n");
            Track = track;
        }

        public string Id { get; set; }
        public Track Track { get; set; }
    }
}