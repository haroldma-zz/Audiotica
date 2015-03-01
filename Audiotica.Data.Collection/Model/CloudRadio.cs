namespace Audiotica.Data.Collection.Model
{
    public class CloudRadio
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ProviderId { get; set; }

        public string GracenoteId { get; set; }
        public int TracksLiked { get; set; }
        public int TracksDisliked { get; set; }
        public int TracksSkipped { get; set; }
        public int TracksPlayed { get; set; }
        public int TracksSaved { get; set; }
    }
}