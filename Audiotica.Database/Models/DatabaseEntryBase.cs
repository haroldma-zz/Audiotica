using System;
using Audiotica.Core.Common;
using SQLite.Net.Attributes;

namespace Audiotica.Database.Models
{
    public abstract class DatabaseEntryBase : ObservableObject
    {
        protected DatabaseEntryBase()
        {
            CreatedAt = DateTime.UtcNow;
        }

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}