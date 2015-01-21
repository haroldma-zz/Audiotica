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
using IF.Lastfm.Core.Objects;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionArtistPage
    {
        private readonly List<ICommandBarElement> _selectionModeCommands;
        private readonly List<ICommandBarElement> _selectionSecondaryModeCommands;
        private readonly PivotItem _bioPivotItem;
        private readonly PivotItem _similarPivotItem;
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;

        public CollectionArtistPage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
            _bioPivotItem = BioPivot;
            _similarPivotItem = SimilarPivot;

            var playButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Play),
                Label = "play",
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
                var tasks = SongList.SelectedItems.Select(m => CollectionHelper.DeleteEntryAsync(m as Song, false)).ToList();
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

        private CollectionArtistViewModel Vm
        {
            get { return DataContext as CollectionArtistViewModel; }
        }

        /// <summary>
        ///     Managing delegate creation to ensure we instantiate a single instance for
        ///     optimal performance.
        /// </summary>
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> ContainerContentChangingDelegate
        {
            get { return _delegate ?? (_delegate = ItemListView_ContainerContentChanging); }
        }

        public override void NavigatedTo(object e)
        {
            base.NavigatedTo(e);
            var id = e as int?;
            if (id == null) return;

            Messenger.Default.Send((int) id, "artist-coll-detail-id");
            Messenger.Default.Register<bool>(this, "artist-coll-bio", BioUpdate);
            Messenger.Default.Register<bool>(this, "artist-coll-sim", SimUpdate);
            Messenger.Default.Register<bool>(this, "artist-coll-pin", ToggleAppBarButton);

            ToggleAppBarButton(SecondaryTile.Exists("artist." + Vm.Artist.Id));
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


        public override void NavigatedFrom(NavigationMode mode)
        {
            base.NavigatedFrom(mode);
            Messenger.Default.Unregister<bool>(this, "artist-coll-bio", BioUpdate);
            Messenger.Default.Unregister<bool>(this, "artist-coll-sim", SimUpdate);
        }

        private void SimUpdate(bool isVisible)
        {
            if (isVisible)
                if (!ArtistPivot.Items.Contains(_similarPivotItem))
                    ArtistPivot.Items.Add(_similarPivotItem);
                else
                    ArtistPivot.Items.Remove(SimilarPivot);
        }

        private void BioUpdate(bool isVisible)
        {
            if (isVisible)
                if (!ArtistPivot.Items.Contains(_bioPivotItem))
                    ArtistPivot.Items.Add(_bioPivotItem);
                else
                {
                    ArtistPivot.Items.Remove(BioPivot);
                }
        }

        private void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as Album;
            App.Navigator.GoTo<CollectionAlbumPage, ZoomInTransition>(album.Id);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as LastArtist;
            App.Navigator.GoTo<SpotifyArtistPage, ZoomInTransition>("name." + artist.Name);
        }

        private async void PinUnpinAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleAppBarButton(await CollectionHelper.PinToggleAsync(Vm.Artist));
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
                        songViewer.ShowRest();
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
            else
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

        private void ArtistPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SongList.SelectionMode = ListViewSelectionMode.None;
        }
    }
}