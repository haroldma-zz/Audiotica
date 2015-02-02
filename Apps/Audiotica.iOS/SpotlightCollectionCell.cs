using System;
using System.ComponentModel;

using Audiotica.Data.Spotify.Models;
using Audiotica.iOS.Implementations;

using UIKit;

namespace Audiotica.iOS
{
    /// <summary>
    /// Class SpotlightCollectionCell.
    /// </summary>
    public partial class SpotlightCollectionCell : UICollectionViewCell
    {
        public const string CellId = "SpotlightCollectionCell";

        private PclBitmapImage _currentBitmap;

        public SpotlightCollectionCell(IntPtr handle)
            : base(handle)
        {
        }

        public void UpdateData(ChartTrack track)
        {
            if (this._currentBitmap != null)
            {
                this._currentBitmap.PropertyChanged -= this.CurrentBitmapOnPropertyChanged;
            }

            this._currentBitmap = new PclBitmapImage(new Uri(track.ArtworkUrl));
            if (this._currentBitmap.Image == null)
            {
                this._currentBitmap.PropertyChanged += this.CurrentBitmapOnPropertyChanged;
            }
            else
            {
                this.ArtworkImage.Image = this._currentBitmap.Image as UIImage;
            }
        }

        private void CurrentBitmapOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.ArtworkImage.Image = this._currentBitmap.Image as UIImage;
        }
    }
}