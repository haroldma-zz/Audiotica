#region

using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GoogleAnalytics;

#endregion

namespace Audiotica
{
    public abstract class PageBase : Page
    {
        private const string StateKey = "State";

        public AppBar Bar
        {
            get;
            protected set;
        }

        public virtual void NavigatedFrom()
        {
            _navigatedAway = true;
        }

        public virtual void BeforeNavigateTo()
        {
            _navigatedAway = false;
        }

        public virtual void NavigatedTo(Object parameter)
        {
            var pageName = "HomePage";

            if (App.Navigator.CurrentPage != null)
            {
                pageName = App.Navigator.CurrentPage.ToString();
                pageName = pageName.Remove(0, pageName.LastIndexOf(".", StringComparison.Ordinal) + 1);
            }
            EasyTracker.GetTracker().SendView(pageName);
        }



        private bool _navigatedAway;
        public void SetSize(Size size)
        {
            if (_navigatedAway) return;

            Width = size.Width;
            Height = size.Height;
            Measure(size);
        }
    }
}