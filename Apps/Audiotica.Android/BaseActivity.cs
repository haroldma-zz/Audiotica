using Android.App;
using Android.OS;

namespace Audiotica.Android
{
    public class BaseActivity : Activity
    {
		protected override void OnCreate(Bundle bundle)
		{
			base.OnResume();
			App.Current.CurrentActivity = this;
		}

        protected override void OnResume()
        {
            base.OnResume();
            App.Current.CurrentActivity = this;
        }

        protected override void OnPause()
        {
			base.OnPause();
			ClearReferences();
        }

        protected override void OnDestroy()
        {
			base.OnDestroy();
			ClearReferences();
        }

        private void ClearReferences()
        {
            var currActivity = App.Current.CurrentActivity;
            if (currActivity != null && currActivity.Equals(this))
                App.Current.CurrentActivity = null;
        }
    }
}