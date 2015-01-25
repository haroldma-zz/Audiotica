#region

using System;
using Android.App;
using Android.Graphics;
using Audiotica.Android.Implementations;
using Audiotica.Core.Common;

#endregion

namespace Audiotica.Android
{
    public static class AppConstant
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
                    return _missingArtwork;
                var artwork = new PclBitmapImage();
                artwork.SetResource(Application.Context.Resources, Resource.Drawable.MissingArtwork);
                _missingArtwork = artwork;
                return _missingArtwork;
            }
        }
    }
}