namespace Audiotica.Data.Collection.Model
{
    public class RadioStation
    {
        public RadioStation(CloudRadio radio)
        {
            Name = radio.Name;
            CloudId = radio.Id;
            ProviderId = radio.ProviderId;
            GracenoteId = radio.GracenoteId;
            TracksLiked = radio.TracksLiked;
            TracksDisliked = radio.TracksDisliked;
            TracksSkipped = radio.TracksSkipped;
            TracksSaved = radio.TracksSaved;
            TracksPlayed = radio.TracksPlayed;
        }

        public int Id { get; set; }
        public string CloudId { get; set; }
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