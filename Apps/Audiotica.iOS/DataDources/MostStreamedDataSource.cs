using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Audiotica.Data.Spotify.Models;

using Foundation;

using UIKit;

namespace Audiotica.iOS.DataDources
{
    public class MostStreamedDataSource : UICollectionViewSource
    {
        public MostStreamedDataSource()
        {
            this.MostStreamed = new List<ChartTrack>();
        }

        public List<ChartTrack> MostStreamed { get; set; }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell =
                (SpotlightCollectionCell)collectionView.DequeueReusableCell(SpotlightCollectionCell.CellId, indexPath);

            var song = this.MostStreamed[indexPath.Row];

            cell.UpdateData(song);

            return cell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return this.MostStreamed.Count;
        }

        public async Task LoadDataAsync()
        {
            var data = await AppDelegate.Current.Locator.SpotifyService.GetMostStreamedTracksAsync();
            this.MostStreamed = data.Take(20).ToList();
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }
    }
}