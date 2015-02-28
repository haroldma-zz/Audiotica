#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Phone.UI.Input;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Audiotica.Data.Collection.Model;
using Audiotica.ViewModel;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionAlbumPage
    {
        private readonly List<ICommandBarElement> _selectionModeCommands;
        private readonly List<ICommandBarElement> _selectionSecondaryModeCommands;
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;

        public CollectionAlbumPage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;

            var playButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Play),
                Label = "play"
            };
            playButton.Click += async (o, p) =>
            {
                var songs = SongList.SelectedItems.Select(m => m as Song).ToList();
                if (songs.Count == 0) return;

                SongList.SelectionMode = ListViewSelectionMode.None;
                await CollectionHelper.PlaySongsAsync(songs, forceClear: true);
            };
            var enqueueButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "enqueue"
            };
            enqueueButton.Click += async (o, p) =>
            {
                var songs = SongList.SelectedItems.Select(m => m as Song).ToList();
                if (songs.Count == 0) return;

                SongList.SelectionMode = ListViewSelectionMode.None;
                await CollectionHelper.AddToQueueAsync(songs);
            };

            _selectionModeCommands = new List<ICommandBarElement>
            {
                enqueueButton,
                playButton
            };

            var addToButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Play),
                Label = "add to playlist..."
            };
            addToButton.Click += (o, p) =>
            {
                var songs = SongList.SelectedItems.Select(m => m as Song).ToList();
                if (songs.Count == 0) return;

                SongList.SelectionMode = ListViewSelectionMode.None;
                CollectionHelper.AddToPlaylistDialog(songs);
            };
            var deleteButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Add),
                Label = "delete"
            };
            deleteButton.Click += async (o, p) =>
            {
                var tasks =
                    SongList.SelectedItems.Select(m => CollectionHelper.DeleteEntryAsync(m as Song, false)).ToList();
                if (tasks.Count == 0) return;

                SongList.SelectionMode = ListViewSelectionMode.None;
                await Task.WhenAll(tasks);
            };
            _selectionSecondaryModeCommands = new List<ICommandBarElement>
            {
                addToButton,
                deleteButton
            };
        }

        private CollectionAlbumViewModel Vm
        {
            get { return DataContext as CollectionAlbumViewModel; }
        }

        /// <summary>
        ///     Managing delegate creation to ensure we instantiate a single instance for
        ///     optimal performance.
        /// </summary>
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> ContainerContentChangingDelegate
        {
            get { return _delegate ?? (_delegate = ItemListView_ContainerContentChanging); }
        }

        public override void NavigatedTo(NavigationMode mode, object e)
        {
            base.NavigatedTo(mode, e);
            var id = e as int?;

            if (id == null) return;

            var msg = new GenericMessage<int>((int) id);
            Messenger.Default.Send(msg, "album-coll-detail-id");

            ToggleAppBarButton(SecondaryTile.Exists("album." + Vm.Album.Id));
        }

        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);

            if (mode != NavigationMode.Back) return;

            var vm = DataContext as CollectionAlbumViewModel;

            vm.Album = null;
        }

        private void ToggleAppBarButton(bool isPinned)
        {
            if (!isPinned)
            {
                PinUnpinAppBarButton.Label = "Pin";
                PinUnpinAppBarButton.Icon = new SymbolIcon(Symbol.Pin);
                Bar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            }
            else
            {
                PinUnpinAppBarButton.Label = "Unpin";
                PinUnpinAppBarButton.Icon = new SymbolIcon(Symbol.UnPin);
                Bar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            }
        }

        private async void PinUnpinAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleAppBarButton(await CollectionHelper.PinToggleAsync(Vm.Album));
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
                        songViewer.ShowPlaceholder(args.Item as Song);
                        args.RegisterUpdateCallback(ContainerContentChangingDelegate);
                        break;
                    case 1:
                        songViewer.ShowTitle();
                        args.RegisterUpdateCallback(ContainerContentChangingDelegate);
                        break;
                    case 2:
                        songViewer.ShowRest(false);
                        break;
                }

            // For imporved performance, set Handled to true since app is visualizing the data item 
            args.Handled = true;
        }

        private void MultiSelectListView_SelectionModeChanged(object sender, RoutedEventArgs e)
        {
            var bar = Bar as CommandBar;
            if (SongList.SelectionMode == ListViewSelectionMode.Multiple)
            {
                HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
                UiBlockerUtility.BlockNavigation(false);
                SongList.SelectedIndex = -1;

                SongList.IsItemClickEnabled = false;

                AppBarHelper.SaveState(bar);
                AppBarHelper.SwitchState(bar, _selectionModeCommands, _selectionSecondaryModeCommands);
            }
            else if (!SongList.IsItemClickEnabled)
            {
                HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
                UiBlockerUtility.Unblock();
                SongList.IsItemClickEnabled = true;

                AppBarHelper.RestorePreviousState(bar);
            }
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            SongList.SelectionMode = ListViewSelectionMode.None;
        }
    }
}