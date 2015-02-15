using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Audiotica
{
    public static class TransitionHelper
    {
        public static void Hide(FrameworkElement element)
        {
            // just in case, resetting scale
            element.RenderTransform = new ScaleTransform();
            Canvas.SetTop(element, 0);
            Canvas.SetLeft(element, 0);
            element.Visibility = Visibility.Collapsed;
            element.Opacity = 1;
        }

        public static void Show(FrameworkElement element)
        {
            // just in case, resetting scale
            element.RenderTransform = new ScaleTransform();
            Canvas.SetTop(element, 0);
            Canvas.SetLeft(element, 0);
            element.Visibility = Visibility.Visible;
            element.Opacity = 1;
        }
    }
}