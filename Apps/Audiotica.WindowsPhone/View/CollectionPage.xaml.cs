#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Utils;
using Audiotica.Core.Utils.Interfaces;
using Audiotica.Core.WinRt;
using Audiotica.Core.WinRt.Utilities;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionPage
    {
        private readonly List<ICommandBarElement> _selectionModeCommands;
        private readonly List<ICommandBarElement> _selectionSecondaryModeCommands;
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;
        private bool _loaded;

        public CollectionPage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
            Loaded += (sender, args) => LoadWallpaperArt();

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

        /// <summary>
        ///     Managing delegate creation to ensure we instantiate a single instance for
        ///     optimal performance.
        /// </summary>
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> ContainerContentChangingDelegate
        {
            get { return _delegate ?? (_delegate = ItemListView_ContainerContentChanging); }
        }

        public override void NavigatedTo(NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);

            LoadWallpaperArt();

            if (parameter == null) return;

            var pivotIndex = (int) parameter;
            CollectionPivot.SelectedIndex = pivotIndex;
        }

        private async void LoadWallpaperArt()
        {
            if (_loaded ||
                !App.Locator.AppSettingsHelper.Read("WallpaperArt", true, SettingsStrategy.Roaming)) return;

            var wait = App.Locator.AppSettingsHelper.Read<int>("WallpaperDayWait");
            var created = App.Locator.AppSettingsHelper.ReadJsonAs<DateTime>("WallpaperCreated");

            // Set the image brush
            var imageBrush = new ImageBrush { Opacity = .5 };
            BlurImageTool.SetBlurPercent(imageBrush, .8);
            LayoutGrid.Background = imageBrush;

            if (created != DateTime.MinValue)
            {
                // Not the first time, so there must already be one created
                BlurImageTool.SetSource(imageBrush, "ms-appdata:/local/wallpaper.jpg");
            }

            // Once a week remake the wallpaper
            if ((DateTime.Now - created).TotalDays > wait)
            {
                var albums =
                    App.Locator.CollectionService.Albums.ToList()
                        .Where(p => p.Artwork != AppConstant.MissingArtworkImage)
                        .ToList();

                var albumCount = albums.Count;

                if (albumCount < 10) return;


                var h = Window.Current.Bounds.Height;
                var rows = (int) Math.Ceiling(h/(ActualWidth/5));
                const int collumns = 5;

                var albumSize = (int) Window.Current.Bounds.Width/collumns;

                var numImages = rows*5;
                var imagesNeeded = numImages - albumCount;

                var shuffle = await Task.FromResult(albums
                    .Shuffle()
                    .Take(numImages > albumCount ? albumCount : numImages)
                    .ToList());

                if (imagesNeeded > 0)
                {
                    var repeatList = new List<Album>();

                    while (imagesNeeded > 0)
                    {
                        var takeAmmount = imagesNeeded > albumCount ? albumCount : imagesNeeded;

                        await Task.Run(() => repeatList.AddRange(shuffle.Shuffle().Take(takeAmmount)));

                        imagesNeeded -= shuffle.Count;
                    }

                    shuffle.AddRange(repeatList);
                }

                // Initialize an empty WriteableBitmap.
                var destination = new WriteableBitmap((int) Window.Current.Bounds.Width,
                    (int) Window.Current.Bounds.Height);
                var col = 0; // Current Column Position
                var row = 0; // Current Row Position
                destination.Clear(Colors.Black); // Set the background color of the image to black

                // will be copied
                foreach (var artworkPath in shuffle.Select(album => string.Format(AppConstant.ArtworkPath, album.Id)))
                {
                    var file = await WinRtStorageHelper.GetFileAsync(artworkPath);

                    // Read the image file into a RandomAccessStream
                    using (var fileStream = await file.OpenReadAsync())
                    {
                        // Now that you have the raw bytes, create a Image Decoder
                        var decoder = await BitmapDecoder.CreateAsync(fileStream);

                        // Get the first frame from the decoder because we are picking an image
                        var frame = await decoder.GetFrameAsync(0);

                        // Convert the frame into pixels
                        var pixelProvider = await frame.GetPixelDataAsync();

                        // Convert pixels into byte array
                        var srcPixels = pixelProvider.DetachPixelData();
                        var wid = (int) frame.PixelWidth;
                        var hgt = (int) frame.PixelHeight;
                        // Create an in memory WriteableBitmap of the same size
                        var bitmap = new WriteableBitmap(wid, hgt); // Temporary bitmap into which the source

                        using (var pixelStream = bitmap.PixelBuffer.AsStream())
                        {
                            pixelStream.Seek(0, SeekOrigin.Begin);
                            // Push the pixels from the original file into the in-memory bitmap
                            await pixelStream.WriteAsync(srcPixels, 0, srcPixels.Length);
                            bitmap.Invalidate();

                            // Resize the in-memory bitmap and Blit (paste) it at the correct tile
                            // position (row, col)
                            destination.Blit(new Rect(col*albumSize, row*albumSize, albumSize, albumSize),
                                bitmap.Resize(albumSize, albumSize, WriteableBitmapExtensions.Interpolation.Bilinear),
                                new Rect(0, 0, albumSize, albumSize));
                            col++;
                            if (col < collumns) continue;

                            row++;
                            col = 0;
                        }
                    }
                }

                var wallpaper =
                    await WinRtStorageHelper.CreateFileAsync("wallpaper.jpg", ApplicationData.Current.LocalFolder);
                using (var rndWrite = await wallpaper.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await destination.ToStreamAsJpeg(rndWrite);
                }

                App.Locator.AppSettingsHelper.WriteAsJson("WallpaperCreated", DateTime.Now);
                // If there are 30 or less albums then recreate in one day, else wait a week
                App.Locator.AppSettingsHelper.Write("WallpaperDayWait", albums.Count < 30 ? 1 : 7);

                BlurImageTool.SetSource(imageBrush, null);
                BlurImageTool.SetSource(imageBrush, "ms-appdata:/local/wallpaper.jpg");
            }

            _loaded = true;
        }

        private void CollectionPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongList != null && SongList.SelectionMode != ListViewSelectionMode.None)
                SongList.SelectionMode = ListViewSelectionMode.None;

            (Bar as CommandBar).Visibility =
                CollectionPivot.SelectedIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<NewPlaylistPage, ZoomOutTransition>(null);
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

                bar.Visibility = Visibility.Visible;
                SongList.IsItemClickEnabled = false;

                AppBarHelper.SaveState(bar);
                AppBarHelper.SwitchState(bar, _selectionModeCommands, _selectionSecondaryModeCommands);
            }
            else if (!SongList.IsItemClickEnabled)
            {
                HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
                UiBlockerUtility.Unblock();
                SongList.IsItemClickEnabled = true;
                (Bar as CommandBar).Visibility =
                    CollectionPivot.SelectedIndex == 3 ? Visibility.Visible : Visibility.Collapsed;

                AppBarHelper.RestorePreviousState(bar);
            }
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            SongList.SelectionMode = ListViewSelectionMode.None;
        }
    }
}