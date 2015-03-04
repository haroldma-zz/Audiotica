#region

using System.Collections.Generic;
using Windows.UI.Xaml;
using Audiotica.Controls.Home;
using GalaSoft.MvvmLight;

#endregion

namespace Audiotica.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private List<UIElement> _homeList;

        public MainViewModel()
        {
            HomeList = new List<UIElement> {new CollectionPeek(), new RecentlyAdded(), new MostPlayed(), new ArtistRecommendations()};
        }

        public List<UIElement> HomeList
        {
            get { return _homeList; }
            set { Set(ref _homeList, value); }
        }
    }
}