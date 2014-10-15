using GalaSoft.MvvmLight;

namespace Audiotica.Data.Collection.SqlHelper
{
    public class BaseEntry : ObservableObject
    {
        [SqlProperty(IsPrimaryKey = true)]
        public long Id { get; set; }
    }
}