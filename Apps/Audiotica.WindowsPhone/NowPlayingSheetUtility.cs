#region

using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml.Controls;
using Audiotica.PartialView;
using Xamarin;

#endregion

namespace Audiotica
{
    public static class NowPlayingSheetUtility
    {
        static NowPlayingSheetUtility()
        {
            CurrentSheet = new NowPlayingSheet();
        }

        private static readonly NowPlayingSheet CurrentSheet;

        public static void OpenNowPlaying()
        {
            UiBlockerUtility.BlockNavigation();

            ModalSheetUtility.Show(CurrentSheet);

            App.SupressBackEvent += HardwareButtonsOnBackPressed;

            ScreenTimeoutHelper.OnNowPlayingOpened();
            Insights.Track("Opened Now Playing");
            IsActive = true;
        }

        private static void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            App.SupressBackEvent -= HardwareButtonsOnBackPressed;
            CloseNowPlaying();
        }

        public static bool IsActive;

        public static void CloseNowPlaying()
        {
            if (!IsActive) return;

            ModalSheetUtility.Hide(CurrentSheet);

            ScreenTimeoutHelper.OnNowPlayingClosed();
            Insights.Track("Closed Now Playing");
        }
    }
}