namespace Audiotica.Data.Model
{
    public class AudioticaStripeCard
    {
        public string Cvc { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
    }
}