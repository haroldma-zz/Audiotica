#region

using Audiotica.Controls;
using Audiotica.Data.Collection.Model;

using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

using Audiotica.View;

#endregion

namespace Audiotica
{
    public sealed partial class SongViewer
    {
        private bool _playlistMode;

        private bool _queueMode;

        private Song _song;

        public SongViewer()
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Drop all refrences to the data item 
        /// </summary> 
        public void ClearData()
        {
            _queueMode = true;
            _playlistMode = true;
            _song = null;
            SongNameTextBlock.ClearValue(TextBlock.TextProperty);
            ArtistAlbumNameTextBlock.ClearValue(TextBlock.TextProperty);
            DataContext = null;
        }

        public void ShowDownload()
        {
            if (_queueMode)
            {
                return;
            }

            DownloadOptionGrid.Visibility = Visibility.Visible;
            DownloadProgressGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// This method visualizes the placeholder state of the data item. When
        /// showing a placehlder, we set the opacity of other elements to zero
        /// so that stale data is not visible to the end user.
        /// </summary>
        /// <param name="song">The song.</param>
        /// <param name="queueMode">if set to <c>true</c> [queue mode].</param>
        /// <param name="playlistMode">if set to <c>true</c> [playlist mode].</param>
        public void ShowPlaceholder(Song song, bool queueMode = false, bool playlistMode = false)
        {
            _playlistMode = playlistMode;
            _queueMode = queueMode;
            DownloadOptionGrid.Visibility = Visibility.Collapsed;
            DownloadProgressGrid.Visibility = Visibility.Collapsed;

            DataContext = song;
            _song = song;
            SongNameTextBlock.Opacity = 0;
            ArtistAlbumNameTextBlock.Opacity = 0;
        }

        /// <summary>
        /// Visualize artist information by updating the correct TextBlock and
        /// setting Opacity to 1.
        /// </summary>
        /// <param name="withAlbumName">if set to <c>true</c> [with album name].</param>
        public void ShowRest(bool withAlbumName = true)
        {
            var albumName = withAlbumName ? _song.Album.Name : string.Empty;

            if (!string.IsNullOrEmpty(albumName))
            {
                albumName = " - " + albumName;
            }

            ArtistAlbumNameTextBlock.Text = _song.ArtistName + albumName;
            ArtistAlbumNameTextBlock.Opacity = 1;

            ShowDownload();
        }

        /// <summary> 
        /// Visualize the Name by updating the TextBlock for Name and setting Opacity 
        /// to 1. 
        /// </summary> 
        public void ShowTitle()
        {
            SongNameTextBlock.Text = _song.Name;
            SongNameTextBlock.Opacity = 1;
        }

        #region Song user events

        private void Song_OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (!_queueMode && !_playlistMode && _song.SongState != SongState.Matching)
            {
                FlyoutBase.ShowAttachedFlyout(RootGrid);
            }
        }

        private void AddToMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            UiBlockerUtility.BlockNavigation();
            var picker = new PlaylistPicker(_song) { Action = async playlist =>
            {
                App.SupressBackEvent -= AppOnSupressBackEvent;
                UiBlockerUtility.Unblock();
                ModalSheetUtility.Hide();
                await App.Locator.CollectionService.AddToPlaylistAsync(playlist, _song).ConfigureAwait(false);
            } };

            App.SupressBackEvent += AppOnSupressBackEvent;
            ModalSheetUtility.Show(picker);
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await CollectionHelper.AddToQueueAsync(_song);
        }

        private void AppOnSupressBackEvent(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            App.SupressBackEvent -= AppOnSupressBackEvent;
            UiBlockerUtility.Unblock();
            ModalSheetUtility.Hide();
        }

        private void DeleteMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.DeleteClickCommand.Execute(_song);
        }

        private void DownloadButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.DownloadClickCommand.Execute(_song);
        }

        private void CancelButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Locator.Collection.Commands.CancelClickCommand.Execute(_song);
        }

        #endregion

        private void MenuFlyout_Opened(object sender, object e)
        {
            AddToOption.IsEnabled = _song.IsMatched;
        }

        private void ManualMatchButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<ManualMatchPage, ZoomInTransition>(_song);
        }
    }
}