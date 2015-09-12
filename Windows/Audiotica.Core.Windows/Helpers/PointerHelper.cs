using System;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Audiotica.Core.Windows.Helpers
{
    public static class PointerHelper
    {
        public static Point? GetPosition()
        {
            var currentWindow = Window.Current;

            try
            {
                var point = currentWindow.CoreWindow.PointerPosition;
                var bounds = currentWindow.Bounds;
                return new Point(point.X - bounds.X, point.Y - bounds.Y);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

        }
    }
}