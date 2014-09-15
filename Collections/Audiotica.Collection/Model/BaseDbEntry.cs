using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Audiotica.Collection.Model
{
    public class BaseDbEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
