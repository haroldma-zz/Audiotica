using Audiotica.Core.Common;
using Audiotica.iOS.Implementations;

namespace Audiotica.iOS
{
    internal class AppConstant
    {
        public const string SongPath = "/Audiotica/{0}/{1}/{2}.mp3";

        public const string ArtworkPath = "/artworks/{0}.jpg";

        public const string ArtistsArtworkPath = "/artists/{0}.jpg";

        public const string SettinsShowLockScreenControls = "ShowLockScreenControls";

        private static IBitmapImage _missingArtwork;

        public static IBitmapImage MissingArtworkImage
        {
            get
            {
                if (_missingArtwork != null)
                {
                    return _missingArtwork;
                }

                var artwork = new PclBitmapImage();
                artwork.SetBundle("Resources/MissingArtwork.png");
                _missingArtwork = artwork;
                return _missingArtwork;
            }
        }
    }
}