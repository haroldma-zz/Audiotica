#region

using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GoogleAnalytics;

#endregion

namespace Audiotica
{
    public abstract class PageBase : Page
    {
        private const string StateKey = "State";

        private readonly NavigationHelper _navigationHelper;

        protected PageBase()
        {
            _navigationHelper = new NavigationHelper(this);
            NavigationCacheMode = NavigationCacheMode.Required;
            _navigationHelper.LoadState += NavigationHelperLoadState;
            _navigationHelper.SaveState += NavigationHelperSaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        protected virtual void LoadState(object state)
        {
        }

        protected void NavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            if (e.PageState != null
                && e.PageState.ContainsKey(StateKey))
            {
                LoadState(e.PageState[StateKey]);
            }
        }

        protected void NavigationHelperSaveState(object sender, SaveStateEventArgs e)
        {
            if (e.PageState == null)
            {
                throw new InvalidOperationException("PageState is null");
            }

            if (e.PageState.ContainsKey(StateKey))
            {
                e.PageState.Remove(StateKey);
            }

            var state = SaveState();

            if (state != null)
            {
                e.PageState.Add(StateKey, state);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedTo(e);

            var pageName = e.Content.ToString();
            pageName = pageName.Remove(0, pageName.LastIndexOf(".", StringComparison.Ordinal) + 1);
            EasyTracker.GetTracker().SendView(pageName);
        }

        protected virtual object SaveState()
        {
            return null;
        }
    }
}