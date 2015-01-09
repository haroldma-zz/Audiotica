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
        public static void Show(IModalSheetPage sheet)
        {
            var size = App.RootFrame.DesiredSize;
            var page = sheet as Page;

            page.Width = size.Width;
            page.Height = size.Height;

            var popup = new Popup
            {
                IsOpen = true,
                Child = page,
                VerticalOffset = page.Height
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

        public static void Hide(IModalSheetPage sheet)
        {
            var page = sheet as Page;

            #region Slide down animation

            var slideAnimation = new DoubleAnimation
            {
                EnableDependentAnimation = true,
                From = 0,
                To = page.Height,
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