namespace Audiotica.Data.Collection.SqlHelper
{
    public class BaseEntry
    {
        [SqlProperty(IsPrimaryKey = true)]
        public long Id { get; set; }
    }
}