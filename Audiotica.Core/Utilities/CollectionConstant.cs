#region

using System;
using Windows.UI.Xaml.Media.Imaging;

#endregion

namespace Audiotica.Core.Utilities
{
    public static class CollectionConstant
    {
        public const string LocalStorageAppPath = "ms-appdata:///local/";
        public const string PackageAppPath = "ms-appx:///";

        public const string SongPath = "songs/{0}.mp3";
        public const string ArtworkPath = "artworks/{0}.jpg";
        public const string ArtistsArtworkPath = "artists/{0}.jpg";
        public const string MissingArtworkAppPath = PackageAppPath + "Assets/MissingArtwork.png";
        private static BitmapImage _missinArtworkImage;

        public static BitmapImage MissingArtworkImage
        {
            get
            {
                return _missinArtworkImage ??
                       (_missinArtworkImage = new BitmapImage(new Uri(MissingArtworkAppPath)));
            }
        }
    }
}