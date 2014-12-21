#region

using System;

#endregion

namespace Audiotica.Data.Collection.SqlHelper
{
    public class SqlProperty : Attribute
    {
        public bool IsNull { get; set; }
        public bool IsPrimaryKey { get; set; }
        public Type ReferenceTo { get; set; }
    }
}