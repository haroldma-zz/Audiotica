using System;

using Audiotica.iOS.DataDources;

using UIKit;

namespace Audiotica.iOS
{
    /// <summary>
    /// Class SpotlightController.
    /// </summary>
    public partial class SpotlightController : UICollectionViewController
    {
        private MostStreamedDataSource _source;

        public SpotlightController(IntPtr handle)
            : base(handle)
        {
        }

        public override async void ViewDidLoad()
        {
            this.RefreshButton.Clicked += this.RefreshButtonOnClicked;

            this._source = new MostStreamedDataSource();
            this.CollectionView.Source = this._source;
            await this._source.LoadDataAsync();
            this.CollectionView.ReloadData();
        }

        private async void RefreshButtonOnClicked(object sender, EventArgs eventArgs)
        {
            this._source.MostStreamed.Clear();
            this.CollectionView.ReloadData();

            await this._source.LoadDataAsync();
            this.CollectionView.ReloadData();
        }
    }
}