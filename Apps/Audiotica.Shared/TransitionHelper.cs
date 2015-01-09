using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Audiotica
{
    public static class TransitionHelper
    {
        public static void Hide(FrameworkElement element)
        {
            Canvas.SetTop(element, 0);
            Canvas.SetLeft(element, 0);
            element.Visibility = Visibility.Collapsed;
            element.Opacity = 1;
        }

        public static void Show(FrameworkElement element)
        {
            Canvas.SetTop(element, 0);
            Canvas.SetLeft(element, 0);
            element.Visibility = Visibility.Visible;
            element.Opacity = 1;
        }
    }
}
