#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

#endregion

namespace Audiotica.Data.Collection
{
    public class BaseEntry : INotifyPropertyChanged
    {
        public BaseEntry()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public DateTime CreatedAt { get; set; }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}