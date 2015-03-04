using System;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Audiotica.Core.Utils;
using Audiotica.View;

namespace Audiotica.Controls.Home
{
    public sealed partial class CollectionPeek
    {
        public CollectionPeek()
        {
            InitializeComponent();
            Loaded += CollectionPeek_Loaded;
        }

        private async void CollectionPeek_Loaded(object sender, RoutedEventArgs e)
        {
            // Get a random artist image, if there is any
            var artists = await StorageHelper.GetFolderAsync("artists");

            if (artists == null)
            {
                // Hook up when an artist is added to re run this
                App.Locator.CollectionService.Artists.CollectionChanged += ArtistsOnCollectionChanged;
                return;
            }

            var files = await artists.GetFilesAsync();

            if (files.Count <= 0)
            {
                App.Locator.CollectionService.Artists.CollectionChanged += ArtistsOnCollectionChanged;
                return;
            }

            var rnd = new Random();
            var index = files.Count == 1 ? 1 : rnd.Next(1, files.Count);

            var randomFile = files[index - 1];

            var imageBrush = new ImageBrush(){Stretch = Stretch.UniformToFill, Opacity = .5};
            BlurImageTool.SetSource(imageBrush, randomFile.Path);

            LayoutGrid.Background = imageBrush;
        }

        private void ArtistsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            App.Locator.CollectionService.Artists.CollectionChanged -= ArtistsOnCollectionChanged;
            CollectionPeek_Loaded(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Navigator.GoTo<CollectionPage, ZoomInTransition>(null);
        }
    }
}