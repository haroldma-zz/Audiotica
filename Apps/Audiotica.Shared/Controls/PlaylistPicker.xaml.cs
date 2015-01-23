#region

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Audiotica.Data.Collection;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.Controls
{
    public sealed partial class PlaylistPicker : IModalSheetPageWithAction<Playlist>
    {
        public PlaylistPicker(BaseEntry song)
        {
            InitializeComponent();
            var playlists =
                App.Locator.CollectionService.Playlists.Where(p => p.Songs.Count(pp => pp.SongId == song.Id) == 0);
            PlaylistPickerListView.ItemsSource = playlists;
        }

        public PlaylistPicker(IReadOnlyCollection<Song> songs)
        {
            InitializeComponent();

            var playlists =
                App.Locator.CollectionService.Playlists.Where(p => p.Songs.Count(pp =>
                    songs.Count(t => t.Id == pp.SongId) == songs.Count) == 0);
            PlaylistPickerListView.ItemsSource = playlists;
        }

        public Popup Popup { get; private set; }

        public void OnOpened(Popup popup)
        {
            Popup = popup;
            PlaylistPickerListView.ItemClick += PlaylistPickerListViewOnItemClick;
        }

        public void OnClosed()
        {
            PlaylistPickerListView.ItemClick -= PlaylistPickerListViewOnItemClick;
        }

        public Action<Playlist> Action { get; set; }

        private void PlaylistPickerListViewOnItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            if (Action != null)
                Action(itemClickEventArgs.ClickedItem as Playlist);
        }
    }
}