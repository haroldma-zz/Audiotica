using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Audiotica.Core.Utilities
{
    public static class CollectionConstant
    {
        private static BitmapImage _missinArtworkImage;
        public const string LocalStorageAppPath = "ms-appdata:///local/";
        public const string PackageAppPath = "ms-appx:///";

        public const string SongPath = "songs/{0}.mp3";
        public const string ArtworkPath = "artworks/{0}.jpg";
        public const string MissingArtworkAppPath = PackageAppPath + "Assets/MissingArtwork.png";

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
