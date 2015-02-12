using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Xamarin;

namespace Audiotica
{
    public static class ModalSheetUtility
    {
        private static IModalSheetPage currentSheet;

        public static void Hide()
        {
            Hide(currentSheet);
        }

        public static void Hide(IModalSheetPage sheet)
        {
            App.RootFrame.SizeChanged -= PageOnSizeChanged;

            #region Slide down animation

            var slideAnimation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                From = 0,
                To = (currentSheet as FrameworkElement).Height,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };

            var sb = new Storyboard();
            sb.Children.Add(slideAnimation);
            Storyboard.SetTarget(slideAnimation, sheet.Popup);
            Storyboard.SetTargetProperty(slideAnimation, "VerticalOffset");

            sb.Completed += (sender, o) =>
            {
                sheet.Popup.IsOpen = false;
                sheet.OnClosed();
                currentSheet = null;
            };

            sb.Begin();

            #endregion
        }

        public static void Show(IModalSheetPage sheet)
        {
            if (currentSheet != null) return;

            currentSheet = sheet;
            var size = App.RootFrame;
            var element = sheet as FrameworkElement;

            element.Width = size.ActualWidth;
            element.Height = size.ActualHeight;
            App.RootFrame.SizeChanged += PageOnSizeChanged;

            var popup = new Popup
            {
                IsOpen = true,
                Child = element,
                VerticalOffset = element.Height
            };

            #region Slide up animation

            var slideAnimation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                From = popup.VerticalOffset,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };

            var sb = new Storyboard();
            sb.Children.Add(slideAnimation);
            Storyboard.SetTarget(slideAnimation, popup);
            Storyboard.SetTargetProperty(slideAnimation, "VerticalOffset");

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
            (currentSheet as FrameworkElement).Width = size.ActualWidth;
            (currentSheet as FrameworkElement).Height = size.ActualHeight;
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