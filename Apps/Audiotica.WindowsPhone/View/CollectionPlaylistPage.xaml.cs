#region

using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Common;
using Audiotica.Data.Collection.Model;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using QKit;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionPlaylistPage
    {
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;
        private List<ICommandBarElement> _reorderModeCommands;
        private List<ICommandBarElement> _selectionModeCommands;

        public CollectionPlaylistPage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
            CreateCommands();
        }

        private void CreateCommands()
        {
            var acceptButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Accept),
                Label = "GenericAccept".FromLanguageResource()
            };
            acceptButton.Click += AcceptButtonOnClick;
            _reorderModeCommands = new List<ICommandBarElement>
            {
                acceptButton
            };

            var deleteButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Label = "GenericRemove".FromLanguageResource()
            };
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
            ToSingleMode();
        }

        public override void NavigatedTo(object parameter)
        {
            base.NavigatedTo(parameter);

            var id = parameter as int?;

            if (id == null) return;

            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;

            var msg = new GenericMessage<int>((int) id);
            Messenger.Default.Send(msg, "playlist-coll-detail-id");
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void ToMultiMode()
        {
            UiBlockerUtility.BlockNavigation(false);
            SongList.SelectedIndex = -1;

            var bar = Bar as CommandBar;
            SongList.IsItemClickEnabled = false;

// ReSharper disable once RedundantCheckBeforeAssignment
            if (SongList.SelectionMode != ListViewSelectionMode.Multiple)
                SongList.SelectionMode = ListViewSelectionMode.Multiple;

            AppBarHelper.SaveState(bar);
            AppBarHelper.SwitchState(bar, _selectionModeCommands);
        }

        private void ToSingleMode()
        {
            UiBlockerUtility.Unblock();
            var bar = Bar as CommandBar;
            SongList.IsItemClickEnabled = true;
            SongList.SelectionMode = ListViewSelectionMode.None;
            AppBarHelper.RestorePreviousState(bar);
        }

        private void SelectAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ToMultiMode();
        }

        private void ReorderAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var bar = Bar as CommandBar;
            SongList.IsItemClickEnabled = false;
            SongList.ReorderMode = ListViewReorderMode.Enabled;
            AppBarHelper.SaveState(bar);
            AppBarHelper.SwitchState(bar, _reorderModeCommands);
        }


        private void ItemListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var songViewer = args.ItemContainer.ContentTemplateRoot as SongViewer;

            if (songViewer == null)
                return;

            if (args.InRecycleQueue)
            {
                songViewer.ClearData();
            }
            else
                switch (args.Phase)
                {
                    case 0:
                        songViewer.ShowPlaceholder((args.Item as PlaylistSong).Song, playlistMode: true);
                        args.RegisterUpdateCallback(ContainerContentChangingDelegate);
                        break;
                    case 1:
                        songViewer.ShowTitle();
                        args.RegisterUpdateCallback(ContainerContentChangingDelegate);
                        break;
                    case 2:
                        songViewer.ShowRest();
                        break;
                }

            // For imporved performance, set Handled to true since app is visualizing the data item 
            args.Handled = true;
        }

        /// <summary>
        ///     Managing delegate creation to ensure we instantiate a single instance for
        ///     optimal performance.
        /// </summary>
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> ContainerContentChangingDelegate
        {
            get { return _delegate ?? (_delegate = ItemListView_ContainerContentChanging); }
        }

        private void SongList_SelectionModeChanged(object sender, RoutedEventArgs e)
        {
            var mode = (sender as MultiSelectListView).SelectionMode;

            if (mode == ListViewSelectionMode.Multiple)
                ToMultiMode();
        }
    }
}