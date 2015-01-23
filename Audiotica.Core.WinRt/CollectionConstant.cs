#region

using System;
using Audiotica.Core.Common;

#endregion

namespace Audiotica.Core.WinRt
{
    public static class CollectionConstant
    {
        public const string LocalStorageAppPath = "ms-appdata:///local/";
        public const string PackageAppPath = "ms-appx:///";
        public const string MissingArtworkAppPath = PackageAppPath + "Assets/MissingArtwork.png";

        public const string SongPath = "songs/{0}.mp3";
        public const string ArtworkPath = "artworks/{0}.jpg";
        public const string ArtistsArtworkPath = "artists/{0}.jpg";

        public static IBitmapImage MissingArtworkImage
        {
            get
            {
                return new PclBitmapImage(new Uri(MissingArtworkAppPath));
            }
        }
    }
}