using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Xamarin;

namespace Audiotica
{
    public static class ModalSheetUtility
    {
        private static IModalSheetPage _currentSheet;

        public static void Hide()
        {
            Hide(_currentSheet);
        }

        public static void Hide(IModalSheetPage sheet)
        {
            App.RootFrame.SizeChanged -= PageOnSizeChanged;

            #region Slide down animation

            var slideAnimation = new DoubleAnimation
            {
                From = 0,
                To = ((FrameworkElement) sheet).Height,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = new SineEase()
            };

            var sb = new Storyboard();
            sb.Children.Add(slideAnimation);
            Storyboard.SetTarget(slideAnimation, sheet.Popup);
            Storyboard.SetTargetProperty(slideAnimation, "(UIElement.RenderTransform).(TranslateTransform.Y)");

            sb.Completed += (sender, o) =>
            {
                sheet.Popup.IsOpen = false;
                sheet.Popup.Child = null;
                sheet.OnClosed();
                _currentSheet = null;
            };

            sb.Begin();

            #endregion
        }

        public static void Show(IModalSheetPage sheet)
        {
            if (_currentSheet != null) return;

            _currentSheet = sheet;
            var size = App.RootFrame;
            var element = (FrameworkElement) sheet;

            element.Width = size.ActualWidth;
            element.Height = size.ActualHeight;
            App.RootFrame.SizeChanged += PageOnSizeChanged;

            var popup = new Popup
            {
                IsOpen = true,
                Child = element,
                RenderTransform = new TranslateTransform
                {
                    Y = element.Height
                }
            };

            #region Slide up animation

            var slideAnimation = new DoubleAnimation
            {
                From = element.Height,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                EasingFunction = new SineEase()
            };

            var sb = new Storyboard();
            sb.Children.Add(slideAnimation);
            Storyboard.SetTarget(slideAnimation, popup);
            Storyboard.SetTargetProperty(slideAnimation, "(UIElement.RenderTransform).(TranslateTransform.Y)");

            sb.Begin();

            #endregion

            sheet.OnOpened(popup);
        }

        public static async Task<T> ShowAsync<T>(IModalSheetPageAsync<T> sheet)
        {
            UiBlockerUtility.BlockNavigation();
            Show(sheet);
            Insights.Track("Opened " + sheet.GetType().Name);
            var results = await sheet.GetResultsAsync();
            Hide(sheet);
            Insights.Track("Closed " + typeof(T).Name);
            UiBlockerUtility.Unblock();
            return results;
        }

        private static void PageOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var size = App.RootFrame;
            (_currentSheet as FrameworkElement).Width = size.ActualWidth;
            (_currentSheet as FrameworkElement).Height = size.ActualHeight;
        }
    }

    public interface IModalSheetPage
    {
        Popup Popup { get; }
        void OnClosed();
        void OnOpened(Popup popup);
    }

    public interface IModalSheetPageWithAction<T> : IModalSheetPage
    {
        Action<T> Action { get; set; }
    }

    public interface IModalSheetPageAsync<T> : IModalSheetPage
    {
        Task<T> GetResultsAsync();
    }
}