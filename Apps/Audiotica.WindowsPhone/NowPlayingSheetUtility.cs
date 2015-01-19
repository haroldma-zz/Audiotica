#region

using System;
using Windows.Phone.UI.Input;
using Audiotica.PartialView;

#endregion

namespace Audiotica
{
    public static class NowPlayingSheetUtility
    {
        private static NowPlayingSheet _currentSheet;

        public static async void OpenNowPlaying()
        {
            if (_currentSheet != null) return;

            _currentSheet = new NowPlayingSheet();

            UiBlockerUtility.BlockNavigation();

            ModalSheetUtility.Show(_currentSheet);

            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private static void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            UiBlockerUtility.Unblock();
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
            CloseNowPlaying();
        }

        public static void CloseNowPlaying()
        {
            if (_currentSheet == null) return;

            ModalSheetUtility.Hide(_currentSheet);
            _currentSheet = null;
        }
    }
}