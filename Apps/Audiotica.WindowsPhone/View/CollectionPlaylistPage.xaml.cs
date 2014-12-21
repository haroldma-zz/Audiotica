#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Common;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionPlaylistPage
    {
        private List<ICommandBarElement> _reorderModeCommands;
        private List<ICommandBarElement> _selectionModeCommands;
        private List<ICommandBarElement> _originalCommands;

        public CollectionPlaylistPage()
        {
            InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            CreateCommands();
        }

        private void CreateCommands()
        {
            var acceptButton = new AppBarButton {Icon = new SymbolIcon(Symbol.Accept), Label = "Accept"};
            acceptButton.Click += AcceptButtonOnClick;
            _reorderModeCommands = new List<ICommandBarElement>
            {
                acceptButton
            };

            var deleteButton = new AppBarButton {Icon = new SymbolIcon(Symbol.Delete), Label = "Remove"};
            deleteButton.Click += DeleteButtonOnClick;
            _selectionModeCommands = new List<ICommandBarElement>
            {
                deleteButton
            };
        }

        private void AcceptButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            SongList.ReorderMode = ListViewReorderMode.Disabled;
            ToSingleMode();
        }

        private async void DeleteButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var button = sender as AppBarButton;
            button.IsEnabled = false;
            SongList.IsEnabled = false;

            var songs = SongList.SelectedItems.ToList();
            if (songs.Count == 0)
            {
                CurtainToast.ShowError("Try selecting some songs");
            }
            else
            {
                foreach (var song in songs)
                {
                    var playlist = (DataContext as CollectionPlaylistViewModel).Playlist;
                    await App.Locator.CollectionService.DeleteFromPlaylistAsync(playlist, song as PlaylistSong);
                }
                ToSingleMode();
            }

            button.IsEnabled = true;
            SongList.IsEnabled = true;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            if (SongList.SelectionMode == ListViewSelectionMode.Multiple)
            {
                e.Handled = true;
                ToSingleMode();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_originalCommands == null)
                _originalCommands = (BottomAppBar as CommandBar).PrimaryCommands.ToList();

            var id = e.Parameter as long?;

            if (id == null) return;

            var msg = new GenericMessage<long>((long) id);
            Messenger.Default.Send(msg, "playlist-coll-detail-id");
        }

        private void ToMultiMode()
        {
            var bar = BottomAppBar as CommandBar;
            SongList.IsItemClickEnabled = false;
            SongList.SelectionMode = ListViewSelectionMode.Multiple;
            bar.PrimaryCommands.Clear();
            bar.PrimaryCommands.AddRange(_selectionModeCommands);
        }

        private void ToSingleMode()
        {
            var bar = BottomAppBar as CommandBar;
            SongList.IsItemClickEnabled = true;
            SongList.SelectionMode = ListViewSelectionMode.None;
            bar.PrimaryCommands.Clear();
            bar.PrimaryCommands.AddRange(_originalCommands);
        }

        private void SelectAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SongList.SelectedIndex = -1;
            ToMultiMode();
        }

        private void ReorderAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var bar = BottomAppBar as CommandBar;
            SongList.IsItemClickEnabled = false;
            SongList.ReorderMode = ListViewReorderMode.Enabled;
            bar.PrimaryCommands.Clear();
            bar.PrimaryCommands.AddRange(_reorderModeCommands);
        }

    }
}