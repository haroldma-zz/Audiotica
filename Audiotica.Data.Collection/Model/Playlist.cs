#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Audiotica.Data.Collection.SqlHelper;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class Playlist : BaseEntry
    {
        private readonly Dictionary<long, PlaylistSong> _lookupMap = new Dictionary<long, PlaylistSong>();

        public string Name { get; set; }

        public ObservableCollection<PlaylistSong> Songs { get; set; }

        public void Load(List<PlaylistSong> songs)
        {
            PlaylistSong head = null;
            Songs = new ObservableCollection<PlaylistSong>();

            foreach (var playlistSong in songs)
            {
                _lookupMap.Add(playlistSong.Id, playlistSong);
                if (playlistSong.PrevId == 0)
                    head = playlistSong;
            }

            if (head == null)
                return;

            for (var i = 0; i < songs.Count; i++)
            {
                Songs.Add(head);

                if (head.NextId != 0)
                    head = _lookupMap[head.NextId];
            }
        }
    }
}