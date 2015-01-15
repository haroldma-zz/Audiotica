namespace Audiotica.Data.Model.AudioticaCloud
{
    public class LoginResponse : BaseAudioticaResponse
    {
        public string AuthenticationToken { get; set; }

        public AudioticaUser User { get; set; }
    }

    public class AudioticaUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public SubscriptionType Subscription { get; set; }
        public SubscriptionStatus SubscriptionStatus { get; set; }
    }

    public enum SubscriptionType
    {
        None,
        Silver,
        Gold
    }

    public enum SubscriptionStatus
    {
        Unknown,
        Trialing,
        Active,
        PastDue,
        Canceled
    }
}