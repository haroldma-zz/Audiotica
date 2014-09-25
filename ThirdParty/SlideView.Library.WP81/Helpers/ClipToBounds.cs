#if WINDOWS_PHONE
using System.Windows.Media;
using System.Windows;
#elif NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

namespace SlideView.Library.Helpers
{
    public static class ClipToBounds
    {
        public static bool GetClipToBounds(DependencyObject obj)
        {
            return (bool)obj.GetValue(ClipToBoundsProperty);
        }

        public static void SetClipToBounds(DependencyObject obj, bool value)
        {
            obj.SetValue(ClipToBoundsProperty, value);
        }

        public static readonly DependencyProperty ClipToBoundsProperty =
            DependencyProperty.RegisterAttached(
                "ClipToBounds",
                typeof(bool),
                typeof(ClipToBounds),
                new PropertyMetadata(false, OnClipToBoundsChanged)
                );

        private static void OnClipToBoundsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as FrameworkElement;

            if (element != null)
            {
                if ((bool)e.NewValue)
                    element.SizeChanged += new SizeChangedEventHandler(Element_SizeChanged);
                else
                    element.SizeChanged -= new SizeChangedEventHandler(Element_SizeChanged);
            }
        }

        static void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var element = sender as FrameworkElement;

            UpdateClipSize(element, e.NewSize);
        }
        
        public static void UpdateClipSize(FrameworkElement element, Size clipSize)
        {
            if (element != null)
            {
                RectangleGeometry clipRectangle = null;

                if (element.Clip == null)
                {
                    clipRectangle = new RectangleGeometry();
                    element.Clip = clipRectangle;
                }
                else
                {
                    if (element.Clip is RectangleGeometry)
                    {
                        clipRectangle = (RectangleGeometry)element.Clip;
                    }
                }

                if (clipRectangle != null)
                {
                    clipRectangle.Rect = new Rect(new Point(0, 0), clipSize);
                }
            }
        }
    }
}
