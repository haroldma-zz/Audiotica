using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Audiotica.Core.Windows.Helpers
{
    public static class FlyoutEx
    {
        public static void ShowAttachedFlyoutAtPointer(Panel flyoutOwner)
        {
            ShowAttachedFlyoutAtPointer(flyoutOwner, flyoutOwner);
        }

        public static void ShowAttachedFlyoutAtPointer(FrameworkElement flyoutOwner, Panel rootPanel)
        {
            var point = PointerHelper.GetPosition();

            // if no pointer, display at the flyout owner
            if (point == null)
            {
                FlyoutBase.ShowAttachedFlyout(flyoutOwner);
                return;
            }

            var bounds = rootPanel.TransformToVisual(Window.Current.Content).TransformPoint(new Point(0, 0));

            var tempGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(point.Value.X - bounds.X, point.Value.Y - bounds.Y, 0, 0)
            };

            rootPanel.Children.Add(tempGrid);
            var flyout = FlyoutBase.GetAttachedFlyout(flyoutOwner);
            EventHandler<object> handler = null;
            handler = (o, o1) =>
            {
                rootPanel.Children.Remove(tempGrid);
                flyout.Closed -= handler;
            };
            flyout.Closed += handler;
            flyout.ShowAt(tempGrid);
        }
    }
}