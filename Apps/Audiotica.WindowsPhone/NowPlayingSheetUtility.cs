#region

using Windows.Phone.UI.Input;
using Audiotica.PartialView;
using Xamarin;

#endregion

namespace Audiotica
{
    public static class NowPlayingSheetUtility
    {
        private static NowPlayingSheet _currentSheet;

        public static void OpenNowPlaying()
        {
            if (_currentSheet != null) return;

            _currentSheet = new NowPlayingSheet();

            UiBlockerUtility.BlockNavigation();

            ModalSheetUtility.Show(_currentSheet);

            App.SupressBackEvent += HardwareButtonsOnBackPressed;

            ScreenTimeoutHelper.OnNowPlayingOpened();
            Insights.Track("Opened Now Playing");
        }

        private static void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressed;
            CloseNowPlaying();
        }

        public static void CloseNowPlaying()
        {
            if (_currentSheet == null) return;

            ModalSheetUtility.Hide(_currentSheet);
            _currentSheet = null;

            ScreenTimeoutHelper.OnNowPlayingClosed();
            Insights.Track("Closed Now Playing");
        }
    }
}