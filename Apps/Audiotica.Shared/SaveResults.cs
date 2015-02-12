using Audiotica.Data.Collection.Model;

namespace Audiotica
{
    public class SaveResults
    {
        public SavingError Error { get; set; }
        public Song Song { get; set; }
    }
}