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

        public const string SongPath = "Audiotica/{0}/{1}/{2}.mp3";
        public const string ArtworkPath = "artworks/{0}.jpg";
        public const string ArtistsArtworkPath = "artists/{0}.jpg";

        private static IBitmapImage _missingArtwork;
        public static IBitmapImage MissingArtworkImage
        {
            get
            {
                return _missingArtwork ?? (_missingArtwork = new PclBitmapImage(new Uri(MissingArtworkAppPath)));
            }
        }
    }
}