﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Utilities;
using Audiotica.Data.Collection.Model;

#endregion

namespace Audiotica.View
{
    public sealed partial class CollectionPage
    {
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> _delegate;

        public CollectionPage()
        {
            InitializeComponent();
            Bar = BottomAppBar;
            BottomAppBar = null;
            Loaded += (sender, args) => LoadWallpaperArt();
        }

        /// <summary>
        ///     Managing delegate creation to ensure we instantiate a single instance for
        ///     optimal performance.
        /// </summary>
        private TypedEventHandler<ListViewBase, ContainerContentChangingEventArgs> ContainerContentChangingDelegate
        {
            get { return _delegate ?? (_delegate = ItemListView_ContainerContentChanging); }
        }

        public override void NavigatedTo(object parameter)
        {
            base.NavigatedTo(parameter);
            if (parameter == null) return;

            LoadWallpaperArt();

            var pivotIndex = (int) parameter;
            CollectionPivot.SelectedIndex = pivotIndex;
        }

        private async void LoadWallpaperArt()
        {
            var vm = App.Locator.Collection;

            if (vm.RandomizeAlbumList.Count != 0 ||
                !AppSettingsHelper.Read("WallpaperArt", true, SettingsStrategy.Roaming)) return;

            var albums =
                App.Locator.CollectionService.Albums.ToList()
                    .Where(p => p.Artwork != CollectionConstant.MissingArtworkImage)
                    .ToList();

            var albumCount = albums.Count;

            if (albumCount < 10) return;

            var h = Window.Current.Bounds.Height;
            var rows = (int) Math.Ceiling(h/(ActualWidth/5));

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

            vm.RandomizeAlbumList.AddRange(shuffle);
        }

        private void CollectionPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
    }
}