using System;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Audiotica
{
    public static class UiBlockerUtility
    {
        private static Popup _popup;
        private static CommandBar _commandBar;
        private static Visibility _commandPrevVisibility;

        public static bool SupressBackEvents { get; private set; }

        public static void BlockNavigation(bool hideNavBar = true)
        {
            SupressBackEvents = true;

            if (!hideNavBar) return;

            _commandBar = (App.RootFrame.Content as Page).BottomAppBar as CommandBar;

            if (_commandBar != null)
            {
                _commandPrevVisibility = _commandBar.Visibility;
                _commandBar.Visibility = Visibility.Collapsed;
            }
        }

        public static void Block(string message)
        {
            StatusBarHelper.ShowStatus(message);

           if (_popup != null) return;

            const double opacity = 255 * 0.8;

            var size = Window.Current.Bounds;
            var grid = new Grid()
            {
                Width = size.Width,
                Height = size.Height,
                Background = new SolidColorBrush(Color.FromArgb(Convert.ToByte(opacity), 0x0, 0x0, 0x0))
            };
            _popup = new Popup()
            {
                Child = grid,
                IsOpen = true
            };
            BlockNavigation();
        }

        public static void Unblock()
        {
            SupressBackEvents = false;

            if (_commandBar != null)
            {
                _commandBar.Visibility = _commandPrevVisibility;
                _commandBar = null;
            }

            if (_popup == null) return;

            _popup.IsOpen = false;
            _popup = null;
            StatusBarHelper.HideStatus();
        }
    }
}
