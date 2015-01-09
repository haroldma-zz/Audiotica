#region

using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

#endregion

namespace Audiotica
{
    public static class ModalSheetUtility
    {
        private static Page _page;

        public static void Show(IModalSheetPage sheet)
        {
            if (_page != null) return;

            var size = App.RootFrame;
            _page = sheet as Page;

            _page.Width = size.ActualWidth;
            _page.Height = size.ActualHeight;
            App.RootFrame.SizeChanged += PageOnSizeChanged;

            var popup = new Popup
            {
                IsOpen = true,
                Child = _page,
                VerticalOffset = _page.Height
            };

            #region Slide up animation

            var slideAnimation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                From = popup.VerticalOffset,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };

            var sb = new Storyboard();
            sb.Children.Add(slideAnimation);
            Storyboard.SetTarget(slideAnimation, popup);
            Storyboard.SetTargetProperty(slideAnimation, "VerticalOffset");

            sb.Begin();

            #endregion

             
            sheet.OnOpened(popup);
        }

        private static void PageOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var size = App.RootFrame;
            _page.Width = size.ActualWidth;
            _page.Height = size.ActualHeight;
        }

        public static void Hide(IModalSheetPage sheet)
        {
            App.RootFrame.SizeChanged -= PageOnSizeChanged;

            #region Slide down animation

            var slideAnimation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                From = 0,
                To = _page.Height,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };

            var sb = new Storyboard();
            sb.Children.Add(slideAnimation);
            Storyboard.SetTarget(slideAnimation, sheet.Popup);
            Storyboard.SetTargetProperty(slideAnimation, "VerticalOffset");

            sb.Completed += (sender, o) =>
            {
                sheet.Popup.IsOpen = false;
                sheet.OnClosed();
                _page = null;
            };

            sb.Begin();

            #endregion
        }
    }

    public interface IModalSheetPage
    {
        Popup Popup { get; }
        void OnOpened(Popup popup);
        void OnClosed();
    }
}