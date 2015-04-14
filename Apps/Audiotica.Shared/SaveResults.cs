using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;

namespace Audiotica
{
    public class SaveResults
    {
        public SavingError Error { get; set; }
        public BaseEntry Entry { get; set; }
    }
}