using GalaSoft.MvvmLight;
using SQLite;

namespace Audiotica.Data.Collection.SqlHelper
{
    public class BaseEntry : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}