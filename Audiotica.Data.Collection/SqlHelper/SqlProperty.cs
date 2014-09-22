using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Data.Collection.SqlHelper
{
    public class SqlProperty : Attribute
    {
        public bool IsPrimaryKey { get; set; }
        public Type ReferenceTo { get; set; }
    }
}
