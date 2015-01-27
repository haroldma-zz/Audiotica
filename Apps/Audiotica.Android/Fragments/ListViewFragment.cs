using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Audiotica.Android.Fragments
{
    public class ListViewFragment : Fragment
    {
        private ViewGroup _mRootView;
        private ListView _mListView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _mRootView = (ViewGroup)inflater.Inflate(Resource.Layout.fragment_list_view, container, false);

            _mListView = (ListView)_mRootView.FindViewById(Resource.Id.generalListView);
            _mListView.FastScrollEnabled = true;

            return null;
        }
    }
}