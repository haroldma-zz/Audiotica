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
            Bar = BottomAppBar;
            BottomAppBar = null;
            CreateCommands();
        }

        private void CreateCommands()
        {
            var acceptButton = new AppBarButton { Icon = new SymbolIcon(Symbol.Accept), Label = "GenericAccept".FromLanguageResource() };
            acceptButton.Click += AcceptButtonOnClick;
            _reorderModeCommands = new List<ICommandBarElement>
            {
                acceptButton
            };

            var deleteButton = new AppBarButton { Icon = new SymbolIcon(Symbol.Delete), Label = "GenericRemove".FromLanguageResource() };
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
                CurtainPrompt.ShowError("SongsNoneSelected".FromLanguageResource());
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
            if (SongList.SelectionMode != ListViewSelectionMode.Multiple &&
                SongList.ReorderMode != ListViewReorderMode.Enabled) return;
            ToSingleMode();
        }

        public override void NavigatedTo(object parameter)
        {
            base.NavigatedTo(parameter);

            if (_originalCommands == null)
                _originalCommands = (Bar as CommandBar).PrimaryCommands.ToList();
            else
                ToSingleMode();

            var id = parameter as long?;

            if (id == null) return;

            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;

            var msg = new GenericMessage<long>((long) id);
            Messenger.Default.Send(msg, "playlist-coll-detail-id");
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void ToMultiMode()
        {
            var bar = Bar as CommandBar;
            SongList.IsItemClickEnabled = false;
            SongList.SelectionMode = ListViewSelectionMode.Multiple;
            bar.PrimaryCommands.Clear();
            bar.PrimaryCommands.AddRange(_selectionModeCommands);
        }

        private void ToSingleMode()
        {
            UiBlockerUtility.Unblock();
            var bar = Bar as CommandBar;
            SongList.IsItemClickEnabled = true;
            SongList.SelectionMode = ListViewSelectionMode.None;
            bar.PrimaryCommands.Clear();
            bar.PrimaryCommands.AddRange(_originalCommands);
        }

        private void SelectAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            UiBlockerUtility.BlockNavigation(false);
            SongList.SelectedIndex = -1;
            ToMultiMode();
        }

        private void ReorderAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var bar = Bar as CommandBar;
            SongList.IsItemClickEnabled = false;
            SongList.ReorderMode = ListViewReorderMode.Enabled;
            bar.PrimaryCommands.Clear();
            bar.PrimaryCommands.AddRange(_reorderModeCommands);
        }

    }
}