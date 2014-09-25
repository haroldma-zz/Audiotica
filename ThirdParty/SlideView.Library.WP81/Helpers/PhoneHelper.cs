using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
#if NETFX_CORE
using Windows.UI.Xaml.Controls;
#else
using Microsoft.Phone.Controls;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
#endif

namespace SlideView.Library.Helpers
{
    internal static class PhoneHelper
    {
        public static bool TryGetFrame(out Frame Frame)
        {
            Frame = Window.Current.Content as Frame;
            return Frame != null;
        }

        public static bool IsPortrait()
        {
            var applicationView = ApplicationView.GetForCurrentView();
            return applicationView.Orientation == ApplicationViewOrientation.Portrait;
        }

        public static double GetUsefullWidth()
        {
            Frame phoneAppFrame = null;
            if (TryGetFrame(out phoneAppFrame))
            {
                return phoneAppFrame.GetUsefulWidth();
            }

            return Window.Current.Bounds.Width;
        }

        public static double GetUsefulHeight()
        {
            Frame phoneAppFrame = null;
            if (TryGetFrame(out phoneAppFrame))
            {
                return phoneAppFrame.GetUsefulHeight();
            }

            return Window.Current.Bounds.Height;
        }

        public static Size GetUsefulSize(this Size initialSize)
        {
            if (IsPortrait())
                return initialSize;

            return new Size(initialSize.Height, initialSize.Width);
        }

        public static double GetUsefulWidth(this Frame Frame)
        {
            return IsPortrait() ? Frame.ActualWidth : Frame.ActualHeight;
        }

        public static double GetUsefulHeight(this Frame Frame)
        {
            return IsPortrait() ? Frame.ActualHeight : Frame.ActualWidth;
        }
    }
}
